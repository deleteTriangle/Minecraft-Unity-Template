using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class RigidbodyController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.3f;
    public float sprintSpeed = 5.6f;
    public float crouchSpeed = 1.3f;

    [Header("Jump")]
    public float jumpHeightInBlocks = 1.25f;
    public float gravity = 20f;
    public float fallMultiplier = 2.5f;

    [Header("Step")]
    public float stepHeight = 0.6f;        // максимальная высота ступеньки (чуть больше полублока)
    public float stepSmooth = 10f;   
    public float snapDownDistance = 0.6f;

    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 0.15f;
    public float maxLookAngle = 90f;
    public bool invertCamera = false;
    public float fov = 70f;
    public float sprintFOV = 75f;
    public float sprintFOVStepTime = 10f;

    [Header("Zoom")]
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public float zoomFOV = 30f;
    public float zoomStepTime = 10f;

    [Header("Crouch")]
    public float normalHeight = 1.8f;
    public float crouchHeight = 1.5f;
    public float normalCameraY = 0.8f;
    public float crouchCameraY = 0.6f;
    public bool holdToCrouch = false;

    [Header("Sprint")]
    public bool enableSprint = true;
    public bool unlimitedSprint = true;
    public float sprintDuration = 5f;
    public float sprintCooldown = 2f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(0.05f, 0.05f, 0f);

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckDistance = 0.05f;

    // ─── Компоненты ────────────────────────────────────────────────────────────
    private Rigidbody rb;
    private BoxCollider boxCollider;
    private InputSystem_Actions inputActions;

    // ─── Состояние ─────────────────────────────────────────────────────────────
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouched;
    private bool isWalking;
    private bool isZoomed;
    private bool jumpRequested;
    private bool isSprintCooldown;

    private float sprintRemaining;
    private float sprintCooldownTimer;
    private float pitch = 0f;
    private float yaw = 0f;
    private float bobTimer = 0f;
    private Vector3 jointOriginalPos;

    // Нижняя точка коллайдера в мировых координатах
    private Vector3 ColliderBottom => transform.position + new Vector3(0f, 0f, 0f);

    private float JumpPower => Mathf.Sqrt(2f * gravity * jumpHeightInBlocks);

    // ─── Init ──────────────────────────────────────────────────────────────────
    public void Init()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        inputActions = G.PlayerInput.InputActions;

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        boxCollider.size = new Vector3(0.58f, normalHeight, 0.58f);
        //boxCollider.size = new Vector3(0.6f, normalHeight, 0.6f);
        boxCollider.center = new Vector3(0f, normalHeight / 2f, 0f);
        
        PhysicsMaterial slippery = new PhysicsMaterial();
        slippery.staticFriction = 0f;
        slippery.dynamicFriction = 0f;
        slippery.frictionCombine = PhysicsMaterialCombine.Minimum;
        slippery.bounciness = 0f;
        slippery.bounceCombine = PhysicsMaterialCombine.Minimum;
        boxCollider.material = slippery;

        Physics.defaultContactOffset = 0.01f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        playerCamera.fieldOfView = fov;
        playerCamera.transform.localPosition = new Vector3(0f, normalCameraY, 0f);

        if (joint != null)
            jointOriginalPos = joint.localPosition;

        sprintRemaining = sprintDuration;
        sprintCooldownTimer = sprintCooldown;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ─── Update ────────────────────────────────────────────────────────────────
    private void Update()
    {
        HandleCamera();
        HandleZoom();
        HandleCrouch();
        HandleFOV();

        if (enableHeadBob && joint != null)
            HeadBob();

        if (inputActions.Player.Jump.WasPressedThisFrame() && isGrounded && !jumpRequested)
        {
            jumpRequested = true;
        }
    }

    // ─── FixedUpdate ───────────────────────────────────────────────────────────
    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        ApplyGravity();
        HandleStep();
        HandleSnapDown();
    }
    
    // ─── Земля ─────────────────────────────────────────────────────────────────
    private void CheckGround()
    {
        float skinWidth = 0.02f;
        float checkY = transform.position.y + skinWidth;
        float radius = boxCollider.size.x / 2f - 0.02f;

        // Центр + 4 угла по краям коллайдера
        float rayDistance = groundCheckDistance + skinWidth * 2f;
        Vector3 position = transform.position;

        isGrounded =
            Physics.Raycast(new Vector3(position.x, checkY, position.z), Vector3.down, rayDistance, groundMask) ||
            Physics.Raycast(new Vector3(position.x + radius, checkY, position.z + radius), Vector3.down, rayDistance, groundMask) ||
            Physics.Raycast(new Vector3(position.x - radius, checkY, position.z + radius), Vector3.down, rayDistance, groundMask) ||
            Physics.Raycast(new Vector3(position.x + radius, checkY, position.z - radius), Vector3.down, rayDistance, groundMask) ||
            Physics.Raycast(new Vector3(position.x - radius, checkY, position.z - radius), Vector3.down, rayDistance, groundMask);
    }

    // ─── Движение ──────────────────────────────────────────────────────────────
    private void HandleMovement()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = (transform.right * input.x + transform.forward * input.y).normalized;

        // Спринт
        bool wantSprint = enableSprint
            && inputActions.Player.Sprint.IsPressed()
            && input.y > 0.1f
            && !isCrouched
            && !isSprintCooldown;

        if (wantSprint && (unlimitedSprint || sprintRemaining > 0f))
        {
            isSprinting = true;

            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.fixedDeltaTime;
                if (sprintRemaining <= 0f)
                {
                    isSprinting = false;
                    isSprintCooldown = true;
                    sprintCooldownTimer = sprintCooldown;
                }
            }
        }
        else
        {
            isSprinting = false;

            if (!unlimitedSprint)
                sprintRemaining = Mathf.Clamp(sprintRemaining + Time.fixedDeltaTime, 0f, sprintDuration);

            if (isSprintCooldown)
            {
                sprintCooldownTimer -= Time.fixedDeltaTime;
                if (sprintCooldownTimer <= 0f)
                    isSprintCooldown = false;
            }
        }

        float currentSpeed = isCrouched ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 targetVelocity = moveDir * currentSpeed;

        isWalking = moveDir.magnitude > 0.1f && isGrounded;

        // Прыжок
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, JumpPower, rb.linearVelocity.z);
            jumpRequested = false;
            isGrounded = false;
        }

        if (isCrouched && isGrounded)
        {
            float colliderBottom = transform.position.y + boxCollider.center.y - boxCollider.size.y / 2f;
            float checkY = colliderBottom + 0.05f;
            float checkDist = 0.3f;

            Vector3 checkX = transform.position + new Vector3(moveDir.x, 0f, 0f) * checkDist;
            Vector3 checkZ = transform.position + new Vector3(0f, 0f, moveDir.z) * checkDist;

            bool hasGroundX = Physics.Raycast(new Vector3(checkX.x, checkY, checkX.z), Vector3.down, 0.15f, groundMask);
            bool hasGroundZ = Physics.Raycast(new Vector3(checkZ.x, checkY, checkZ.z), Vector3.down, 0.15f, groundMask);

            if (!hasGroundX) targetVelocity.x = 0f;
            if (!hasGroundZ) targetVelocity.z = 0f;
        }
        
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    // ─── Гравитация ────────────────────────────────────────────────────────────
    private void ApplyGravity()
    {
        if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
            return;
        }

        float multiplier = rb.linearVelocity.y < 0f ? fallMultiplier : 1f;
        rb.linearVelocity += Vector3.down * (gravity * multiplier * Time.fixedDeltaTime);
    }

    // ─── Ступеньки ─────────────────────────────────────────────────────────────
    private void HandleStep()
    {
        if (!isGrounded || rb.linearVelocity.magnitude < 0.1f) return;

        Vector3 moveDir = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized;
        if (moveDir.magnitude < 0.1f) return;

        float radius = boxCollider.size.x / 2f;
        float colliderBottom = transform.position.y + boxCollider.center.y - boxCollider.size.y / 2f;

        if (TryStep(moveDir, radius, colliderBottom)) return;
        if (TryStep(Quaternion.Euler(0, 30, 0) * moveDir, radius, colliderBottom)) return;
        if (TryStep(Quaternion.Euler(0, -30, 0) * moveDir, radius, colliderBottom)) return;
        if (TryStep(Quaternion.Euler(0, 60, 0) * moveDir, radius, colliderBottom)) return;
        TryStep(Quaternion.Euler(0, -60, 0) * moveDir, radius, colliderBottom);
    }

    private bool TryStep(Vector3 dir, float radius, float colliderBottom)
    {
        Vector3 position = transform.position;
        Vector3 lowOrigin = new Vector3(position.x, colliderBottom + 0.05f, position.z);
        Vector3 highOrigin = new Vector3(position.x, colliderBottom + stepHeight + 0.05f, position.z);

        bool blockedLow = Physics.Raycast(lowOrigin, dir, radius + 0.15f, groundMask);
        bool blockedHigh = Physics.Raycast(highOrigin, dir, radius + 0.15f, groundMask);

        if (!blockedLow || blockedHigh) return false;

        Vector3 checkPos = position + dir * (radius + 0.15f) + Vector3.up * (stepHeight + 0.05f);
        if (!Physics.Raycast(checkPos, Vector3.down, out RaycastHit hit, stepHeight + 0.1f, groundMask)) return false;

        float stepUp = hit.point.y - colliderBottom;
        if (stepUp < 0.01f || stepUp > stepHeight) return false;

        transform.position = new Vector3(position.x, position.y + stepUp + 0.02f, position.z);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Physics.SyncTransforms();
        return true;
    }

    private void HandleSnapDown()
    {
        // Не спускаемся если поднимаемся
        if (rb.linearVelocity.y > 0f) return;
        if (isGrounded) return;
        if (new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude < 0.1f) return;

        float colliderBottom = transform.position.y + boxCollider.center.y - boxCollider.size.y / 2f;

        if (Physics.Raycast(
            new Vector3(transform.position.x, colliderBottom, transform.position.z),
            Vector3.down, out RaycastHit hit, stepHeight, groundMask))
        {
            float snapDown = colliderBottom - hit.point.y;
            if (snapDown < 0.02f) return;

            // Задаём скорость вниз вместо прямого изменения transform
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -snapDown / Time.fixedDeltaTime, rb.linearVelocity.z);
        }
    }
    
    // ─── Камера ────────────────────────────────────────────────────────────────
    private void HandleCamera()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity;
        float mouseY = invertCamera ? mouseDelta.y : -mouseDelta.y;
        pitch += mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    // ─── Зум ───────────────────────────────────────────────────────────────────
    private void HandleZoom()
    {
        if (!enableZoom) return;

        var zoomKey = inputActions.Player.Zoom;

        if (!holdToZoom)
        {
            if (zoomKey.WasPressedThisFrame() && !isSprinting)
                isZoomed = !isZoomed;
        }
        else
        {
            isZoomed = zoomKey.IsPressed() && !isSprinting;
        }

        if (isSprinting) isZoomed = false;
    }

    // ─── FOV ───────────────────────────────────────────────────────────────────
    private void HandleFOV()
    {
        float targetFOV;

        if (isSprinting)
            targetFOV = sprintFOV;
        else if (isZoomed)
            targetFOV = zoomFOV;
        else
            targetFOV = fov;

        float stepTime = isZoomed ? zoomStepTime : sprintFOVStepTime;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, stepTime * Time.deltaTime);
    }

    // ─── Присед ────────────────────────────────────────────────────────────────
    private void HandleCrouch()
    {
        var crouchKey = inputActions.Player.Crouch;

        if (!holdToCrouch)
        {
            if (crouchKey.WasPressedThisFrame())
                SetCrouch(!isCrouched);
        }
        else
        {
            if (crouchKey.WasPressedThisFrame()) SetCrouch(true);
            if (crouchKey.WasReleasedThisFrame()) SetCrouch(false);
        }
    }

    private void SetCrouch(bool crouch)
    {
        if (!crouch && CeilingCheck()) return;

        isCrouched = crouch;
        float height = crouch ? crouchHeight : normalHeight;

        boxCollider.size = new Vector3(0.6f, height, 0.6f);
        boxCollider.center = new Vector3(0f, height / 2f, 0f);

        float targetCamY = crouch ? crouchCameraY : normalCameraY;
        StopAllCoroutines();
        StartCoroutine(SmoothCameraHeight(targetCamY));
    }

    private bool CeilingCheck()
    {
        Vector3 origin = transform.position + new Vector3(0f, crouchHeight, 0f);
        return Physics.Raycast(origin, Vector3.up, normalHeight - crouchHeight + 0.1f, groundMask);
    }

    private IEnumerator SmoothCameraHeight(float targetY)
    {
        Vector3 start = playerCamera.transform.localPosition;
        Vector3 end = new Vector3(start.x, targetY, start.z);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            playerCamera.transform.localPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }

        playerCamera.transform.localPosition = end;
    }

    // ─── Боббинг ───────────────────────────────────────────────────────────────
    private void HeadBob()
    {
        if (isWalking)
        {
            float speed = isSprinting ? bobSpeed * 1.5f : (isCrouched ? bobSpeed * 0.7f : bobSpeed);
            bobTimer += Time.deltaTime * speed;

            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(bobTimer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(bobTimer * 2f) * bobAmount.y,
                jointOriginalPos.z
            );
        }
        else
        {
            bobTimer = 0f;
            joint.localPosition = Vector3.Lerp(
                joint.localPosition,
                jointOriginalPos,
                Time.deltaTime * bobSpeed
            );
        }
    }
}

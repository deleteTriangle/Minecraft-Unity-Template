using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Mathf = UnityEngine.Mathf;
using Time = UnityEngine.Time;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.3f;
    public float sprintSpeed = 5.6f;
    public float crouchSpeed = 1.3f;
    public float jumpHeight = 1.25f;
    public float gravity = 25f;
    private float JumpPower => Mathf.Sqrt(2f * gravity * jumpHeight);
    public float fallMultiplier = 2.5f;
    private bool jumpRequested = false;
    private float verticalVelocity = 0f;

    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 0.15f;
    public float maxLookAngle = 90f;
    public bool invertCamera = false;
    public float fov = 70f;
    public float sprintFOV = 75f;
    public float sprintFOVStepTime = 10f;

    [Header("Crouch")]
    public float normalHeight = 1.8f;
    public float crouchHeight = 1.2f;
    public float normalCameraY = 0.75f;
    public float crouchCameraY = 0.45f;
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

    [Header("Crosshair")]
    public bool crosshair = true;
    public GameObject crosshairObject;

    // Private
    private CharacterController cc;
    private InputSystem_Actions inputActions;

    private Vector3 velocity;
    private float pitch = 0f;
    private float yaw = 0f;

    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouched;
    private bool isWalking;
    private bool isSprintCooldown;

    private float sprintRemaining;
    private float sprintCooldownTimer;

    private float bobTimer = 0f;
    private Vector3 jointOriginalPos;
    
    public void Init()
    {
        cc = GetComponent<CharacterController>();
        inputActions = G.PlayerInput.InputActions;

        cc.height = normalHeight;
        cc.center = new Vector3(0, 0, 0);

        playerCamera.fieldOfView = fov;
        playerCamera.transform.localPosition = new Vector3(0, normalCameraY, 0);

        jointOriginalPos = joint != null ? joint.localPosition : Vector3.zero;

        sprintRemaining = sprintDuration;
        sprintCooldownTimer = sprintCooldown;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairObject != null)
            crosshairObject.SetActive(crosshair);
    }

    private void Update()
    {
        HandleCamera();
        HandleCrouch();
        HandleFOV();

        if (enableHeadBob) HeadBob();

        if (inputActions.Player.Jump.WasPressedThisFrame() && isGrounded && !jumpRequested)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

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

    private void HandleFOV()
    {
        float targetFOV = isSprinting ? sprintFOV : fov;
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            sprintFOVStepTime * Time.deltaTime
        );
    }

    private void HandleMovement()
    {
        isGrounded = cc.isGrounded;
        
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        float currentSpeed = isCrouched ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 horizontalMove = (transform.right * input.x + transform.forward * input.y) * currentSpeed;
        
        isWalking = horizontalMove.magnitude > 0.1f && isGrounded;
        
        if (isGrounded && velocity.y < 0f)
            verticalVelocity = 0f;

        bool wantSprint = enableSprint && inputActions.Player.Sprint.IsPressed()
                          && input.y > 0.1f && !isCrouched && !isSprintCooldown;

        if (wantSprint)
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
        
        if (jumpRequested)
        {
            verticalVelocity = JumpPower;
            jumpRequested = false;
        }
        
        verticalVelocity -= gravity * Time.fixedDeltaTime;
        
        Vector3 move = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);
        cc.Move(move * Time.fixedDeltaTime);
    }

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
        if (!crouch && CeilingCheck())
            return;

        isCrouched = crouch;
        cc.height = crouch ? crouchHeight : normalHeight;
        
        cc.center = new Vector3(0f, cc.height / 2f - normalHeight / 2f, 0f);

        float targetCamY = crouch ? crouchCameraY : normalCameraY;
        StopAllCoroutines();
        StartCoroutine(SmoothCameraHeight(targetCamY));
    }

    private bool CeilingCheck()
    {
        return Physics.Raycast(transform.position, Vector3.up, normalHeight - crouchHeight + 0.1f);
    }

    private System.Collections.IEnumerator SmoothCameraHeight(float targetY)
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

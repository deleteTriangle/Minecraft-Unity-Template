using Unity.VisualScripting;
using UnityEditor.Build.Player;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    public RigidbodyController rigidbodyController;
    //public FirstPersonController firstPersonController;
    
    public BlockRegistry blockRegistry;
    public World world;
    public Interaction interaction;
    public BlockPlacing blockPlacing;
    public BlockDestroing blockDestroing;

    private int slotsCount;
    public HotbarView hotbarView;
    public HotbarInput hotbarInput;
    
    public EntityConfig playerConfig;

    public GameLoop gameLoop;
    
    public Player player;
    private Health playerHealth;
    public HUDHealthView hudHealthView;
    
    public CombatManager combatManager;
    
    private void Awake()
    {
        RegisterSystems();
        InitializeSystems();
    }

    private void OnDisable()
    {
        G.Clear();
    }

    private void RegisterSystems()
    {
        G.Register(new PlayerInput());
        G.Register(rigidbodyController);
        //G.Register(firstPersonController);
        
        G.Register(blockRegistry);
        G.Register(world);
        G.Register(interaction);
        G.Register(blockPlacing);
        G.Register(blockDestroing);

        G.Register(player);
        
        slotsCount = 9;
        G.Register(new Hotbar(slotsCount));
        
        G.Register(gameLoop);
        G.Register(combatManager);
    }
    
    private void InitializeSystems()
    {
        G.PlayerInput.Init();
        G.RigidbodyController.Init();
        //G.FirstPersonController.Init();
        
        G.BlockRegistry.Init();
        G.World.Init();
        G.BlockDestroing.Init();
        G.BlockPlacing.Init();
        
        G.Interaction.Init();
        
        G.Player.Init(playerConfig);
        hudHealthView.Init(G.Player.Health);
        
        G.Hotbar.PutItemInSlot(0, G.BlockRegistry.GetBlockConfig(BlockType.Water), 60);
        G.Hotbar.PutItemInSlot(1, G.BlockRegistry.GetBlockConfig(BlockType.Trapdoor), 60);
        G.Hotbar.PutItemInSlot(2, G.BlockRegistry.GetBlockConfig(BlockType.Furnace), 60);
        G.Hotbar.PutItemInSlot(3, G.BlockRegistry.GetBlockConfig(BlockType.Grass), 60);
        G.Hotbar.PutItemInSlot(4, G.BlockRegistry.GetBlockConfig(BlockType.Door), 60);
        hotbarView.Init();
        hotbarInput.Init(slotsCount);
        
        G.CombatManager.Init();
    }
}

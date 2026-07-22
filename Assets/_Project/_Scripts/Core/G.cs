using System;
using System.Collections.Generic;

public static class G
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static PlayerInput PlayerInput => Get<PlayerInput>();
    public static RigidbodyController RigidbodyController => Get<RigidbodyController>();
    public static FirstPersonController FirstPersonController => Get<FirstPersonController>();
    
    public static BlockRegistry BlockRegistry => Get<BlockRegistry>();
    public static World World => Get<World>();
    public static BlockPlacing BlockPlacing => Get<BlockPlacing>();
    public static BlockDestroing BlockDestroing => Get<BlockDestroing>();
    
    public static Hotbar Hotbar => Get<Hotbar>();
    
    public static Interaction Interaction => Get<Interaction>();

    public static Player Player => Get<Player>();

    public static GameLoop GameLoop => Get<GameLoop>();
    public static CombatManager CombatManager => Get<CombatManager>();

    public static void Register<T>(T service)
    {
        Type serviceType = typeof(T);

        if (service == null) throw new Exception(serviceType.Name + " is null!");
        if (services.ContainsKey(serviceType)) throw new Exception(serviceType.Name + " is already registered!");

        services.Add(serviceType, service);
    }

    public static void Clear()
    {
        services.Clear();
    }

    private static T Get<T>()
    {
        Type serviceType = typeof(T);
        
        if (!services.TryGetValue(serviceType, out object service))
        {
            throw new Exception(serviceType.Name + " is not registered!");
        }

        return (T)service;
    }
}

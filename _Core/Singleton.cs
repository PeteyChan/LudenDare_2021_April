using System;
using System.Collections.Generic;

/// <summary>
/// Helper class to get singletons
/// </summary>
static class Singleton
{
    static Dictionary<Type, object> singletons = new Dictionary<Type, object>();
    public static T Get<T>() where T: class, new ()
    {
        if (!singletons.TryGetValue(typeof(T), out var obj))
            singletons[typeof(T)] = obj = new T();
        return (T)obj;
    }

    public static bool TryGet(Type type, out object singleton)
    {
        if (!singletons.TryGetValue(type, out singleton))
        {
            if (type.IsClass && type.GetConstructor(Type.EmptyTypes) != null)
                singletons[type] = singleton = Activator.CreateInstance(type);
            else return false;
        }
        return true;
    }
}
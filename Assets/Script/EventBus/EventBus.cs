using System;
using System.Collections.Generic;

public class EventBus
{
    public delegate void Handlers(Event e);
    static Dictionary<Type, Handlers> bus = new Dictionary<Type, Handlers>();

    public static void Sub(Type t, Handlers hs)
    {
        if (bus.ContainsKey(t))
        {
            bus[t] += hs;
        }
        else
        {
            bus[t] = hs;
        }
    }

    public static void UnSub(Type t, Handlers hs)
    {
        if (bus.ContainsKey(t))
        {
            bus[t] -= hs;
        }
    }

    public static void Notify(Event e)
    {
        System.Type t = e.GetType();
        if (bus.ContainsKey(t))
        {
            bus[t](e);
        }
        else
        {
            Log.Warn("No Event Handler: " + e.GetType());
        }
    }
}

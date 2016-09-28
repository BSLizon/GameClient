using System;
using System.Collections.Generic;

public class EventSystem
{
    public delegate void Handlers(Object e);
    Dictionary<Type, Handlers> bus = new Dictionary<Type, Handlers>();

    public void Sub(Type t, Handlers hs)
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

    public void UnSub(Type t, Handlers hs)
    {
        if (bus.ContainsKey(t))
        {
            bus[t] -= hs;
        }
    }

    public void Notify(Object e)
    {
        System.Type t = e.GetType();
        if (bus.ContainsKey(t))
        {
            bus[t](e);
        }
        
    }
}

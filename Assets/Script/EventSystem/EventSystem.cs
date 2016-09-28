using System;
using UnityEngine;
using System.Collections.Generic;

public class EventSystem
{
    public delegate void Handlers(Event e);
    Dictionary<System.Type, Handlers> bus = new Dictionary<System.Type, Handlers>();

    public void Sub(System.Type t, Handlers hdlr)
    {
        if (bus.ContainsKey(t))
        {
            bus[t] += hdlr;
        }
        else
        {
            bus[t] = hdlr;
        }
    }

    public void UnSub(System.Type t, Handlers hdlr)
    {
        if (bus.ContainsKey(t))
        {
            bus[t] -= hdlr;
        }
    }

    public void Notify(Event e)
    {
        System.Type t = e.GetType();
        if (bus.ContainsKey(t))
        {
            bus[t](e);
        }
        
    }
}

using System.Collections.Generic;

public class EventSystem
{
    public delegate void Handlers(Event evt);
    Dictionary<Event.Type, Handlers> bus = new Dictionary<Event.Type, Handlers>();

    public void Sub(Event.Type t, Handlers hdlr)
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

    public void UnSub(Event.Type t, Handlers hdlr)
    {
        if (bus.ContainsKey(t))
        {
            bus[t] -= hdlr;
        }
    }

    public void Notify(Event evt)
    {
        if (bus.ContainsKey(evt.type))
        {
            bus[evt.type](evt);
        }
        
    }
}

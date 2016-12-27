using System;
using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
	void Start ()
    {
        EventBus.Sub(typeof(Network.Event_RecvMessage), f);
    }

    void Update()
    {
        for (int i = 0; i < 20; i++)
        {
            Network.getInstance().Send(System.Text.Encoding.Default.GetBytes("Hello World"));
        }
        Network.getInstance().Update();
    }

    void f(Event e)
    {
        Debug.Log(System.Text.Encoding.Default.GetString((e as Network.Event_RecvMessage).data));
    }
}



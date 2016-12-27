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
        Network.getInstance().Update();
    }

    void f(Event e)
    {
        Debug.Log((e as Network.Event_RecvMessage).data);
    }
}



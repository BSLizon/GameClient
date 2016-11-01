using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
	void Start ()
    {
        Network.getInstance();
        Network.getInstance().Reconnect();
    }

    public void func(Event e)
    {
        MyEvent me = e as MyEvent;
        Debug.Log("asdf");
    }
}

class MyEvent : Event
{

}

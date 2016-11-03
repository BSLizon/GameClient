using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
	void Start ()
    {
        Network.getInstance();
    }

    public void func(Event e)
    {
        MyEvent me = e as MyEvent;
        Debug.Log("asdf");
    }

    void Update()
    {
        Network.getInstance().Update();
    }
}

class MyEvent : Event
{

}

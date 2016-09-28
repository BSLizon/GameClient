using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
	void Start ()
    {
        EventSystem es = new EventSystem();
        es.Sub(typeof(MyEvent), func);
        es.Notify(new MyEvent());
	}

    public void func(Event e)
    {
        MyEvent me = e as MyEvent;
        Debug.Log("asdf");
    }
}

class MyEvent : Event
{}

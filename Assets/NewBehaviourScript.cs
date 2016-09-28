using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
	void Start ()
    {
        EventSystem es = new EventSystem();
        es.Sub(typeof(NewBehaviourScript), func);
        es.Notify(new NewBehaviourScript());
	}

    public void func(System.Object e)
    {
        NewBehaviourScript me = e as NewBehaviourScript;
        Debug.Log("asdf");
    }
}

class MyEvent : Event
{}

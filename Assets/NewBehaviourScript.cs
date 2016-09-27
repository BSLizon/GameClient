using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour
{
	void Start ()
    {
        EventSystem es = new EventSystem();
        es.UnSub(Event.Type.None, func);
        es.Notify(new Event());
	}

    public void func(Event evt)
    {
        Debug.Log("asdf");
    }
}

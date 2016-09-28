using System;
using UnityEngine;
using System.Collections.Generic;

public class Entity
{
    Dictionary<PropDef, System.Object> propDic = new Dictionary<PropDef, System.Object>();

    public bool GetProp(PropDef prop, out System.Object obj)
    {
        bool ret;
        ret = propDic.TryGetValue(prop, out obj);
        if (!ret)
        {
            Debug.LogWarning("Get Null Prop");
        }
        return ret;
    }

    public void SetProp(PropDef prop, System.Object obj)
    {
        propDic[prop] = obj;
    }
}

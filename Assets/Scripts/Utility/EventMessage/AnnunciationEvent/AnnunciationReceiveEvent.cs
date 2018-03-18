using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class AnnunciationReceiveEvent : CitrusGameEvent
{
    public List<JSONObject> Result;

    public AnnunciationReceiveEvent(List<JSONObject> result)
    {
        Result = result;
    }
}

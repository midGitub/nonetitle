using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class AnnunciationShowEvent : CitrusGameEvent
{
    public List<string> AnnunciationList;

    public AnnunciationShowEvent(List<string> annunciationList)
    {
        AnnunciationList = annunciationList;
    }
}

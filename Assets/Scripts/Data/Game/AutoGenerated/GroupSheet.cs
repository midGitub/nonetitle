using System;
using System.Collections;
using System.Collections.Generic;

///
/// !!! Machine generated code !!!
///
/// A class which deriveds ScritableObject class so all its data 
/// can be serialized onto an asset data file.
/// 
[System.Serializable]
#if CORE_DLL
public class GroupSheet : IExcelSheet<GroupData>
#else
public class GroupSheet : UnityEngine.ScriptableObject
#endif
{	
	#if !CORE_DLL
    [UnityEngine.SerializeField] 
    #endif
    public string SheetName = "";
    
    #if !CORE_DLL
    [UnityEngine.SerializeField] 
    #endif
    public string WorksheetName = "";
    
    // Note: initialize in OnEnable() not here.
    public GroupData[] dataArray;
    public GroupData[] DataArray { get { return dataArray; } set { dataArray = value; } }
    
    void OnEnable()
    {		
//#if UNITY_EDITOR
        //hideFlags = HideFlags.DontSave;
//#endif
        // Important:
        //    It should be checked an initialization of any collection data before it is initialized.
        //    Without this check, the array collection which already has its data get to be null 
        //    because OnEnable is called whenever Unity builds.
        // 		
        if (dataArray == null)
            dataArray = new GroupData[0];
    }
}

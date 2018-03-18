using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class BetOptionData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string key;
  public string Key { get {return key; } set { key = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] options = new int[0];
  public int[] Options { get {return options; } set { options = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string[] machines = new string[0];
  public string[] Machines { get {return machines; } set { machines = value;} }
  
}
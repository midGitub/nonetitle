using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class ChestData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float money;
  public float Money { get {return money; } set { money = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int cDTime;
  public int CDTime { get {return cDTime; } set { cDTime = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] reward = new int[0];
  public int[] Reward { get {return reward; } set { reward = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] weights = new int[0];
  public int[] Weights { get {return weights; } set { weights = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int lTLucky;
  public int LTLucky { get {return lTLucky; } set { lTLucky = value;} }
  
}
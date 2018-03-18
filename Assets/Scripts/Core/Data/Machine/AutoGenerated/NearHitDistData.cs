using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
// Caution: IJoyDistData is added by manual
public class NearHitDistData : IJoyDistData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int id;
  public int Id { get {return id; } set { id = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float reward;
  public float Reward { get {return reward; } set { reward = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float overallHit;
  public float OverallHit { get {return overallHit; } set { overallHit = value;} }
  
}
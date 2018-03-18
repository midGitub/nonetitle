using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class AdBonusData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string bonusType;
  public string BonusType { get {return bonusType; } set { bonusType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int adTypeId;
  public int AdTypeId { get {return adTypeId; } set { adTypeId = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int minRegisterDays;
  public int MinRegisterDays { get {return minRegisterDays; } set { minRegisterDays = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int requireSpinCount;
  public int RequireSpinCount { get {return requireSpinCount; } set { requireSpinCount = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int priority;
  public int Priority { get {return priority; } set { priority = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string[] unavailableMachineList = new string[0];
  public string[] UnavailableMachineList { get {return unavailableMachineList; } set { unavailableMachineList = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int duration;
  public int Duration { get {return duration; } set { duration = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int basicRewardCredits;
  public int BasicRewardCredits { get {return basicRewardCredits; } set { basicRewardCredits = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int enable;
  public int Enable { get {return enable; } set { enable = value;} }
  
}
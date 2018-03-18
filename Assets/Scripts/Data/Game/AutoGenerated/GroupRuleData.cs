using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class GroupRuleData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int iD;
  public int ID { get {return iD; } set { iD = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] hasPayInWeek = new int[0];
  public int[] HasPayInWeek { get {return hasPayInWeek; } set { hasPayInWeek = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] payCount = new int[0];
  public int[] PayCount { get {return payCount; } set { payCount = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] payAmout = new int[0];
  public int[] PayAmout { get {return payAmout; } set { payAmout = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] historyMaxPaid = new int[0];
  public int[] HistoryMaxPaid { get {return historyMaxPaid; } set { historyMaxPaid = value;} }
  
}
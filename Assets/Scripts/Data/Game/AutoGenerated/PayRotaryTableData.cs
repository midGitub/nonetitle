using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class PayRotaryTableData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int bonusID;
  public int BonusID { get {return bonusID; } set { bonusID = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  long bonus;
  public long Bonus { get {return bonus; } set { bonus = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] prob = new float[0];
  public float[] Prob { get {return prob; } set { prob = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float angle;
  public float Angle { get {return angle; } set { angle = value;} }
  
}
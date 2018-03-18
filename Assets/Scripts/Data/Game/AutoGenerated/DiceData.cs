using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class DiceData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int diceType;
  public int DiceType { get {return diceType; } set { diceType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int diceNum;
  public int DiceNum { get {return diceNum; } set { diceNum = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float minRatio;
  public float MinRatio { get {return minRatio; } set { minRatio = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int minreward;
  public int Minreward { get {return minreward; } set { minreward = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int maxreward;
  public int Maxreward { get {return maxreward; } set { maxreward = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] gameResult = new int[0];
  public int[] GameResult { get {return gameResult; } set { gameResult = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string probability;
  public string Probability { get {return probability; } set { probability = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int iAPId;
  public int IAPId { get {return iAPId; } set { iAPId = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int payUserIAPId;
  public int PayUserIAPId { get {return payUserIAPId; } set { payUserIAPId = value;} }
  
}
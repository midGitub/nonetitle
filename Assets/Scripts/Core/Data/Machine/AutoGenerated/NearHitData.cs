using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class NearHitData : IJoyData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int id;
  public int Id { get {return id; } set { id = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string[] symbols = new string[0];
  public string[] Symbols { get {return symbols; } set { symbols = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  PayoutType payoutType;
  public PayoutType PayoutType { get {return payoutType; } set { payoutType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int count;
  public int Count { get {return count; } set { count = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float overallHit;
  public float OverallHit { get {return overallHit; } set { overallHit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] reel1Wild = new float[0];
  public float[] Reel1Wild { get {return reel1Wild; } set { reel1Wild = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] reel2Wild = new float[0];
  public float[] Reel2Wild { get {return reel2Wild; } set { reel2Wild = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] reel3Wild = new float[0];
  public float[] Reel3Wild { get {return reel3Wild; } set { reel3Wild = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] reel4Wild = new float[0];
  public float[] Reel4Wild { get {return reel4Wild; } set { reel4Wild = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float slide1;
  public float Slide1 { get {return slide1; } set { slide1 = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float slide2;
  public float Slide2 { get {return slide2; } set { slide2 = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float slide3;
  public float Slide3 { get {return slide3; } set { slide3 = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float fix1ReelOverallHit;
  public float Fix1ReelOverallHit { get {return fix1ReelOverallHit; } set { fix1ReelOverallHit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float fix2ReelOverallHit;
  public float Fix2ReelOverallHit { get {return fix2ReelOverallHit; } set { fix2ReelOverallHit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float fix1Hit;
  public float Fix1Hit { get {return fix1Hit; } set { fix1Hit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float fix2Hit;
  public float Fix2Hit { get {return fix2Hit; } set { fix2Hit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  bool multiLine;
  public bool MultiLine { get {return multiLine; } set { multiLine = value;} }
  
}
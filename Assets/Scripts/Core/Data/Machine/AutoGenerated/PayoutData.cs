using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class PayoutData : IJoyData
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
  float ratio;
  public float Ratio { get {return ratio; } set { ratio = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] periodRatios = new float[0];
  public float[] PeriodRatios { get {return periodRatios; } set { periodRatios = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string jackpotType;
  public string JackpotType { get {return jackpotType; } set { jackpotType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string ratioType;
  public string RatioType { get {return ratioType; } set { ratioType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int jackpotCount;
  public int JackpotCount { get {return jackpotCount; } set { jackpotCount = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string[] wheelNames = new string[0];
  public string[] WheelNames { get {return wheelNames; } set { wheelNames = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] rewindHits = new float[0];
  public float[] RewindHits { get {return rewindHits; } set { rewindHits = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float overallHit;
  public float OverallHit { get {return overallHit; } set { overallHit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float freeSpinOverallHit;
  public float FreeSpinOverallHit { get {return freeSpinOverallHit; } set { freeSpinOverallHit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] freeSpinHits = new float[0];
  public float[] FreeSpinHits { get {return freeSpinHits; } set { freeSpinHits = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] freeSpinStopProbs = new float[0];
  public float[] FreeSpinStopProbs { get {return freeSpinStopProbs; } set { freeSpinStopProbs = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float freeSpinStopOverallHit;
  public float FreeSpinStopOverallHit { get {return freeSpinStopOverallHit; } set { freeSpinStopOverallHit = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float longLuckySubtractFactor;
  public float LongLuckySubtractFactor { get {return longLuckySubtractFactor; } set { longLuckySubtractFactor = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  bool isFixed;
  public bool IsFixed { get {return isFixed; } set { isFixed = value;} }
  
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
  bool isShortLucky;
  public bool IsShortLucky { get {return isShortLucky; } set { isShortLucky = value;} }
  
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
  bool nearHitOrLossForFixWild;
  public bool NearHitOrLossForFixWild { get {return nearHitOrLossForFixWild; } set { nearHitOrLossForFixWild = value;} }
  
}
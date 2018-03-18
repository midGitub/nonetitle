using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class SymbolData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string name;
  public string Name { get {return name; } set { name = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  SymbolType symbolType;
  public SymbolType SymbolType { get {return symbolType; } set { symbolType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  SlideType slideType;
  public SlideType SlideType { get {return slideType; } set { slideType = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string artAsset;
  public string ArtAsset { get {return artAsset; } set { artAsset = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int jackpotCount;
  public int JackpotCount { get {return jackpotCount; } set { jackpotCount = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int multiplier;
  public int Multiplier { get {return multiplier; } set { multiplier = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  bool canMirror;
  public bool CanMirror { get {return canMirror; } set { canMirror = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] mirrorReelIndexes = new int[0];
  public int[] MirrorReelIndexes { get {return mirrorReelIndexes; } set { mirrorReelIndexes = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  bool hasStrongSpecialEffect;
  public bool HasStrongSpecialEffect { get {return hasStrongSpecialEffect; } set { hasStrongSpecialEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialEffect;
  public string StrongSpecialEffect { get {return strongSpecialEffect; } set { strongSpecialEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialHintEffect;
  public string StrongSpecialHintEffect { get {return strongSpecialHintEffect; } set { strongSpecialHintEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float strongSpecialHintDelay;
  public float StrongSpecialHintDelay { get {return strongSpecialHintDelay; } set { strongSpecialHintDelay = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialHintSound;
  public string StrongSpecialHintSound { get {return strongSpecialHintSound; } set { strongSpecialHintSound = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialWinEffect;
  public string StrongSpecialWinEffect { get {return strongSpecialWinEffect; } set { strongSpecialWinEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float strongSpecialWinDelay;
  public float StrongSpecialWinDelay { get {return strongSpecialWinDelay; } set { strongSpecialWinDelay = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialWinSound;
  public string StrongSpecialWinSound { get {return strongSpecialWinSound; } set { strongSpecialWinSound = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialStartEffect;
  public string StrongSpecialStartEffect { get {return strongSpecialStartEffect; } set { strongSpecialStartEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float strongSpecialStartDelay;
  public float StrongSpecialStartDelay { get {return strongSpecialStartDelay; } set { strongSpecialStartDelay = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string strongSpecialStartSound;
  public string StrongSpecialStartSound { get {return strongSpecialStartSound; } set { strongSpecialStartSound = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string winEffect;
  public string WinEffect { get {return winEffect; } set { winEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string winEffect3D;
  public string WinEffect3D { get {return winEffect3D; } set { winEffect3D = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string collectTrailEffect;
  public string CollectTrailEffect { get {return collectTrailEffect; } set { collectTrailEffect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string collectAnimator;
  public string CollectAnimator { get {return collectAnimator; } set { collectAnimator = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string[] canApplyCollect = new string[0];
  public string[] CanApplyCollect { get {return canApplyCollect; } set { canApplyCollect = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string switchToSymbol;
  public string SwitchToSymbol { get {return switchToSymbol; } set { switchToSymbol = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string[] switchSymbolNames = new string[0];
  public string[] SwitchSymbolNames { get {return switchSymbolNames; } set { switchSymbolNames = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] switchSymbolProbs = new float[0];
  public float[] SwitchSymbolProbs { get {return switchSymbolProbs; } set { switchSymbolProbs = value;} }
  
}
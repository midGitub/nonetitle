using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class WheelData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int iD;
  public int ID { get {return iD; } set { iD = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] ratio = new float[0];
  public float[] Ratio { get {return ratio; } set { ratio = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float prob;
  public float Prob { get {return prob; } set { prob = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  float[] angle = new float[0];
  public float[] Angle { get {return angle; } set { angle = value;} }
  
}
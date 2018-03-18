using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class GroupRepresentData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string model;
  public string Model { get {return model; } set { model = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] presentValue = new int[0];
  public int[] PresentValue { get {return presentValue; } set { presentValue = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] realValue = new int[0];
  public int[] RealValue { get {return realValue; } set { realValue = value;} }
  
}
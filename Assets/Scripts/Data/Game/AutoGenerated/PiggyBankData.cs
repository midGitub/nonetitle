using UnityEngine;
using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class PiggyBankData
{
  [SerializeField]
  int iD;
  public int ID { get {return iD; } set { iD = value;} }
  
  [SerializeField]
  int minCredits;
  public int MinCredits { get {return minCredits; } set { minCredits = value;} }
  
  [SerializeField]
  int maxCredits;
  public int MaxCredits { get {return maxCredits; } set { maxCredits = value;} }
  
  [SerializeField]
  float conversionRate;
  public float ConversionRate { get {return conversionRate; } set { conversionRate = value;} }
  
}
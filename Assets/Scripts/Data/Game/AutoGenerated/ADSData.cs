using UnityEngine;
using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class ADSData
{
  [SerializeField]
  int iD;
  public int ID { get {return iD; } set { iD = value;} }
  
  [SerializeField]
  bool rewardAdvertisingOpen;
  public bool RewardAdvertisingOpen { get {return rewardAdvertisingOpen; } set { rewardAdvertisingOpen = value;} }
  
  [SerializeField]
  int rewardCredit;
  public int RewardCredit { get {return rewardCredit; } set { rewardCredit = value;} }
  
  [SerializeField]
  int[] rewardADSTimeInterval = new int[0];
  public int[] RewardADSTimeInterval { get {return rewardADSTimeInterval; } set { rewardADSTimeInterval = value;} }
  
  [SerializeField]
  int getRewardADSLimit;
  public int GetRewardADSLimit { get {return getRewardADSLimit; } set { getRewardADSLimit = value;} }
  
  [SerializeField]
  bool plaqueADSOPen;
  public bool PlaqueADSOPen { get {return plaqueADSOPen; } set { plaqueADSOPen = value;} }
  
  [SerializeField]
  int registerDaysOpenPlaqueADS;
  public int RegisterDaysOpenPlaqueADS { get {return registerDaysOpenPlaqueADS; } set { registerDaysOpenPlaqueADS = value;} }
  
  [SerializeField]
  int[] plaqueADSTimeInterval = new int[0];
  public int[] PlaqueADSTimeInterval { get {return plaqueADSTimeInterval; } set { plaqueADSTimeInterval = value;} }
  
  [SerializeField]
  int getPlaqueADSLimit;
  public int GetPlaqueADSLimit { get {return getPlaqueADSLimit; } set { getPlaqueADSLimit = value;} }
  
  [SerializeField]
  bool openBuyNoADS;
  public bool OpenBuyNoADS { get {return openBuyNoADS; } set { openBuyNoADS = value;} }
  
}
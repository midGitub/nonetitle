using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class LocalNotificationData
{
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int iD;
  public int ID { get {return iD; } set { iD = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string key;
  public string Key { get {return key; } set { key = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  int[] lostDays = new int[0];
  public int[] LostDays { get {return lostDays; } set { lostDays = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string time;
  public string Time { get {return time; } set { time = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string title;
  public string Title { get {return title; } set { title = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string content;
  public string Content { get {return content; } set { content = value;} }
  
  #if !CORE_DLL
  [UnityEngine.SerializeField]
  #endif
  string contentIOS;
  public string ContentIOS { get {return contentIOS; } set { contentIOS = value;} }
  
}
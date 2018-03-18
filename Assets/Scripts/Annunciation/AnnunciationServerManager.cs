using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public enum AnnunciationType
{
    VipLevelUp = 0,
}
/*MSG FORMAT
 
/broadcast/get
req:
{
	"ProjectName": "Slots",
	"BroadcastID": 0
}

res:
{
  "error": 0,
  "ContentArray": [
    {
      "ID": 1515053707445,
      "Content": {
        "Code": "test",
        "test1": 111
      }
    },
    {
      "ID": 1515053707199,
      "Content": {
        "Code": "test",
        "test1": 111
      }
    }
  ]
}

/broadcast/add
req:
{
	"ProjectName": "Slots",
	"Content": {
	    "Code": "test",
	    "test1": 111
	}
}
res:
{
  "error": 0
}

 */
public class AnnunciationServerManager : Singleton<AnnunciationServerManager>
{
    private readonly string _addUrlSuffix = "broadcast/add";
    private readonly string _getUrlSuffix = "broadcast/get";
    private readonly float _reqTimeoutTime = 10;
    private float _updateAnnunciationsIntervalTime;
    private readonly float _updateAnnunciationDefaultTime = 30;
    private bool _enableAnnunciation;
    private WaitForSeconds _updateIntervalSecs;

    #region ExcelConfigString
    private readonly string _annunciationUpdateIntervalTimeKey = "AnnunciationUpdateIntervalTime";
    private readonly string _enableKey = "AnnunciationEnable";
    #endregion

    public void Init()
    {
        _updateAnnunciationsIntervalTime = MapSettingConfig.Instance.Read(_annunciationUpdateIntervalTimeKey, _updateAnnunciationDefaultTime);
        _enableAnnunciation = MapSettingConfig.Instance.Read(_enableKey, "0") == "1";
        _updateIntervalSecs = new WaitForSeconds(_updateAnnunciationsIntervalTime);
        StartCoroutine(UpdateAnnunciationsMsg());
    }

    IEnumerator UpdateAnnunciationsMsg()
    {
        while (true)
        {
            GetAnnunciationEventsFromServer();
            yield return _updateIntervalSecs;
        }
    }

    private bool Enable()
    {
        return DeviceUtility.IsConnectInternet() && _enableAnnunciation;
    }

    public void AddAnnunciationEventsToServer(Dictionary<string, object> contentDic)
    {
        if (Enable())
        {
            Dictionary<string, object> sendDic = new Dictionary<string, object>();
            sendDic.Add("ProjectName", BuildUtility.GetProjectName());
            sendDic.Add("Content", contentDic);

            StartCoroutine(NetWorkHelper.UniversalNetCall(UnityTimer.Instance, _reqTimeoutTime, ServerConfig.GameServerUrl, _addUrlSuffix,
                sendDic,
                jsonObj =>
                {
                    LogUtility.Log("AnnunciationServerManager: add broadcast event to server successfully", Color.yellow);
                },
                jsonObj => { Debug.LogError("AnnunciationServerManager: fail to add broadcast event to server"); }));

            //requst annunciation data immediately from server when user's vip lv upgrade
            GetAnnunciationEventsFromServer();
        }
    }

    void GetAnnunciationEventsFromServer()
    {
        if (Enable())
        {
            Dictionary<string, object> sendDic = new Dictionary<string, object>();
            sendDic.Add("ProjectName", BuildUtility.GetProjectName());
            sendDic.Add("BroadcastID", UserBasicData.Instance.LastReceivedBroadcastId);

            StartCoroutine(NetWorkHelper.UniversalNetCall(UnityTimer.Instance, _reqTimeoutTime, ServerConfig.GameServerUrl, _getUrlSuffix,
                sendDic,
                jsonObj =>
                {
                    if (jsonObj.HasField("ContentArray") && jsonObj.GetField("ContentArray").IsArray)
                    {
                        List<JSONObject> contentList = jsonObj.GetField("ContentArray").list;
                        if (contentList.Count > 0 && contentList[0].HasField("ID"))
                        {
                            CitrusEventManager.instance.Raise(new AnnunciationReceiveEvent(contentList));
                            UserBasicData.Instance.LastReceivedBroadcastId = (ulong)contentList[0].GetField("ID").n;
                        }
                    }
                    LogUtility.Log("AnnunciationServerManager: get broadcast event from server successfully");
                },
                jsonObj =>
                {
                    Debug.LogError("AnnunciationServerManager: fail to get broadcast event from server");
                }));
        }
    }
}

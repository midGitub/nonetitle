using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class AnnunciationVipLvUpManager : Singleton<AnnunciationVipLvUpManager>
{
    private readonly string _typeField = "type";
    private readonly string _userNameField = "userName";
    private readonly string _vipLvField = "vipLv";
    private readonly string _contentField = "Content";

    private int _rollNumStart;
    private int _rollNumEnd;
    private float _robotVipLvupIntervalTime;
    private int _rollNumStartDefault = 1000000;
    private int _rollNumEndDefault = 1099999;
    private float _robotVipLvupIntervalTimeDefault = 600;
    private bool _enableRobot;
    private readonly string _vipLvUpStrKey = "annunciation_vipLvUp";
    private readonly string _guestStrKey = "annunciation_guest";
    private readonly string _robotVipLvupIntervalTimeKey = "AnnunciationRobotLvupInterTime";
    private readonly string _rollNumStartKey = "AnnunciationRobotUdidStart";
    private readonly string _rollNumEndKey = "AnnunciationRobotUdidEnd";
    private readonly string _robotVipLvRatioKey = "AnnunciationRobotVipLvRatioList";
    private List<int> _robotVipLvList = new List<int>();
    private List<float> _robotVipLvRatioList = new List<float>();
    private WaitForSeconds _robotViplvupSecs;
    private IRandomGenerator _roller = new LCG((uint)new System.Random().Next(), null);

    #region ExcelConfigString
    private readonly string _enableRobotKey = "AnnunciationEnableRobotVipLvUp";
    #endregion


    public void Init()
    {
        InitRobotVipLvRatioList();
        _robotVipLvupIntervalTime = MapSettingConfig.Instance.Read(_robotVipLvupIntervalTimeKey, _robotVipLvupIntervalTimeDefault);
        _rollNumStart = MapSettingConfig.Instance.Read(_rollNumStartKey, _rollNumStartDefault);
        _rollNumEnd = MapSettingConfig.Instance.Read(_rollNumEndKey, _rollNumEndDefault);
        _enableRobot = MapSettingConfig.Instance.Read(_enableRobotKey, "0") == "1";

        CitrusEventManager.instance.AddListener<AnnunciationReceiveEvent>(OnReceiveAnnunciationEvent);
        StartCoroutine(RobotVipLvupMsg());
    }

    public void SendVipLvupEventToServer()
    {
        Dictionary<string, object> content = new Dictionary<string, object>();
        string userName = FacebookHelper.IsLoggedIn 
                        ? FacebookHelper.UserName 
                        : LocalizationConfig.Instance.GetValue(_guestStrKey) + UserBasicData.Instance.UDID;
        content.Add(_typeField, AnnunciationType.VipLevelUp.ToString());
        content.Add(_userNameField, userName);
        content.Add(_vipLvField, (uint)VIPSystem.Instance.GetCurrVIPLevelData.Level);
        AnnunciationServerManager.Instance.AddAnnunciationEventsToServer(content);
    }

    private void OnReceiveAnnunciationEvent(AnnunciationReceiveEvent e)
    {
        List<string> annunciationList = new List<string>();
        ListUtility.ForEach(e.Result, jsonObj =>
        {
            if (jsonObj.HasField(_contentField))
            {
                JSONObject content = jsonObj.GetField(_contentField);
                if (content.HasField(_typeField) && content.GetField(_typeField).str == AnnunciationType.VipLevelUp.ToString())
                {
                    if (content.HasField(_userNameField) && content.HasField(_vipLvField))
                    {
                        annunciationList.Add(String.Format(LocalizationConfig.Instance.GetValue(_vipLvUpStrKey), content.GetField(_userNameField).str, (uint)content.GetField(_vipLvField).n));
                    }
                }

            }
        });

        if (annunciationList.Count > 0)
        {
            CitrusEventManager.instance.Raise(new AnnunciationShowEvent(annunciationList));
        }
    }

#region robot vip level upgrade

    private bool ShouldShowRobotLvUpMsg(string robotUdid)
    {
        return DeviceUtility.IsConnectInternet() && robotUdid != UserBasicData.Instance.UDID && _enableRobot;
    }

    IEnumerator RobotVipLvupMsg()
    {
        _robotViplvupSecs = new WaitForSeconds(_robotVipLvupIntervalTime);

        while (true)
        {
            yield return _robotViplvupSecs;
            int robotUdid = RandomUtility.RollInt(_roller, _rollNumStart, _rollNumEnd);
            //there is very low probability that the random udid is equal with user's udid, just do not show annunciation ui rather than roll again
            if(ShouldShowRobotLvUpMsg(robotUdid.ToString()))
            {
                int vipLv = RandomUtility.RollSingleIntByRatios(_roller, _robotVipLvList, _robotVipLvRatioList);
                CitrusEventManager.instance.Raise(new AnnunciationShowEvent(new List<string>
                {
                    String.Format(LocalizationConfig.Instance.GetValue(_vipLvUpStrKey), LocalizationConfig.Instance.GetValue(_guestStrKey) + robotUdid, vipLv)
                }));
            }
        }
    }

    void InitRobotVipLvRatioList()
    {
        string ratioList = MapSettingConfig.Instance.Read(_robotVipLvRatioKey, "");
        Debug.Assert(ratioList != "", "AnnunciationVipLvUpManager : Do not find robot vip level ratio list config in gamesetting.xlsx!");

        if (ratioList != "")
        {
            string[] robotVipLvInfoList = ratioList.Split('~');
            ListUtility.ForEach(robotVipLvInfoList, vipLvInfo =>
            {
                string[] infoArray = vipLvInfo.Split(',');
                if (infoArray.Length >= 2 && infoArray[0] != null && infoArray[1] != null)
                {
                    int vipLv = 0;
                    float vipRatio = 0;
                    if (int.TryParse(infoArray[0], out vipLv) && float.TryParse(infoArray[1], out vipRatio))
                    {
                        _robotVipLvList.Add(vipLv);
                        _robotVipLvRatioList.Add(vipRatio);
                    }
                    else
                        Debug.LogError("AnnunciationVipLvUpManager : issue occur when read robot vip lv info from excel file");
                }
            });
        }
    }
#endregion
}

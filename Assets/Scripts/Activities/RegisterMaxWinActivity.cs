using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class MaxWinDateInfo
{
    public DateTime StartDate;
    public DateTime EndDate;

    public MaxWinDateInfo(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
public class RegisterMaxWinActivity : Singleton<RegisterMaxWinActivity>
{
    private readonly string _urlSuffix = "events/set_param";
    private readonly string _activityName = "Christmas";
    private readonly float _reqTimeoutTime = 10;
    private bool _isRegistering;
    private float _maxWinUiCooldownHour;
    private List<MaxWinDateInfo> _dateInfoList;
    public MaxWinDateInfo CurActivityDateInfo;

    public void Init()
    {
        LoadDateConfig();
        CitrusEventManager.instance.AddListener<EnterMainMapSceneEvent>(OnEnterMapScene);
        CitrusEventManager.instance.AddListener<EnterGameSceneEvent>(OnEnterGameScene);
    }

    void LoadDateConfig()
    {
        string dateInfo = MapSettingConfig.Instance.Read("MaxWinActivityDateList", "");
        InitDateInfoList(dateInfo);
        _maxWinUiCooldownHour = MapSettingConfig.Instance.Read("ChristmasMaxWinUiCooldownHour", 0);
    }

    void InitDateInfoList(string dateInfoList)
    {
        Debug.Assert(dateInfoList != "", "RegisterMaxWinActivity : Do not find activity date config in gamesetting.xlsx!");
        _dateInfoList = new List<MaxWinDateInfo>();

        if (dateInfoList != "")
        {
            string[] dateinfoArray = dateInfoList.Split('~');
            ListUtility.ForEach(dateinfoArray, dates =>
            {
                string[] dateInfo = dates.Split(',');
                if (dateInfo.Length >= 2 && dateInfo[0] != null && dateInfo[1] != null)
                {
                    object startDate = Convert.ChangeType(dateInfo[0], typeof(DateTime));
                    object endDate = Convert.ChangeType(dateInfo[1], typeof(DateTime));

                    if (startDate != null && endDate != null)
                        _dateInfoList.Add(new MaxWinDateInfo((DateTime)startDate, (DateTime)endDate));
                    else
                        Debug.LogError("RegisterMaxWinActivity : issue occur when change datestr into DateTime Type");
                }
            });
        }
    }

    void OnEnterMapScene(EnterMainMapSceneEvent e)
    {
        CheckMaxWinReward();
        SetCurActivityDate();
    }

    void OnEnterGameScene(EnterGameSceneEvent e)
    {
        StartActivity();
        CheckMaxWinReward();
    }

    void SetCurActivityDate()
    {
        CurActivityDateInfo = ListUtility.FindFirstOrDefault(_dateInfoList, info => 
            TimeUtility.IsBetweenRange(info.StartDate, info.EndDate));

        if (CurActivityDateInfo == null)
        {
            CurActivityDateInfo = new MaxWinDateInfo(DateTime.MinValue, DateTime.MinValue);
        }
    }

    void CheckMaxWinReward()
    {
        if (DeviceUtility.IsConnectInternet() && IsInRewardTime())
        {
            if (UserBasicData.Instance.MaxWinDuringActivity != 0)
            {
                CreatMailSelf.Instance.CreateChristmasMaxWinRewardMail();
                UserBasicData.Instance.MaxWinDuringActivity = 0;
            }

            UserBasicData.Instance.LastGetMaxWinActivityRewardDate = NetworkTimeHelper.Instance.GetNowTime();
        }
    }

    bool IsInRewardTime()
    {
        bool result = ListUtility.IsAnyElementSatisfied(_dateInfoList, info => 
            TimeUtility.IsDatePast(info.EndDate) && (info.EndDate - UserBasicData.Instance.LastGetMaxWinActivityRewardDate).TotalSeconds > 0);
  
        return result;
    }

    public void StartActivity()
    {
        //clean all coroutines when user load game scene and restart all coroutains
        StopAllCoroutines();

        ListUtility.ForEach(_dateInfoList, dateInfo =>
        {
            DateTime startDate = dateInfo.StartDate;
            DateTime endDate = dateInfo.EndDate;

            if (!TimeUtility.IsDatePast(endDate))
            {
                //during activity days
                if (TimeUtility.IsBetweenRange(startDate, endDate))
                {
                    StartRegister();
                    StartCoroutine(WaitForActivityEnd(endDate));
                }
                //wait for activity start
                else
                {
                    StartCoroutine(WaitForActivityStart(startDate));
                    StartCoroutine(WaitForActivityEnd(endDate));
                }
            }
        });
    }

    IEnumerator WaitForActivityStart(DateTime startDate)
    {
        double totalSecondsLeft = TimeUtility.CountdownOfDateFromNowOn(startDate).TotalSeconds;
        yield return new WaitForSeconds((float)totalSecondsLeft);

        StartRegister();
    }

    IEnumerator WaitForActivityEnd(DateTime endDate)
    {
        double totalSecondsLeft = TimeUtility.CountdownOfDateFromNowOn(endDate).TotalSeconds;
        yield return new WaitForSeconds((float)totalSecondsLeft);

        EndRegister();
        CheckMaxWinReward();
    }

    void StartRegister()
    {
        if (!_isRegistering)
        {
            CitrusEventManager.instance.AddListener<RegisterWinAmountEvent>(UpdateMaxWinAmount);
            _isRegistering = true;
        }
    }

    void EndRegister()
    {
        if (_isRegistering)
        {
            CitrusEventManager.instance.RemoveListener<RegisterWinAmountEvent>(UpdateMaxWinAmount);
            _isRegistering = false;
        }
    }

    public void TryShowActivityPopup()
    {
        if (CanShowActivityPopup())
        {
            UIManager.Instance.ShowPopup<RegisterMaxWinUiController>(UIManager.ChristmasActivityUiPath);
            UserDeviceLocalData.Instance.LastOpenChristmasMaxWInUiDate = NetworkTimeHelper.Instance.GetNowTime();
        }

        TryShowBillboard();
    }

    bool CanShowActivityPopup()
    {
        bool result = false;
        DateTime lastOpenMaxWinUiDateTime = UserDeviceLocalData.Instance.LastOpenChristmasMaxWInUiDate;
        bool isInCoolDownTime = (NetworkTimeHelper.Instance.GetNowTime() - lastOpenMaxWinUiDateTime).TotalHours < _maxWinUiCooldownHour;
        if (TimeUtility.IsBetweenRange(CurActivityDateInfo.StartDate, CurActivityDateInfo.EndDate) && !isInCoolDownTime)
        {
            result = true;
        }

        return result;
    }

    void TryShowBillboard()
    {
        if (TimeUtility.IsBetweenRange(CurActivityDateInfo.StartDate, CurActivityDateInfo.EndDate))
        {
            GameObject maxWin = UGUIUtility.InstantiateUI(UIManager.ChristmasMaxWinBillboardPath);
            maxWin.SetActive(false);
            ChristmasBillboard billboard = maxWin.GetComponent<ChristmasBillboard>();
            BillBoardManager.Instance.Add(billboard);
        }
    }

    public void UpdateMaxWinAmount(RegisterWinAmountEvent e)
    {
        if (e.WinAmount > UserBasicData.Instance.MaxWinDuringActivity)
        {
            UserBasicData.Instance.MaxWinDuringActivity = e.WinAmount;
            SendDataToServer();
        }
    }

    void SendDataToServer()
    {
        if (DeviceUtility.IsConnectInternet())
        {
            Dictionary<string, object> sendDic = new Dictionary<string, object>();
            Dictionary<string, object> argsDic = new Dictionary<string, object>();
            argsDic.Add("MaxWinRecord", UserBasicData.Instance.MaxWinDuringActivity);

            sendDic.Add("ProjectName", BuildUtility.GetProjectName());
            sendDic.Add("UDID", UserBasicData.Instance.UDID);
            //add startDate to distinguish different round of activity
            sendDic.Add("EventName", _activityName + CurActivityDateInfo.StartDate.ToString("yyyyMMdd"));
            sendDic.Add("Params", argsDic);

            StartCoroutine(NetWorkHelper.UniversalNetCall(UnityTimer.Instance, _reqTimeoutTime, ServerConfig.GameServerUrl, _urlSuffix,
                sendDic,
                jsonObj =>
                {
                    LogUtility.Log(
                        "ChristmasMaxWinActivity: save data to server successfully, max win is : " + argsDic["MaxWinRecord"],
                        Color.yellow);
                },
                jsonObj => { Debug.LogError("ChristmasMaxWinActivity: fail to save data to server"); }));
        }
    }

    public void HandleLocalNotification()
    {
        if (_dateInfoList == null)
        {
            LoadDateConfig();
        }
        ListUtility.ForEach(_dateInfoList, info =>
        {
            SendLocalNotification(info.EndDate, "notify_christmas_title", "notify_christmas_maxwin", NotificationUtility.NotifyType.MaxWinBonus);
        });
    }

    void SendLocalNotification(DateTime sendDate, string titleKey, string contentKey, NotificationUtility.NotifyType type)
    {
        if (!TimeUtility.IsDatePast(sendDate))
        {
            string title = LocalizationConfig.Instance.GetValue(titleKey);
            string notifyContent = LocalizationConfig.Instance.GetValue(contentKey);
            double delayTime = TimeUtility.CountdownOfDateFromNowOn(sendDate).TotalSeconds;
            NotificationUtility.Instance.NotificationMessage(type, notifyContent, title, delayTime);
        }
    }
}

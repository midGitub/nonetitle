using CitrusFramework;
using UnityEngine;
using UnityEngine.Events;

public class VIPSystem : Singleton<VIPSystem>
{
	private LevelData _currVIPLevelData;
	public LevelData GetCurrVIPLevelData
	{
		get
		{
			UpdateLevelData();
			return _currVIPLevelData;
		}
	}

	public VIPData GetCurrVIPInforData { get { return VIPConfig.Instance.FindVIPDataByLevel((int)GetCurrVIPLevelData.Level); } }
	public VIPData GetNextLevelVIPInforData { get { return VIPConfig.Instance.FindVIPDataByLevel((int)(GetCurrVIPLevelData.Level + 1)); } }
	public UnityEvent VIPLevelDataChangeEvent = new UnityEvent();

	private void Awake()
	{
		_currVIPLevelData = new LevelData
		{
			LevelPoint = UserBasicData.Instance.VIPPoint,
			Level = VIPConfig.Instance.GetPointAboutVIPLevel(UserBasicData.Instance.VIPPoint)
		};

		CitrusEventManager.instance.AddListener<UserDataLoadEvent>(UpdateLevelDataMessageP);
	}

	public void UpdateLevelDataMessageP(UserDataLoadEvent loadms)
	{
		UpdateLevelData();
	}

	void UpdateLevelData()
	{
		_currVIPLevelData.LevelPoint = UserBasicData.Instance.VIPPoint;
		_currVIPLevelData.Level = VIPConfig.Instance.GetPointAboutVIPLevel(UserBasicData.Instance.VIPPoint);
	}

	public void AddVIPPoint(int point, bool nowSave = true)
	{
		if(point <= 0)
			return;
		
		int lastLevel = (int)_currVIPLevelData.Level;
		_currVIPLevelData.LevelPoint += point;
		_currVIPLevelData.Level = VIPConfig.Instance.GetPointAboutVIPLevel((int)_currVIPLevelData.LevelPoint);
		UserBasicData.Instance.SetVIPPoint((int)_currVIPLevelData.LevelPoint, nowSave);

		LogUtility.Log("当前vip等级" + _currVIPLevelData.Level + "当前VIP点数" + _currVIPLevelData.LevelPoint, Color.blue);

		VIPLevelDataChangeEvent.Invoke();

	    if (_currVIPLevelData.Level > lastLevel)
	    {
            GameObject vipUnlockNotify = UIManager.Instance.LoadPopupAtPath(UIManager.VipMachineUnlockNotifyUiPath);
            VipMachineUnlockNotifyUiController controller = vipUnlockNotify.GetComponent<VipMachineUnlockNotifyUiController>();
            controller.HandleVipLvUp(_currVIPLevelData);
            AnnunciationVipLvUpManager.Instance.SendVipLvupEventToServer();
            AnalysisManager.Instance.VIPLevelUp((int)_currVIPLevelData.Level);
        }
	}
}

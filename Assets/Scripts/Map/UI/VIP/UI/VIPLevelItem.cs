using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VIPLevelItem : MonoBehaviour
{
	public int VipLevelID;
	private VIPData currVIPData;

	public Text LevelText;
	private Color _levelC;
	public Text VIPPointNeededText;
	private Color _vIPC;
	public Text BuyCreditsText;
	private Color _buyCreditsC;
	public Text DailyBonusText;
	private Color _dailyBonusC;
	public Text HourlyBonusText;
	private Color _hourlyBonusC;
	public Image IconImage;
	public Image LineImage;
	public Text CommingSoon;
	private Color _commingC;
	public GameObject SelectShowGameObject;

	Color newc = new Color(46f / 255f, 5f / 255f, 3f / 255f);
	// public Text ExclusiveMachineText;

	// Use this for initialization
	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		GetVIPData();
		AssignmentText(currVIPData);
	}

	private void GetVIPData()
	{
		currVIPData = VIPConfig.Instance.FindVIPDataByLevel(VipLevelID);
	}

	private void Init()
	{
		_levelC = LevelText.color;
		_vIPC = VIPPointNeededText.color;
		_buyCreditsC = BuyCreditsText.color;
		_dailyBonusC = DailyBonusText.color;
		_hourlyBonusC = HourlyBonusText.color;
		_commingC = CommingSoon.color;
	}


	public void AssignmentText(VIPData vipData)
	{
		LevelText.text = vipData.VIPLevelName;
		IconImage.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(vipData.VIPLevelName);
		LineImage.sprite = VIPConfig.Instance.GetDiamondLineByLevelName(vipData.VIPLevelName);
		VIPPointNeededText.text = StringUtility.FormatNumberStringWithComma((ulong)vipData.VIPLevelNeedPoint);
		BuyCreditsText.text = "+" + (vipData.StoreAddition * 100) + "%";
		DailyBonusText.text = "+" + (vipData.DailyBonusAddition * 100) + "%";
		HourlyBonusText.text = "+" + (vipData.HourBonusAddition * 100) + "%";
		if(VipLevelID == VIPSystem.Instance.GetCurrVIPLevelData.Level)
		{
			SelectShowGameObject.SetActive(true);
			SetColorAndScale(newc, Vector3.one * 1.1f);

		}
		else
		{
			SelectShowGameObject.SetActive(false);
			BackColor();

		}
	}

	private void BackColor()
	{
		LevelText.color = _levelC;
		VIPPointNeededText.color = _vIPC;
		BuyCreditsText.color = _buyCreditsC;
		DailyBonusText.color = _dailyBonusC;
		HourlyBonusText.color = _hourlyBonusC;
		CommingSoon.color = _commingC;
		LevelText.transform.localScale = Vector3.one;
		VIPPointNeededText.transform.localScale = Vector3.one;
		BuyCreditsText.transform.localScale = Vector3.one;
		DailyBonusText.transform.localScale = Vector3.one;
		HourlyBonusText.transform.localScale = Vector3.one;
		CommingSoon.transform.localScale = Vector3.one;
	}

	private void SetColorAndScale(Color co, Vector3 vs)
	{
		LevelText.color = co;
		VIPPointNeededText.color = co;
		BuyCreditsText.color = co;
		DailyBonusText.color = co;
		HourlyBonusText.color = co;
		CommingSoon.color = co;

		LevelText.transform.localScale = vs;
		VIPPointNeededText.transform.localScale = vs;
		BuyCreditsText.transform.localScale = vs;
		DailyBonusText.transform.localScale = vs;
		HourlyBonusText.transform.localScale = vs;
		CommingSoon.transform.localScale = vs;
	}
}

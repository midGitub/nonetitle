using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VIPStoreBarUI : MonoBehaviour
{
	public ImageBar VipImagebar;
	public Image VipIco;
	public Text DescribeText;

	public Image CurrLevelIcon;
	public Image NextLevelIcon;

	// Use this for initialization
	private void OnEnable()
	{
		VIPSystem.Instance.VIPLevelDataChangeEvent.AddListener(UpdateShowInfor);
		UpdateShowInfor();
	}

	private void OnDisable()
	{
		if(VIPSystem.Instance != null)
		{
			VIPSystem.Instance.VIPLevelDataChangeEvent.RemoveListener(UpdateShowInfor);
		}
	}

	public void LearnMoreButtonDown()
	{
//		VIPUIController.Instance.ShowVIPInforUI(true);
		VIPUIController.Instance.Show(true);
	}

	private void UpdateShowInfor()
	{
		var currvipdata = VIPSystem.Instance.GetCurrVIPInforData;
		var nextvipdata = VIPSystem.Instance.GetNextLevelVIPInforData;
		var currLevel = VIPSystem.Instance.GetCurrVIPLevelData;
		CurrLevelIcon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(currvipdata.VIPLevelName);

		NextLevelIcon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(nextvipdata.VIPLevelName);

		VipImagebar.ChangeBarState(currvipdata.VIPLevelNeedPoint, nextvipdata.VIPLevelNeedPoint, currLevel.LevelPoint);
		VipIco.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(currvipdata.VIPLevelName);
		var needPoint = nextvipdata.VIPLevelNeedPoint - currLevel.LevelPoint;
		DescribeText.text = "ONLY " + StringUtility.FormatNumberStringWithComma((ulong)needPoint) + " Points left to become next VIP!";

	}
}

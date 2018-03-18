using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPiggyBank : MonoBehaviour 
{
    // Xhj 曾今是在破产时，smallBuy界面的关闭按钮被点击时调用，现在作用被PopWindowConfig替代
    [System.Obsolete("Replaced by PopWindowConfig")]
	public void ShowPiggyBankUi()
	{
		PiggyBankUIController.Instance.OpenUI();
		string names = "Lobby";
		if(ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
		{
			names = "GameUp";
		}
		StoreController.Instance.CurrStoreAnalysisData = new StoreAnalysisData();
		StoreController.Instance.CurrStoreAnalysisData.OpenPosition = names;
		StoreController.Instance.CurrStoreAnalysisData.StoreEntrance = StoreType.PiggyBank.ToString();
		AnalysisManager.Instance.OpenShop();
	}
}

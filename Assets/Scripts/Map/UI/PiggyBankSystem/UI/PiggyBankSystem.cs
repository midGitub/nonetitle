using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using System;

public class PiggyBankSystem : Singleton<PiggyBankSystem>
{
	public PiggyBankData CurrPiggyBankData { get; private set; }

	public Action PiggyBankAddCoinsAction;

	// Use this for initialization
	void Awake()
	{
		UpdateData();
		CitrusEventManager.instance.AddListener<UserDataLoadEvent>(UpdateLevelDataMessage);
	}

	public void UpdateData()
	{
		CurrPiggyBankData = PiggyBankConfig.Instance.FindPiggyBankDataWithCoins(UserBasicData.Instance.PiggyBankCoins);
		//LogUtility.Log("当前的金币转化率" + CurrPiggyBankData.ConversionRate + "ID" + CurrPiggyBankData.ID, Color.cyan);
	}

	private void UpdateLevelDataMessage(UserDataLoadEvent loadms)
	{
		UpdateData();
	}

	/// <summary>
	/// 添加转化之前的金币
	/// </summary>
	/// <param name="coins">Coins.</param>
	public void AddCoinsInPiggyBank(int coins)
	{

		if(PiggyBankAddCoinsAction != null)
		{
			PiggyBankAddCoinsAction();
		}

		//LogUtility.Log("处理" + coins, Color.cyan);
		while(CurrPiggyBankData.MaxCredits != 0)
		{
			// 此等级最高级还剩下的钱
			int lastCoins = CurrPiggyBankData.MaxCredits - UserBasicData.Instance.PiggyBankCoins;// 整数
			int needallCoins = Mathf.RoundToInt(lastCoins / CurrPiggyBankData.ConversionRate);// 整数
																							  // 钱超过此等级所需要的真的钱数钱数
			if(coins > needallCoins)
			{
				//LogUtility.Log("当前的金币转化率" + CurrPiggyBankData.ConversionRate + "ID" + CurrPiggyBankData.ID, Color.cyan);

				coins -= needallCoins;
				var newcoins = UserBasicData.Instance.PiggyBankCoins + lastCoins;
				UserBasicData.Instance.SetPiggyBankCoins(newcoins, false);
				//LogUtility.Log("剩下的需处理金币数" + coins, Color.cyan);
				//LogUtility.Log("当前的小猪金币" + UserBasicData.Instance.PiggyBankCoins, Color.cyan);
				UpdateData();
				//LogUtility.Log("当前的金币转化率" + CurrPiggyBankData.ConversionRate + "ID" + CurrPiggyBankData.ID, Color.cyan);
			}
			else { break; }
		}

		UpdateData();

		//LogUtility.Log("剩下的需处理金币数" + coins, Color.cyan);
		// 四舍五入加钱
		coins = Mathf.RoundToInt(coins * CurrPiggyBankData.ConversionRate);
		//Debug.Log("钱数" + coins);
		//LogUtility.Log("当前的金币转化率" + CurrPiggyBankData.ConversionRate + "ID" + CurrPiggyBankData.ID, Color.cyan);
		var lastcoins = UserBasicData.Instance.PiggyBankCoins + coins;

		// don't save for better performance. not save until the spin ends
		UserBasicData.Instance.SetPiggyBankCoins(lastcoins, false);

		//LogUtility.Log("当前的小猪金币" + UserBasicData.Instance.PiggyBankCoins, Color.cyan);
		// 当前的Data已经是最大了不需要刷新,无限大
		if(CurrPiggyBankData.MaxCredits == 0)
		{
			//LogUtility.Log("当前的金币转化率最有一个等级" + CurrPiggyBankData.ConversionRate + "ID" + CurrPiggyBankData.ID, Color.cyan);
			return;
		}

		if(UserBasicData.Instance.PiggyBankCoins >= CurrPiggyBankData.MaxCredits)
		{
			UpdateData();

			//LogUtility.Log("当前的金币转化率" + CurrPiggyBankData.ConversionRate + "ID" + CurrPiggyBankData.ID, Color.cyan);
		}
	}
}

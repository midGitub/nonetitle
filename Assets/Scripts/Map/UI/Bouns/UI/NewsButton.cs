using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewsButton : MonoBehaviour
{
	private static NewsButton _instance;
	public static NewsButton Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<NewsButton>();
			}

			return _instance;
		}
	}

	private static bool _isShowed = false;
	public static bool IsShowed { get { return _isShowed; } }

	private void Awake()
	{
		_instance = this;

		//测试 之后删除
		//UserBasicData.Instance.IsFistStartGameBonus = true;
	}

	public void ShowNewsComing()
	{
		ComingSoon.Instance.ShowNoNews();
	}

	//public void ClickGetCoins()
	//{
	//	if(UserBasicData.Instance.IsFistStartGameBonus)
	//	{
	//		BonusHelper.GetFirstBonus();
	//		FindObjectOfType<BonusButton>().ShowCollectioningCoinsEffect();
	//	}
	//}
}

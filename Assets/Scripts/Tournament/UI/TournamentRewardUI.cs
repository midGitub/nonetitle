using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TournamentRewardUI : MonoBehaviour
{
	public Text Num;
	public Text Coins;
	public Canvas ThisCanvas;
	public float TextAnimationTime = 2;

	private Dictionary<int, string> _st = new Dictionary<int, string>()
	{ {1,"1st"},{2,"2nd"},{3,"3rd"},{4,"4th"},{5,"5th"}
	};

	//
	private void OnEnable()
	{
		ThisCanvas.worldCamera = Camera.main;
	}

	//public void Collect()
	//{
	//	BonusManager.Instance.GetTournamentReward();
	//}

	public void SetValue(int rank, ulong coins)
	{
		Num.text = _st[rank];
		StartCoroutine(TextExtension.IETickNumber(0, coins, TextAnimationTime, (obj) => { Coins.text = StringUtility.FormatNumberStringWithComma(obj); }));
	}
}

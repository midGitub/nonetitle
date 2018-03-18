using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuySuccessUI : MonoBehaviour
{
	public Image Icon;
	public Text Coins;
	public Text VipPoint;

	public void Show(Sprite icon, ulong coins, string vipPoint)
	{
		Icon.sprite = icon;
		Icon.SetNativeSize();
		Icon.transform.localScale = Vector3.one * 2f;
		StartCoroutine(Effect(coins));
		VipPoint.text = "+" + vipPoint;
	}

	private IEnumerator Effect(ulong currCoins)
	{
		float allTime = 2f;// 音频的时间 的一半
		float startTime = 0;
		AudioManager.Instance.PlaySound(AudioType.CreditsRollUp);
		while(startTime <= allTime)
		{
			startTime += Time.deltaTime;
			ulong coins = (ulong)Mathf.Lerp(0, currCoins, startTime / allTime);
			Coins.text = StringUtility.FormatNumberStringWithComma((ulong)coins);
			yield return null;
		}
	}
}

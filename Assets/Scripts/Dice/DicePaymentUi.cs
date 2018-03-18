using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DicePaymentUi : MonoBehaviour {

	[SerializeField]
	private Text _winCredits;	
	[SerializeField]
	private Button _collectButton;	

	public void Show(ulong coins)
	{
		gameObject.SetActive(true);
		_winCredits.text = "0";
		StartCoroutine(Effect(coins));
	}

	public void Hide()
	{
		AudioManager.Instance.StopSound(AudioType.M10_WheelBGM);
		gameObject.SetActive(false);
	}

	private IEnumerator Effect(ulong currCoins)
	{
		float allTime = 2f;// 音频的时间的一半
		float startTime = 0;
		AudioManager.Instance.PlaySound(AudioType.CreditsRollUp);
		while(startTime <= allTime)
		{
			startTime += Time.deltaTime;
			ulong coins = (ulong)Mathf.Lerp(0, currCoins, startTime / allTime);
			_winCredits.text = StringUtility.FormatNumberStringWithComma((ulong)coins);
			yield return null;
		}
	}
}

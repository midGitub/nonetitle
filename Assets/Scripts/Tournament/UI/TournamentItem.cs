using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TournamentItem : MonoBehaviour
{
	public Image Icon;
	public Text Num;
	public Text Score;

	public Sprite DeflutSprite;

	public GameObject RewardGameObject;
	public Text RewardText;

	private string _currIconUrlMD5 = "";

	public void SetValue(string udid, int rank, ulong score, ulong rewardcoins, int state, string iconurl)
	{
		_currIconUrlMD5 = StringUtility.GetStringMD5(iconurl);

		//根据UDID得到图片
		Sprite sp = SpriteStoreManager.Instance.GetSprite(_currIconUrlMD5);
		if(sp != null)
		{
			Icon.sprite = sp;
		}
		else
		{
			SpriteStoreManager.Instance.DownloadSprite(_currIconUrlMD5, iconurl);
			Icon.sprite = DeflutSprite;
			//StartCoroutine(WaitUpdatePicture());
		}
		Num.text = "#" + rank.ToString();

		var scoretext = StringUtility.FormatNumberString(score, true, true);
		if(scoretext.Contains("."))
		{
			string[] sl = scoretext.Split('.');
			// 得到保留小数点后一位
			string lst = sl[0] + "." + sl[1].ToCharArray()[0] + "M";
			Score.text = lst;
		}
		else
		{
			Score.text = scoretext;
		}
		if(state == 0)
		{
			RewardGameObject.SetActive(true);
			RewardText.text = StringUtility.FormatNumberString(rewardcoins, true, true);

		}
		else
		{
			RewardGameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		Sprite sp = SpriteStoreManager.Instance.GetSprite(_currIconUrlMD5);
		if(sp == null)
		{
			StartCoroutine(WaitUpdatePicture());
		}
	}

	private IEnumerator WaitUpdatePicture()
	{
		while(true)
		{
			// 1秒刷新一次
			yield return new WaitForSeconds(1);
			Sprite sp = SpriteStoreManager.Instance.GetSprite(_currIconUrlMD5);
			if(sp != null)
			{
				Icon.sprite = sp;
				yield break;
			}
		}
	}
}

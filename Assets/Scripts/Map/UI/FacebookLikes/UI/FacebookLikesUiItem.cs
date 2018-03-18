using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using UnityEngine.Events;

public class FacebookLikesUiItem : MonoBehaviour {

	public Button LikeUsButton;
	public Image RewardImg;
	public Text RewardText;
	public Text FanText;

	private readonly string likeUsText = "LIKE US";
	private readonly string fanPageText = "FANPAGE";

	private bool _pauseByMe;

	void OnEnable()
	{
		SetUiState();
	}

	private void SetUiState()
	{
		bool isOurFan = UserBasicData.Instance.LikeOurAppInFacebook;

		RewardImg.gameObject.SetActive(!isOurFan);
		RewardText.gameObject.SetActive(!isOurFan);
		FanText.text = isOurFan ? fanPageText : likeUsText;

		RewardText.text = "+" + StringUtility.FormatNumberString((ulong)CoreConfig.Instance.MiscConfig.FacebookLikesReward, true, true);
	}

	public void OnLikeUsButtonPressed()
	{
		FacebookLikes.Instance.JumpToFacebookPage();
		_pauseByMe = true;
	}

	void OnApplicationPause(bool isPause)
	{
        LogUtility.Log("FBLike Module: is app paused : " + isPause);

		if(!isPause)
		{
			if(_pauseByMe)
			{
                LogUtility.Log("FBLike Module: pauseByMe!" );
                //check if user is alread our facebook fan
                bool isOurFan = UserBasicData.Instance.LikeOurAppInFacebook;

				if (!isOurFan) 
				{
					#if UNITY_EDITOR
					#else 
					FacebookLikes.Instance.CheckUserLikes();
					#endif 
				} 

				_pauseByMe = false;
			}
		}
	}

    void OnApplicationFocus(bool focus)
    {
        LogUtility.Log("FBLike Module: is app get focus : " + focus);
    }
}

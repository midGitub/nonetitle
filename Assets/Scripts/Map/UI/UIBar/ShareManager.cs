using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
#if Trojan_FB
using Facebook.Unity;
#endif

public enum SharePlace
{
	BigWin,
	EpicWin,
	Tournament,
	JACKPOT,
	None,
}


public class ShareManager : Singleton<ShareManager>
{
	// 锦标赛级别
	private static readonly string[] _rankStrArray = new string[]{ 
		"1st", "2nd", "3rd", "4th", "5th"
	};

	// 锦标赛等级图片
	private static readonly string[] _tournamentJPGArray = new string[]{
		"tournament01.jpg", "tournament02.jpg", "tournament03.jpg", "tournament04.jpg", "tournament05.jpg"
	};

	// 分享描述
	private static readonly string _bigwinDesc = "I Big-Won in HUGE WIN SLOTS with just one spin. Beat that – click to join me now!";
	private static readonly string _epicwinDesc = "I Epic-Won in HUGE WIN SLOTS with just one spin. Beat that – click to join me now!";
	private static readonly string _jackpotDesc = "I Won JACKPOT in HUGE WIN SLOTS!!!!!!! Beat that – click to join me now!";
	private static readonly string _tournamentDesc = "I won {0} in tournament. Join me now and win the grand prize of $10,000,000!";

	public string DefultTitle = "Huge Win Slots";
	public bool TournamentCanShare = true;
	/// <summary>
	/// 只有修改了分享内容才能分享
	/// </summary>
	/// <param name="shareplace">Shareplace.</param>
	/// <param name="text">Text.</param>
	public string GetShareText(SharePlace shareplace, string text, int tournamentRank = 1)
	{
		switch (shareplace)
		{
		#if false
			case SharePlace.BigWin:
				return "I got $" + text + " winning prize on Huge Win Slots. Get yourself a Big Win too! Click to join now!";
			case SharePlace.Tournament:
				return "I got " + text + "st place in tournament. Click to see if you can do better!";
			case SharePlace.EpicWin:
				return "I got $" + text + " winning prize on Huge Win Slots. Get yourself a Epic Win too! Click to join now!";
			case SharePlace.JACKPOT:
				return "I hit a JACKPOT on Huge Win Slots. Yours is just a click away! Click to join now!";
		#else
			case SharePlace.BigWin:
				return _bigwinDesc;
			case SharePlace.Tournament:
				return string.Format(_tournamentDesc, _rankStrArray[tournamentRank - 1]);
			case SharePlace.EpicWin:
				return _epicwinDesc;
			case SharePlace.JACKPOT:
				return _jackpotDesc;
		#endif
			case SharePlace.None:
				return text;

			default:
				return "";
		}
	}

	// 根据当前奖励获得分享图片URL
	public string GetSharePhotoURL(SharePlace shareplace, int tournamentRank = 1)
	{
		switch (shareplace)
		{
			case SharePlace.BigWin:
				return ServerConfig.SharePhotoBaseURL + "bigwin.jpg";
			case SharePlace.Tournament:
				return ServerConfig.SharePhotoBaseURL + _tournamentJPGArray [tournamentRank - 1];
			case SharePlace.EpicWin:
				return ServerConfig.SharePhotoBaseURL + "epicwin.jpg";
			case SharePlace.JACKPOT:
				return ServerConfig.SharePhotoBaseURL + "jackpot.jpg";
			case SharePlace.None:
			default:
				return ServerConfig.SharePhotoURL;
		}
	}

	public void PublishEasyShare(string applink, string contentDescription, string contentTitle = "", string photoURL = null, FacebookDelegate<IGraphResult> callback = null)
	{
#if Trojan_FB

		FacebookHelper.PublishEasyShare(applink, contentDescription, contentTitle, photoURL, callback);
#endif
	}
#if Trojan_FB
	private void ShareToFaceBookWithScreeSort()
	{
		ScreenShotMakeTexture.TakeScreenshot(NetworkTimeHelper.Instance, (obj) =>
		{
			FacebookUtility.UploadScreenShotAndShare(obj.EncodeToPNG(), null);
			Debug.Log("发布截图视频成功");
		});
	}
#endif
}

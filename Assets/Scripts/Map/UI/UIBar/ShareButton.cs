using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
#if Trojan_FB
using Facebook.Unity;
#endif

public class ShareButton : MonoBehaviour
{
	public Toggle SToggle;
	public UnityEvent ShareFinishedEvent = new UnityEvent();
	public bool CanShare = false;
	//private bool _thisTimeShared = false;

	private string _currString = "";
	private string _currSharePhotoURL = ServerConfig.SharePhotoURL;
	private static bool _isOpenedFBASKBOX = false;

	/// <summary>
	/// 只有修改了分享内容才能分享
	/// </summary>
	/// <param name="shareplace">Shareplace.</param>
	/// <param name="text">Text.</param>
	public void ChangeShareText(SharePlace shareplace, string text, int tournamentRank = 1)
	{
		_currString = ShareManager.Instance.GetShareText(shareplace, text, tournamentRank);
		_currSharePhotoURL = ShareManager.Instance.GetSharePhotoURL(shareplace, tournamentRank);

		CanShare = true;
		//_thisTimeShared = false;
	}
#if Trojan_FB
	public void ShareButtonDown()
	{
		if(!CanShare)
		{
			return;
		}
		CanShare = false;

		// 实例化了并且登陆了
		if(FB.IsInitialized && FacebookHelper.IsLoggedIn && SToggle.isOn)
		{
			// 有权限
			if(FacebookHelper.HavePublishActions)
			{
				Debug.Log("有权限");
				ShareHelp();
			}
			else
			{
				// 登录没有权限 并且没有打开过分享,或是玩家想打开了 我们就弹
				if(!_isOpenedFBASKBOX || SToggle.isOn)
				{
					Debug.Log("打开了窗口");
					// 不管是否获取权限都去发送一下 然后finished 如果获取了就发送 不获取发送后无作用
					FacebookHelper.LoginPublishWithFB((result) => { ShareHelp(); _isOpenedFBASKBOX = true; });
				}
				else
				{
					Debug.Log("box关闭并且已经展示了");
					ShareFinishedEvent.Invoke();
				}
			}
		}
		else
		{
			ShareFinishedEvent.Invoke();
		}
	}

	private void OnEnable()
	{
		// 在提示一次申请权限并且用户还没有给我们权限的时候 关闭toggle其他的时候打开
		if(_isOpenedFBASKBOX && !FacebookHelper.HavePublishActions)
		{
			SToggle.isOn = false;
		}
	}

	private void ShareHelp()
	{
		// 分享打开并且Facebook 登陆了开始进行分享
		if(SToggle.isOn && FacebookHelper.IsLoggedIn)
		{
			//_thisTimeShared = true;
			SToggle.isOn = false;
			ShareToFaceBook(_currString);
			Debug.Log("分享成功");
		}
		else { Debug.Log("玩家不分享" + _currString); }

		ShareFinishedEvent.Invoke();
	}

	private void ShareToFaceBook(string dir)
	{
		ShareManager.Instance.PublishEasyShare(ServerConfig.APPLINK, dir, ShareManager.Instance.DefultTitle, _currSharePhotoURL);
		//NetworkTimeHelper.Instance.StartCoroutine(ShareHelper(dir, (obj) => { ShareFinishedEvent.Invoke(); Debug.Log("玩家分享成功"); Debug.Log(dir); }));
	}

	//private IEnumerator ShareHelper(string dir, FacebookDelegate<IShareResult> callback)
	//{
	//	FacebookHelper.EasyShareInformation
	//			  (
	//					  new Uri(ServerConfig.APPLINK)
	//				  ,
	//					dir
	//				  , "Huge Win Slots",
	//				  new Uri(ServerConfig.SharePhotoURL),
	//				  callback
	//);
	//	yield return new WaitForEndOfFrame();
	//}
#else
	public void ShareButtonDown()
	{
	ShareFinishedEvent.Invoke();
	}
#endif
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System.Runtime.InteropServices;

public enum NoGiftType{
	NotExist,
	RunOut,
	AlreadyGot,
	Channel,
	Max,
}

public class GiftHelper : Singleton<GiftHelper> {
	private string _code = "";
	private string _type = "";
	private string _url = "";
	private bool _isFromURL = false;// 是否从url进的游戏
	private bool _shouldOpenMail = false;// 是否需要打开邮箱
	private int _sendCount = 0;// 统计从url进来次数，测试用
	private bool _isSendURLCD = false;// 是否处于sendurl的冷却中（这里设置这个参数，是因为我发现从url进来，可能会连续调用2次我们sendurl的接口）
	private bool _isApplicationResume = false;// 是否从后台进入
	private float _mailUIDelay;// 邮箱界面弹窗延迟
	private float _nogiftUIDelay;// 提示界面弹窗延迟
	private static readonly string _typeTag = "type";
	private static readonly string _codeTag = "code";
	private static readonly string _getGiftProtocal = "gift/get";
	private static readonly float _cdTime = 3.0f;// 刷新冷却时间

	private static readonly float _mailDelayWhenStart = 1.5f;
	private static readonly float _nogiftDelayWhenStart = 4.0f;
	private static readonly float _mailDelayWhenResume = 1.0f;
	private static readonly float _nogiftDelayWhenResume = 1.0f;

	public string GiftType {
		get { return _type; }
	}

	public string GiftCode {
		get { return _code; }
	}

	public bool IsFromURL{
		get { return _isFromURL; }
		set { _isFromURL = value; }
	}

	public bool ShouldOpenMail{
		get { return _shouldOpenMail; }
		set { _shouldOpenMail = value; }
	}


	void Start(){
		// Test();
		CitrusEventManager.instance.AddListener<GiftMailEvent>(GetGiftMailFunc);
		CitrusEventManager.instance.AddListener<UpdateMailBarStateEvent>(AutoOpenMailUIFunc);

		_mailUIDelay = _mailDelayWhenStart;
		_nogiftUIDelay = _nogiftDelayWhenStart;
	}

	/// <summary>
	/// Callback sent to all game objects when the player pauses.
	/// </summary>
	/// <param name="pauseStatus">The pause state of the application.</param>
	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus){
			// 从后台切入
			_mailUIDelay = _mailDelayWhenResume;
			_nogiftUIDelay = _nogiftDelayWhenResume;
			_isApplicationResume = true;

			LogUtility.Log("gift come from resume", Color.yellow);
		}
	}

	public void SentURL(string url){
		// analyze url
		// get code
		++_sendCount;
		LogUtility.Log("Sent url = " + url + " count = " + _sendCount, Color.yellow);
		AnalyzeCode(url);

		// if (!_type.Equals("default"))
		if (_type == "gift" && !_isSendURLCD)
		{
			_isFromURL = true;
			_isSendURLCD = true;
			StartCoroutine(CoolDownUpdate());
			LogUtility.Log("_isFromURL = true", Color.yellow);
		}
	}

	private IEnumerator CoolDownUpdate(){
		yield return new WaitForSeconds(_cdTime);
		// 结束冷却
		_isSendURLCD = false;
	}

	private void AnalyzeCode(string url){
		_url = url;
		_type = StringUtility.AnalyzeURL (url, _typeTag);
		_code = StringUtility.AnalyzeURL (url, _codeTag);
	}

	// 激活码验证
	public IEnumerator VerifyCode(){
		LogUtility.Log("Gift Verify Code", Color.yellow);

		if (DeviceUtility.IsConnectInternet()){
			// 构建协议
			Dictionary<string, object> dict = new Dictionary<string, object> ();
			dict.Add ("ProjectName", BuildUtility.GetProjectName ());
			dict.Add ("UDID", UserBasicData.Instance.UDID);
			dict.Add ("Channel", PlatformManager.Instance.GetServerChannelString());
			dict.Add ("Code", _code);

			yield return StartCoroutine (NetWorkHelper.UniversalNetCall (this, 10, ServerConfig.GameServerUrl,
				_getGiftProtocal, dict, HandleSuccess, HandleFailed));
		}else{
			NoCollectionErrorObject.Instance.Show();
		}
	}

	private void HandleSuccess(JSONObject json){
		LogUtility.Log("Gift HandleSuccess", Color.yellow);
		// 激活码发送成功,需要打开邮箱界面
		_shouldOpenMail = true;
		StartCoroutine(DelayGetMail());
	}

	private IEnumerator DelayGetMail(){
		yield return new WaitForSeconds(0.5f);
		// 如果是从后台进入，需要刷一次邮箱
		if (_isApplicationResume){
			if (Application.internetReachability != NetworkReachability.NotReachable && !string.IsNullOrEmpty(UserBasicData.Instance.UDID)){
				MailHelper.Instance.GetMail(MailServerFunction.GetReward.ToString());
			}
		}
	}

	private void HandleFailed(JSONObject jSON){
		LogUtility.Log("Gift HandleFailed", Color.yellow);

		// TODO: 激活码发送失败, 就弹界面
		NoGiftType type = NoGiftType.NotExist;
		ServerErrorCode errorCode = (ServerErrorCode)jSON.GetField("error").n;
		if (errorCode == ServerErrorCode.giftAlreadyGot){
			type = NoGiftType.AlreadyGot;
		}else if (errorCode == ServerErrorCode.giftRunOut){
			type = NoGiftType.RunOut;
		}else if (errorCode == ServerErrorCode.giftNotExist){
			type = NoGiftType.NotExist;
		}else if (errorCode == ServerErrorCode.giftChannel){
			type = NoGiftType.Channel;
		}
		StartCoroutine(DelayOpenNoGiftUI(type));
	}

	// 跟服务器通信验证
	public void ServerVerify(){
		LogUtility.Log("ServerVerify", Color.yellow);
		if (_isFromURL){
			StartCoroutine(VerifyCode());
			_isFromURL = false;
		}
	}

	private void GetGiftMailFunc(GiftMailEvent message){
		// 这里感觉需要重新设计一下逻辑，因为我们现在收取邮件都是在场景切换时进行。
		// 而现在是无法保证在进大厅时，礼包邮件已经发送过来了，所以需要在第一遍收取邮件后，再次进行邮件的获取
		if (_shouldOpenMail){
			if (message._mailStr == MailServerFunction.GetMail.ToString()){
				MailHelper.Instance.GetMail(MailServerFunction.GetMail.ToString());
			}
		}
	}

	// 邮件到达后的回调
	private void AutoOpenMailUIFunc(UpdateMailBarStateEvent message){
		if (message.MailType == MailServerFunction.GetReward.ToString()){
			StartCoroutine(DelayOpenMailUI());
		}
	}

	// TODO: 这种方法不好，以后要改
	private IEnumerator DelayOpenNoGiftUI(NoGiftType type){
		yield return new WaitForSeconds(_nogiftUIDelay);
		// 根据回调信息来判断显示哪个界面
		UIManager.Instance.OpenNoGiftUI(type);
	}

	private IEnumerator DelayOpenMailUI(){
		yield return new WaitForSeconds(_mailUIDelay);
		AutoOpenMailUI();
	}

	// 自动打开邮箱
	public void AutoOpenMailUI(){
		LogUtility.Log("AutoOpenUI should open = " + _shouldOpenMail, Color.yellow);
		if (_shouldOpenMail){
			UIManager.Instance.OpenMailUI();
			_shouldOpenMail = false;
		}
	}

	public void PrintTypeAndCode(){
		LogUtility.Log("Print url = " + _url, Color.yellow);
		LogUtility.Log("Print type = " + _type, Color.yellow);
		LogUtility.Log("Print code = " + _code, Color.yellow);
	}

	private void Test(){
		string url = "hugewinslots://test?type=gift&code=xxx";
		AnalyzeCode(url);
	}

	private void TestSort(){
		List<int> a = new List<int>{2, 6, 10, 5, 1, 9};
		a.Sort(sortFunc);
		ListUtility.ForEach(a, (int i)=>{
			Debug.Log("a : " + i);
		});
	}

	private int sortFunc(int a, int b){
		if (a > b) return 1;
		else if ( a < b ) return -1;
		else return 0;
	}
}

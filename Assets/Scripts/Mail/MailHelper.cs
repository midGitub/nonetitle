// #define MAIL_DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using MiniJSON;
using System;

public class MailHelper : Singleton<MailHelper>
{
	static readonly char DELIMETER = ',';
	static readonly float _minGetMailSeconds = 120.0f;

	bool GettingMail = false;
	bool GettingReward = false;
	bool GettingBillBoard = false;
	DateTime _lastReceiveMailTime = DateTime.MinValue;

	public void Init()
	{
		CitrusEventManager.instance.AddListener<EnterMainMapSceneEvent>(EnterSceneCallback);
	}

	private void EnterSceneCallback(EnterMainMapSceneEvent e)
	{
		TryGetAllMail();
	}

	private enum FieldName
	{
		MailList,
		SuccessIdArray,
		IdArray,
	}

	public void GetAllMail()
	{
		#if MAIL_DEBUG
		LogUtility.Log("Start Get All Mail", Color.yellow);
		#endif
		if (Application.internetReachability != NetworkReachability.NotReachable
		   && !string.IsNullOrEmpty(UserBasicData.Instance.UDID))
		{
			#if MAIL_DEBUG
			LogUtility.Log("Gettingmail = "+GettingMail + " GettingReward = "+GettingReward + " GettingBillboard = " + GettingBillBoard, Color.yellow);
			#endif
			if(!GettingMail)
				StartCoroutine(GetMailIE(MailServerFunction.GetMail.ToString()));
			if(!GettingReward)
				StartCoroutine(GetMailIE(MailServerFunction.GetReward.ToString()));
			if (!GettingBillBoard)
				StartCoroutine(GetMailIE(MailServerFunction.GetBillBoard.ToString()));

			_lastReceiveMailTime = DateTime.Now;
		}
	}


	public void TryGetAllMail()
	{
		if(ShouldGetAllMail())
			GetAllMail();
	}

	bool ShouldGetAllMail()
	{
		bool result = false;
		if(_lastReceiveMailTime == DateTime.MinValue)
		{
			result = true;
		}
		else
		{
			TimeSpan span = DateTime.Now - _lastReceiveMailTime;
			if(span.TotalSeconds < 0.0)
			{
				Debug.LogError("ShouldGetAllMail: detect time reverses!");
				result = true;
			}
			else if(span.TotalSeconds >= _minGetMailSeconds)
			{
				result = true;
			}
			else
			{
				result = false;
			}
		}
		return result;
	}
	
	public void GetMail(string serverFuncName){
		if (Application.internetReachability != NetworkReachability.NotReachable
		   && !string.IsNullOrEmpty(UserBasicData.Instance.UDID))
		{
			bool shouldGet = false;
			if (serverFuncName == MailServerFunction.GetMail.ToString()){
				shouldGet = !GettingMail;
			}
			else if (serverFuncName == MailServerFunction.GetReward.ToString()){
				shouldGet = !GettingReward;
			}
			else if (serverFuncName == MailServerFunction.GetBillBoard.ToString()){
				shouldGet = !GettingBillBoard;
			}

			if (shouldGet){
				#if MAIL_DEBUG
				LogUtility.Log("GetMail = "+serverFuncName, Color.yellow);
				#endif
				StartCoroutine(GetMailIE(serverFuncName));
				_lastReceiveMailTime = DateTime.Now;
			}
		}
	}

	void SetGettingState(string functionName,bool flag)
	{
		#if MAIL_DEBUG
		LogUtility.Log("set mail state : "+ functionName + " = " + flag, Color.yellow);
		#endif
		if (functionName == MailServerFunction.GetMail.ToString())
			GettingMail = flag;
		else if (functionName == MailServerFunction.GetReward.ToString())
			GettingReward = flag;
		else if (functionName == MailServerFunction.GetBillBoard.ToString())
			GettingBillBoard = flag;
	}

	private IEnumerator GetMailIE(string functionName)
	{
		#if MAIL_DEBUG
		Debug.Log("GetMailIE: call " + functionName);
		#endif
		SetGettingState(functionName, true);
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		bool error = false;
		JSONObject resultJS = null;
		JSONObject errorJS = null;
		yield return StartCoroutine(MailNetWorkHelper.Instance.NetCall(this, functionName, sendDic, 
			(r) => { resultJS = r; },
			(obj) => { error = true; errorJS = obj; }
		));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}

			SetGettingState(functionName, false);
			yield break;
		}

		if (functionName == MailServerFunction.GetMail.ToString())
			yield return StartCoroutine(ProcessMailRJS(resultJS));
		else if (functionName == MailServerFunction.GetReward.ToString())
			ProcessReward(resultJS);
		else if (functionName == MailServerFunction.GetBillBoard.ToString())
			ProcessBillBoard(resultJS);
		SetGettingState(functionName, false);
	}

	void ProcessBillBoard(JSONObject js)
	{
		#if MAIL_DEBUG
		LogUtility.Log("ProcessBillBoard", Color.red);
		#endif
		LogUtility.Log(js.ToString(), Color.red);
		if (js == null || !js.HasField("error") || js.GetField("error").n > 0.1)
			return;
		List<JSONObject> jsonList = js.GetField("NoticeArray").list;
		bool hasNewMail = false;
		for (int i = 0; i < jsonList.Count; i++)
		{
			MailInfor infor = ReadMailInfor(jsonList [i],MailUtility.BillBoardMail,"BonusItems",(int)SystemMailType.BillBoard,"Content");
			UserBasicData.Instance.MailInforDic[ReadJson(jsonList[i],"NoticeId")] = infor;
			hasNewMail = true;
		}
		if (js.HasField("LastNoticeID"))
		{
			if(js.GetField("LastNoticeID").str != null)
				UserBasicData.Instance.LastNoticeID = js.GetField("LastNoticeID").str;
		}

		if (hasNewMail){
			// 提示bar尝试刷新ui
			CitrusEventManager.instance.Raise(new UpdateMailBarStateEvent(MailServerFunction.GetBillBoard.ToString()));
		}

		Debug.Log("LastNoticeID "+UserBasicData.Instance.LastNoticeID);
	}

	void ProcessReward(JSONObject js)
	{
		#if MAIL_DEBUG
		LogUtility.Log("ProcessReward", Color.red);
		#endif
		LogUtility.Log(js.ToString(), Color.red);
		JSONObject res = js;
		if (res.GetField("error").n > 0.1)
			return;
		List<JSONObject> jsonList = res.GetField("BonusArray").list;
		bool hasNewMail = false;
		for (int i = 0; i < jsonList.Count; i++)
		{
			MailInfor infor = ReadMailInfor(jsonList [i],MailUtility.RewardMail,"Items",(int)SystemMailType.Reward,"Desc");
			UserBasicData.Instance.MailInforDic[ReadJson(jsonList[i],"ID")] = infor;
			hasNewMail = true;
		}

		if (hasNewMail){
			// 提示bar尝试刷新ui
			CitrusEventManager.instance.Raise(new UpdateMailBarStateEvent(MailServerFunction.GetReward.ToString()));
		}
	}

	MailInfor ReadMailInfor(JSONObject js,string infoType,string fileName,int systemType,string titleFileName)
	{
		MailInfor info = new MailInfor ();
		info.Title = ReadJson(js, titleFileName, "");
		info.Type = infoType;
		JSONObject resultJson = new JSONObject ();
		if (js.HasField(fileName))
		{
			List<JSONObject> itemsList = js.GetField(fileName).list;
			for (int i = 0; i < itemsList.Count; i++)
			{
				string type = ReadJson(itemsList [i], "Type", "");
				if (itemsList [i].HasField("Number") && type != "")
				{
					int num = (int)itemsList [i].GetField("Number").n;
					if (type == "Credits")
						resultJson.AddField(MailUtility._system_credits, num);
					else if (type == "Lucky")
						resultJson.AddField(MailUtility._system_long_lucky, num);
					else if (type == "Vippoint")
						resultJson.AddField(MailUtility._system_vip_exp, num);
					else if (type == "UserLevel")
						resultJson.AddField(MailUtility._system_level, num);
					else if (type == "UserLevelPoint")
						resultJson.AddField(MailUtility._system_exp, num);
				}
				// TODO：新增优先级
				if (itemsList[i].HasField("Priority")){
					int priority = (int)itemsList [i].GetField ("Priority").n;
					resultJson.AddField (MailUtility._system_priority, priority);
				}
			}
		}
		resultJson.AddField(MailUtility._system_type, systemType);
		info.State = MailState.DoneConfirm;
		info.Message = resultJson.ToString();

		// 收到邮件打点
		AnalysisManager.Instance.ReceiveMail(MailDefine.GetMailTypeString(infoType), 
			MailDefine.GetSystemMailTypeString(systemType));
		return info;
	}

	private string ReadJson(JSONObject js,string file,string defaultvalue = "error")
	{
		if (js.HasField(file))
		{
			return js.GetField(file).str;
		}
		else
			return defaultvalue;
	}
	// 处理得到的信息和以前的信息
	private IEnumerator ProcessMailRJS(JSONObject js)
	{
		#if MAIL_DEBUG
		LogUtility.Log("ProcessMailRJS", Color.red);
		#endif
		LogUtility.Log(js.ToString(), Color.red);
		var newMailDic = (js.GetField(FieldName.MailList.ToString())).ToDictionary();
		// zhousen 获得所有json列表
		List<JSONObject> jsonList = js.GetField (FieldName.MailList.ToString ()).list;

		//var lis = js.GetField(FieldName.MailList.ToString()).list;
		//foreach(var item in lis)
		//{
		//	Debug.Log(item.ToString());
		//}
		//Debug.Log(js.GetField(FieldName.MailList.ToString()).ToString());
		// 筛选等待的字典
		//var waitingDic = UserDeviceLocalData.Instance.MailInforDic.Where((arg1, arg2) => { return arg1.Value.State == MailState.WaitingConfirm; });
		var oldDicKeys = new List<string>(UserBasicData.Instance.MailInforDic.Keys);
		var newDicKeys = new List<string>(newMailDic.Keys);

		// 将为处理的确认状态
		for(int i = 0; i < oldDicKeys.Count; i++)
		{
			var currKey = oldDicKeys[i];
			var currMI = UserBasicData.Instance.MailInforDic[currKey];

			if(currMI.State == MailState.WaitingConfirm)
			{
				// 如果不存在了 就是说明上次在服务器已经删除了但是我没接收到,我就变为可读状态
				if(!newMailDic.ContainsKey(currKey))
				{
					currMI.State = MailState.DoneConfirm;
					LogUtility.Log(currKey + "自动替换成了已收取的", Color.red);
				}
			}
		}
		//  没有新邮件不需要删除 不需要处理
		if(newDicKeys.Count == 0)
		{
			LogUtility.Log("new mail count = 0", Color.yellow);
			// 提示bar尝试刷新ui
			CitrusEventManager.instance.Raise(new UpdateMailBarStateEvent(MailServerFunction.GetMail.ToString()));
			UserBasicData.Instance.Save();
			//  todo 这里发送邮件ui刷新的事件
			yield break;
		}

		// 如果新的或存在Key相同就替换刷新成新的默认Waitconfirm
		// 再把得到的List发送删除
		for(int i = 0; i < newDicKeys.Count; i++)
		{
			#if false
			UserBasicData.Instance.MailInforDic[newDicKeys[i]] = ServerJson2MailInfor(new JSONObject(newMailDic[newDicKeys[i]]));
			#else
			// zhousen 这里直接使用json，而不是用string 再生成一个json
			UserBasicData.Instance.MailInforDic[newDicKeys[i]] = ServerJson2MailInfor(jsonList[i]);//make it show after billboard
			#endif
			LogUtility.Log(newDicKeys[i] + ":" + newMailDic[newDicKeys[i]], Color.black);
			Debug.Log("转化后的信息" + UserBasicData.Instance.MailInforDic[newDicKeys[i]].Message);
		}
		UserBasicData.Instance.Save();
		yield return StartCoroutine(RemoveMailIE(newDicKeys));

		LogUtility.Log("new mail count = " + newDicKeys.Count, Color.yellow);
		// 提示bar尝试刷新ui
		CitrusEventManager.instance.Raise(new UpdateMailBarStateEvent(MailServerFunction.GetMail.ToString()));
	}

	private IEnumerator RemoveMailIE(List<string> willRemoveMK)
	{
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		sendDic.Add(FieldName.IdArray.ToString(), willRemoveMK);
		Debug.Log(Json.Serialize(sendDic));
		yield return null;
		bool error = false;
		JSONObject resultJS = null;
		JSONObject errorJS = null;
		yield return StartCoroutine(MailNetWorkHelper.Instance.NetCall
									(this, MailServerFunction.DeletMail.ToString(), sendDic, (r) => { resultJS = r; }, (obj) =>
		   { error = true; errorJS = obj; }));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}

			GettingMail = false;
			// 这里发送错误信息  
			yield break;
		}
		// 这里发送 得到信息成功信息
		ProcessDeletMailRJS(resultJS);
	}

	private void ProcessDeletMailRJS(JSONObject js)
	{
		var dsLS = js.GetField(FieldName.SuccessIdArray.ToString()).list;
		for(int i = 0; i < dsLS.Count; i++)
		{
			Debug.Log("删除成功的邮件" + dsLS[i].str);
			// 确认可以使用读取的邮件
			UserBasicData.Instance.MailInforDic[dsLS[i].str].State = MailState.DoneConfirm;
			Debug.Log(UserBasicData.Instance.MailInforDic[dsLS[i].str].State);
		}
		UserBasicData.Instance.Save();
		//  todo 这里发送邮件ui刷新的事件
	}

	private MailInfor ServerJson2MailInfor(JSONObject jsob)
	{
		#if false
		MailInfor mf = new MailInfor
		{
			//Title = jsob.GetField("Title").str,
			//Message = jsob.GetField("Msg").str,
			Message = MailTextHelper.Instance.GetText(jsob.GetField("Type").n.ToString(), jsob),
			State = MailState.WaitingConfirm,
			Type = jsob.GetField("Type").n.ToString(),
		};

		if(jsob.HasField("Title"))
		{
			mf.Title = jsob.GetField("Title").str;
		}

		if(jsob.HasField("Bonus"))
		{
			mf.Bonus = (int)jsob.GetField("Bonus").n;
		}
		else { mf.Bonus = 0; }
		#else
		MailInfor mf = new MailInfor ();
		if (jsob.HasField ("Title")) {
			mf.Title = jsob.GetField ("Title").str;
			Debug.Log("ServerJson2MailInfor Title = " + mf.Title);
		}
		if (jsob.HasField("Bonus")) {
			mf.Bonus = (int)jsob.GetField("Bonus").n;
			Debug.Log("ServerJson2MailInfor Bonus = " + mf.Bonus);
		}
		if (jsob.HasField ("Type")) {
			mf.Type = jsob.GetField ("Type").n.ToString ();
			Debug.Log("ServerJson2MailInfor Type = " + mf.Type);
//			mf.Message = MailTextHelper.Instance.GetText (mf.Type, jsob);
		}
		mf.Message = GetJsonMessage(jsob);
		mf.State = MailState.WaitingConfirm;
		
		// 收到邮件打点
		MailInforExtension extension = MailUtility.String2MailInforExtension(mf.Message);
		AnalysisManager.Instance.ReceiveMail(MailDefine.GetMailTypeString(mf.Type), 
			MailDefine.GetSystemMailTypeString((int)extension.SystemType));
		#endif

		return mf;
	}

	private string GetJsonMessage(JSONObject json){
		string message = "";

		if (json.HasField ("Msg")) {
			message = json.GetField("Msg").str;
			Debug.Log("GetJsonMessage = " + message);
		}

		if (json.HasField ("Rank")) {
			int rank = (int)json.GetField ("Rank").n;
			message = message + DELIMETER + rank.ToString ();
		}

		return message;
	}
		
	/// <summary>
	/// 测试用的添加邮件
	/// </summary>
	public void TestAddMail()
	{
		StartCoroutine(TestAddMailIE());
	}

	public IEnumerator TestAddMailIE()
	{
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		Dictionary<string, object> mailDic = new Dictionary<string, object>();

		#if true
		mailDic["Title"] = "test_title";
		mailDic["Type"] = 1;
		mailDic["Msg"] = "game_tournament";
		mailDic["Bonus"] = 2000;
		mailDic["Rank"] = 3;
		#else
		Dictionary<string, object> msgDict = new Dictionary<string, object>();
		msgDict[MailUtility._system_msg] = " {0} lucky credits for you!";
		msgDict[MailUtility._system_type] = (int)SystemMailType.CompensateMail;
		msgDict[MailUtility._system_credits] = 1001;
		msgDict[MailUtility._system_level] = 3;
		msgDict[MailUtility._system_vip_level] = 4;
		msgDict[MailUtility._system_exp] = 5;
		msgDict[MailUtility._system_vip_exp] = 6;
		msgDict[MailUtility._system_long_lucky] = 7;
		msgDict[MailUtility._system_piggybank_credits] = 8;
		msgDict[MailUtility._system_totalspin_count] = 9;

		mailDic["Title"] = "test_title";
		mailDic["Type"] = (int)MailType.System;
		mailDic["Msg"] = Json.Serialize(msgDict);
		mailDic["Bonus"] = 0;
		mailDic["Rank"] = 0;
		#endif

		sendDic.Add("Mail", mailDic);
		Debug.Log(Json.Serialize(sendDic));
		yield return null;
		bool error = false;
		JSONObject resultJS = null;
		JSONObject errorJS = null;
		yield return StartCoroutine(MailNetWorkHelper.Instance.NetCall
									(this, MailServerFunction.AddMail.ToString(), sendDic, (r) => { resultJS = r; }, (obj) =>
			{ error = true; errorJS = obj; }));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}

			GettingMail = false;
			// 这里发送错误信息  
			yield break;
		}
	}

}

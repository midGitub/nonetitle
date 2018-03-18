using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using CitrusFramework;
#if Trojan_FB
using Facebook.Unity;
#endif

public class FriendScrollviewController : DynamicScrollviewController<FriendData, FriendItem> {
	// 选中的列表
	private List<FriendData> _selectDataList = new List<FriendData> ();
	// 已收集礼物的朋友列表
	private List<FriendData> _collectRemoveDataList = new List<FriendData> ();
	// 已发送礼物的朋友列表
	private List<FriendData> _HasSendDataList = new List<FriendData> ();
	// 已邀请好友列表
	private List<FriendData> _inviteRemoveDataList = new List<FriendData>();
	// 金币特效父节点
	public GameObject _coinEffectParent;
	// 金币特效携程
	private Coroutine _coinCoroutine = null;
	// 勾选全部
	public Toggle _selectAllToggle;
	// 对所有勾选项执行操作
	public GameObject _handleToggleButton;
	// 对所有勾选项执行操作
	private Button _button;
	// 剩余次数
	public Text _countLeftText;
	// 无好友时提示
	public GameObject _noFriendHint;
	// 邀请按钮
	public GameObject _inviteButton;
	// 面板类型
	public FriendToggleType _toggleType;

	// 好友界面
	public FriendUIBehaviour _friendUI;

	// 金币特效
	private GameObject _coinEffect = null;

	// Use this for initialization
	void Start () {
		if (_handleToggleButton != null) {
			EventTriggerListener.Get (_handleToggleButton).onClick += OnClick;
		}
		if (_selectAllToggle != null) {
			EventTriggerListener.Get (_selectAllToggle.gameObject).onClick += OnClick;
		}
		if (_inviteButton != null) {
			EventTriggerListener.Get (_inviteButton).onClick += OnClick;
		}

		_button = _handleToggleButton.GetComponent<Button> ();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();
		UpdateCountLeftNum ();
		UpdateButton ();
		UpdateSelectAllToggle ();
	}

	private void UpdateCountLeftNum(){
		if (_countLeftText != null) {
			int sendOrCollectLeftCount = GetSendOrCollectLeftNum ();
			_countLeftText.text = "Times left : " + sendOrCollectLeftCount.ToString () + "/" + GetSendOrCollectMaxNum();
		}
	}

	private void UpdateSelectAllToggle(){
		if (_selectAllToggle != null) {
			if (_toggleType == FriendToggleType.Invite) {
				_selectAllToggle.isOn = _selectDataList.Count == _dataList.Count && _dataList.Count != 0;
				_selectAllToggle.interactable = _dataList.Count != 0;
			} else if (_toggleType == FriendToggleType.Collect) {
				_selectAllToggle.isOn = _selectDataList.Count == _dataList.Count && _dataList.Count != 0;
				_selectAllToggle.interactable = GetSendOrCollectLeftNum() > 0 && _dataList.Count != 0;
			} else if (_toggleType == FriendToggleType.Send) {
				_selectAllToggle.isOn = (_selectDataList.Count + _HasSendDataList.Count) == _dataList.Count
					&& _dataList.Count != 0;
				_selectAllToggle.interactable = GetSendOrCollectLeftNum() > 0
					&& (_HasSendDataList.Count < _dataList.Count) && _dataList.Count != 0;
			}
		}
	}

	private void UpdateNoFriendHint(){
		if (_noFriendHint != null) {
			bool enable = false;
			if (_toggleType == FriendToggleType.Collect) {
				enable = (FriendManager.Instance.CollectList.Count == 0);
			} else if (_toggleType == FriendToggleType.Send) {
				enable = (FriendManager.Instance.SendList.Count == 0);
			}
			_noFriendHint.SetActive (FriendManager.Instance.HasAllFriends && enable);
		}
	}


	private void UpdateButton(){
		if (_button != null) {
			bool enable = _selectDataList.Count != 0 && _dataList.Count != 0;
			if (_toggleType == FriendToggleType.Invite) {
				_button.interactable = enable;
			} else if (_toggleType == FriendToggleType.Send || _toggleType == FriendToggleType.Collect) {
				_button.interactable = GetSendOrCollectLeftNum () > 0 && enable;
			}
		}
	}

	protected override void Init ()
	{
		if (_toggleType == FriendToggleType.Invite) {
			_selectDataList.Clear ();
			_selectDataList.AddRange (_dataList);
			_selectAllToggle.isOn = true;
		} else if (_toggleType == FriendToggleType.Send) {
			ListUtility.ForEach (_dataList, (FriendData data) => {
				bool canSend = CanFriendSendGiftByTime(data);
				if (!canSend){
					_HasSendDataList.Add(data);
				}
			});
		}else if (_toggleType == FriendToggleType.Collect){
		}
	}

	protected override void DefaultProcess ()
	{
		UpdateCountLeftNum ();
		UpdateButton ();
		UpdateSelectAllToggle ();
		UpdateNoFriendHint ();
	}

	private int GetSendOrCollectMaxNum(){
		if (_toggleType == FriendToggleType.Send)
			return FriendSettingConfig.Instance.SendNumMax;
		else if (_toggleType == FriendToggleType.Collect)
			return FriendSettingConfig.Instance.CollectNumMax;

		return FriendDefine.GiftNumTHreshold;
	}

	private int GetSendOrCollectLeftNum(){
		int num = 0;
		if (_toggleType == FriendToggleType.Send) {
			num = UserDeviceLocalData.Instance.FriendSendGiftNum;
		} else if (_toggleType == FriendToggleType.Collect) {
			num = UserDeviceLocalData.Instance.FriendCollectGiftNum;
		}

		return GetSendOrCollectMaxNum() - num;
	}

	protected override void ItemHandlerInit (FriendItem item)
	{
		item.ToggleClickHandler = UpdateItemToggle;
		item.ButtonClickHandler = ClickItemButton;
	}
		
	protected override void ItemUpdateFunc (FriendItem item, FriendData data)
	{
		bool enable = CanItemEnable(data);

		int index = _selectDataList.FindIndex ((FriendData d) => {
			return d == data;
		});

		int hasSendIndex = _HasSendDataList.FindIndex ((FriendData d) => {
			return d == data;
		});

		item.UpdateData(data, index, enable, hasSendIndex);
	}

	private bool CanItemEnable(FriendData data){
		bool result = false;
		if (_toggleType == FriendToggleType.Send) {
			result = CanFriendSendGift (data);
		} else if (_toggleType == FriendToggleType.Collect) {
			result = CanFriendCollectGift (data);
		} else if (_toggleType == FriendToggleType.Invite) {
			result = CanFriendInvite (data);
		}
		return result;
	}

	private bool CanFriendSendGift(FriendData data){
		return CanFriendSendGiftByTime (data) && GetSendOrCollectLeftNum () > 0;
	}

	private bool CanFriendSendGiftByTime(FriendData data){
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		return !TimeUtility.IsSameDay (now, data.LastSendTime);
	}

	private bool CanFriendCollectGift(FriendData data){
		return GetSendOrCollectLeftNum () > 0;
	}

	private bool CanFriendInvite(FriendData data){
		return true;
	}

	void OnClick(GameObject obj){
		if (obj == _handleToggleButton && _button.interactable) {
			AudioManager.Instance.PlaySound (AudioType.Click);
			if (_toggleType == FriendToggleType.Send) {
				FriendGift gift = new FriendGift ();
				HandleSendDataList (_selectDataList, gift, SendGiftCallback);
			} else if (_toggleType == FriendToggleType.Collect) {
				HandleCollectDataList (_selectDataList, CollectGiftCallback);
			} else if (_toggleType == FriendToggleType.Invite) {
				HandleInviteDataList (_selectDataList, InviteCallback);
			}
		} else if (obj == _inviteButton) {
			_friendUI.SetFriendToggle (FriendToggleType.Invite);
			AudioManager.Instance.PlaySound (AudioType.Click);
		} else if (obj == _selectAllToggle.gameObject && _selectAllToggle.interactable) {
			UpdateSelectAllToggle (_selectAllToggle.isOn);
			AudioManager.Instance.PlaySound (AudioType.Click);
		}
	}

	private void UpdateSelectAllToggle(bool value){
		_selectDataList.Clear ();

		if (value) {
			if (_toggleType == FriendToggleType.Send) {
				List<FriendData> list = ListUtility.SubtractList (_dataList, _HasSendDataList);
				_selectDataList.AddRange (list);
			} else if (_toggleType == FriendToggleType.Collect || _toggleType == FriendToggleType.Invite) {
				_selectDataList.AddRange (_dataList);
			} 
		}

		ItemUpdateList (_lastIndex);
	}


	private List<string> FilterFriendDataList(List<FriendData> list, int leftNum, FriendToggleType type){
		List<string> ids = new List<string> ();
		ListUtility.ForEach (list, (FriendData data) => {
			if (type == FriendToggleType.Send){
				ids.Add(data.ID);
			}
			else{
				string request_id = FriendManager.Instance.FindCollectRequestUserId(data);
				if (!string.IsNullOrEmpty(request_id)){
					ids.Add(request_id);
				}
			}
		});

		if (ids.Count > leftNum) {
			int deleteCount = ids.Count - leftNum;
			ids.RemoveRange(0, deleteCount);
		}

		return ids;
	}

	private List<string> FilterSendFriendIDs(List<FriendData> list, int leftNum){
		List<FriendData> sendList = ListUtility.FilterList (list, (FriendData data) => {
			return ListUtility.IsAnyElementSatisfied(_HasSendDataList, (FriendData sendData)=>{
				return data == sendData;
			});
		});

		List<FriendData> noSendList = ListUtility.SubtractList (list, sendList);
		List<string> ids = FilterFriendDataList (noSendList, leftNum, FriendToggleType.Send);

		return ids;
	}

	private void HandleSendDataList(List<FriendData> list, FacebookDelegate<IAppRequestResult> cb){
		int sendLeftNum = GetSendOrCollectLeftNum ();
		if (sendLeftNum <= 0)
			return;
		
		List<string> ids = FilterSendFriendIDs(list, sendLeftNum);
		FriendManager.Instance.SendGift (ids, cb);
	}

	private void HandleSendDataList(List<FriendData> list, FriendGift gift, FacebookDelegate<IAppRequestResult> cb){
		int sendLeftNum = GetSendOrCollectLeftNum ();
		if (sendLeftNum <= 0)
			return;

		List<string> ids = FilterSendFriendIDs(list, sendLeftNum);
		FriendManager.Instance.SendGift (ids, gift, cb);
	}

	private void HandleCollectDataList(List<FriendData> list, FacebookDelegate<IGraphResult> cb){
		int collectLeftNum = GetSendOrCollectLeftNum ();
		if (collectLeftNum <= 0) {
			return;
		}

		List<string> ids = FilterFriendDataList (list, collectLeftNum, FriendToggleType.Collect);
		UpdateCollectRemoveDataList (ids);
		FriendManager.Instance.GetGift (ids, cb);
	}

	private void UpdateCollectRemoveDataList(List<string> ids){
		if (ids.Count == 0)
			return;
		
		_collectRemoveDataList.Clear ();
		Dictionary<string, FriendData> collectDict = FriendManager.Instance.CollectFriends;
		ListUtility.ForEach (ids, (string s) => {
			if (collectDict.ContainsKey(s)){
				_collectRemoveDataList.Add(collectDict[s]);
			}
		});
	}

	private void HandleInviteDataList(List<FriendData> list, FacebookDelegate<IAppRequestResult> cb){
		List<string> ids = new List<string> ();
		ListUtility.ForEach (list, (FriendData data) => {
			ids.Add(data.ID);
		});

		UpdateInviteRemoveDataList (list);
		FriendManager.Instance.InviteFriend (ids, cb);
	}

	private void UpdateInviteRemoveDataList(List<FriendData> list){
		_inviteRemoveDataList.Clear ();
		_inviteRemoveDataList.AddRange (list);
	}

	private void SendGiftCallback(IAppRequestResult result){
		LogUtility.Log ("SendGiftCallback ", Color.red);

		if (FacebookUtility.ResultValid (result)) {
			LogUtility.Log ("RawResult = " + result.RawResult, Color.red);

			if (result.To == null || result.To.GetEnumerator() == null) {
				GameDebug.Assert (false, "SendGiftCallback GetEnumerator failed");
				return;
			}

			IEnumerator<string> enumerator = result.To.GetEnumerator ();
			List<string> ids = new List<string> ();
			while (enumerator.MoveNext ()) {
				ids.Add(enumerator.Current);
			}

			List<FriendData> dataList = ListUtility.FilterList(_dataList, (FriendData data) => {
				return ListUtility.CountElements(ids, (string id)=>{
					return data.ID.Equals(id);
				}) > 0;
			});

			// 更新发送礼物时间
			ListUtility.ForEach(dataList, (FriendData data)=>{
				data.LastSendTime = NetworkTimeHelper.Instance.GetNowTime ();
				if (_selectDataList.Contains(data)) _selectDataList.Remove(data);
			});

			_HasSendDataList.AddRange (dataList);

			FriendManager.Instance.SaveFriendList ();
			UserDeviceLocalData.Instance.FriendSendGiftNum += dataList.Count;

			ItemUpdateList (_lastIndex);

			// 打点，计算赠送的点数
			ulong creditsSum = CalculateTotalCredits(dataList);
			UserDeviceLocalData.Instance.FriendSendCreditsDailySum += (int)creditsSum;
			AnalysisManager.Instance.SendGift ((int)creditsSum, UserDeviceLocalData.Instance.FriendSendCreditsDailySum);
		} else {
			// 发送礼物失败
			LogUtility.Log("send gift failed", Color.red);
		}
	}

	private ulong CalculateTotalCredits(List<FriendData> list){
		ulong creditsSum = 0;
		ListUtility.ForEach (list, (FriendData data) => {
			if (data.Gift != null && data.Gift.GetGift(GiftType.Credit) != null){
				creditsSum += (ulong)data.Gift.GetGift(GiftType.Credit).Num;
			}
		});
		return creditsSum;
	}

	private void CollectGiftCallback(IGraphResult result){
		LogUtility.Log ("CollectGiftCallback", Color.red);

		if (FacebookUtility.ResultValid (result)) {
			LogUtility.Log ("RawResult = " + result.RawResult, Color.red);

			if (_collectRemoveDataList.Count != 0){
				ulong creditsSum = CalculateTotalCredits(_collectRemoveDataList);
				UserBasicData.Instance.AddCredits (creditsSum, FreeCreditsSource.FBReceiveGiftBonus, false);
				UserBasicData.Instance.AddLongLucky ((int)(creditsSum * FriendSettingConfig.Instance.CreditsToLuckyFactor), true);
				DeleteItemList (_collectRemoveDataList);
				ListUtility.ForEach (_collectRemoveDataList, (FriendData data) => {
					FriendManager.Instance.CollectList.Remove(data);
					if (_dataList.Contains(data)) _dataList.Remove(data);
					if (_selectDataList.Contains(data)) _selectDataList.Remove(data);
				});

				UserDeviceLocalData.Instance.FriendCollectGiftNum += _collectRemoveDataList.Count;
				UserDeviceLocalData.Instance.FriendCollectCreditsDailySum += (int)creditsSum;
				AnalysisManager.Instance.CollectGift ((int)creditsSum, UserDeviceLocalData.Instance.FriendCollectCreditsDailySum);

				PlayCoinEffect ();
			}
		} else {
			// 收取礼物失败
			LogUtility.Log("collect gift failed");
		}
		_collectRemoveDataList.Clear ();
	}

	private void InviteCallback(IAppRequestResult result){
		LogUtility.Log ("InviteCallback", Color.red);

		if (FacebookUtility.ResultValid (result)) {
			// id数组
			LogUtility.Log ("RawResult = " + result.RawResult, Color.red);

			if (_inviteRemoveDataList.Count != 0) {
				// 删除当前面板中的好友
				DeleteItemList (_inviteRemoveDataList);
				ListUtility.ForEach (_inviteRemoveDataList, (FriendData data) => {
					FriendManager.Instance.InviteList.Remove(data);
					if (_dataList.Contains(data)) _dataList.Remove(data);
					if (_selectDataList.Contains(data)) _selectDataList.Remove(data);
				});

				// 这里似乎无法删除好友，因为发送和回调的id是不同的。
				AnalysisManager.Instance.InviteFriend();
			}
		} else {
			// 邀请好友失败
			LogUtility.Log("invite friend failed", Color.red);
		}

		_inviteRemoveDataList.Clear ();
	}

	private void PlayCoinEffect(){
		if (_coinCoroutine != null) {
			StopCoroutine (_coinCoroutine);
			_coinCoroutine = null;
		}

		if (_coinEffect == null) {
			_coinEffect = UIManager.Instance.OpenFriendCoinEffect (_coinEffectParent);
		} else {
			_coinEffect.SetActive (true);
		}

		FriendCoinController controller = _coinEffect.GetComponent<FriendCoinController> ();

		if (controller != null) {
			controller.Stop ();
			controller.Play ();
		}

		_coinCoroutine = UnityTimer.Start (this, 3.0f, () => {
			_coinEffect.SetActive(false);
			_coinCoroutine = null;
		});

		AudioManager.Instance.PlaySound (AudioType.CollectFBGift);
	}


	#region FriendItem callback

	private void UpdateItemToggle(bool value, FriendData data){
		// 这里需要判断是玩家点击勾选还是因为发送礼物后的强制勾选

		// 强制勾选逻辑不播放音效，不会把按钮推送到selectlist
		if (value) {
			if (!_HasSendDataList.Contains (data) && !_selectDataList.Contains (data)) {
				_selectDataList.Add (data);
				AudioManager.Instance.PlaySound (AudioType.Click);
			} 
		} else {
			if (_selectDataList.Contains (data)) {
				_selectDataList.Remove (data);
				AudioManager.Instance.PlaySound (AudioType.Click);
			}
		}
	}

	private void ClickItemButton(FriendData data){
		List<FriendData> list = new List<FriendData> ();
		list.Add (data);

		if (_toggleType == FriendToggleType.Send) {
			HandleSendDataList (list, new FriendGift(), SendGiftCallback);
		} else if (_toggleType == FriendToggleType.Collect) {
			HandleCollectDataList (list, CollectGiftCallback);
		} else if (_toggleType == FriendToggleType.Invite) {
			HandleInviteDataList (list, InviteCallback);
		}
		AudioManager.Instance.PlaySound (AudioType.Click);
	}

	void OnEnable(){
	}

	void OnDisable(){
		if (_coinEffect != null)
			_coinEffect.SetActive (false);
	}

	#endregion
}

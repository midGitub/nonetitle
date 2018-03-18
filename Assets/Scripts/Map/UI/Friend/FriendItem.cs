using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class FriendItem : MonoBehaviour {
	// 头像
	public Image _img;
	// 等级
	public Text _lv;
	// 姓名
	public Text _name;
	// 礼物数量
	public Text _gold;
	// 勾选栏
	public Toggle _choose;
	// 按钮
	public Button _button;
	// 已发送字样
	public Text _hasSentHint;

	// 玩家数据
	private FriendData _data;

	// 勾选框回调
	public Callback<bool, FriendData> ToggleClickHandler = null;
	// 按钮回调
	public Callback<FriendData> ButtonClickHandler = null;

	// 面板对象
	public FriendScrollviewController _control;

	// Use this for initialization
	void Start () {
		_choose.onValueChanged.AddListener (OnValueChanged);
		EventTriggerListener.Get(_button.gameObject).onClick += OnClick;
	}

	// index：在当前选择列表的index， enable：是否可以点击button， hasSendIndex：是否已经发送存在于发送列表
	public void UpdateData(FriendData data, int index, bool enable, int hasSendIndex){
		_data = data;

		// sprite通过data的id去读取
		Sprite spr = SpriteStoreManager.Instance.GetSprite(data.ID);
		if (spr != null) _img.sprite = spr;
		else {
			SpriteStoreManager.Instance.DownloadSprite (data.ID, data.ICON, ()=>{
				if (_img != null){
					_img.sprite = SpriteStoreManager.Instance.GetSprite(data.ID);	
				}
			});
		}
			
		if (_name != null) _name.text = data.Name;

		if (_button != null) {
//			_button.interactable = enable;
			_button.gameObject.SetActive (enable);
		}

		if (_lv != null) _lv.text = "Level: " + data.Level.ToString ();

		if (_gold != null && data.Gift != null) {
			if (data.Gift.GiftDict.ContainsKey (GiftType.Credit))
				_gold.text = data.Gift.GiftDict [GiftType.Credit].Num.ToString();
			else
				_gold.text = "0";
		}

		// 如果存在于已发送列表或选择列表，则勾选
		if (_choose != null) {
			_choose.isOn = index >= 0 || hasSendIndex >= 0;
			_choose.interactable = enable;
		}

		if (_hasSentHint != null) {
			_hasSentHint.gameObject.SetActive (!enable);
		}
	}

	void OnValueChanged(bool value){
		if (ToggleClickHandler != null) {
			LogUtility.Log("Item Toggle Valuechange = "+value+" data = "+_data.ToString(), Color.yellow);
			ToggleClickHandler (value, _data);
		}
	}

	void OnClick(GameObject obj){
		if (obj == _button.gameObject && _button.interactable) {
			if (ButtonClickHandler != null) {
				LogUtility.Log("Item Button click data = "+_data.ToString(), Color.yellow);
				ButtonClickHandler (_data);
			}
		}
	}
}

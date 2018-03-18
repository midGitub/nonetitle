using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;

public class UserLevelUIController : MonoBehaviour
{
	public ImageBar Ibar;

	public GameObject EffectGameObject;
	public Image LevelUpImage;
	public Text CoinsNum;
	public Text LevelText;


	public float ShowTime;
	public float StayTime;

	public float XPBarAddSpeed = 1;

	private bool _isShowingEffect = false;

	private float _upupNum;

	public void Update()
	{
		if(_upupNum != UserLevelSystem.Instance.CurrUserLevelData.LevelPoint)
		{
			var all = UserLevelSystem.Instance.NextUserLevelConfigData.RequiredXP;
			float speed = all * XPBarAddSpeed * Time.deltaTime;

			_upupNum += speed;
			_upupNum = _upupNum > UserLevelSystem.Instance.CurrUserLevelData.LevelPoint ? (float)UserLevelSystem.Instance.CurrUserLevelData.LevelPoint : _upupNum;
		}

		Ibar.ChangeBarState(0,UserLevelSystem.Instance.NextUserLevelConfigData.RequiredXP,
							_upupNum);
		LevelText.text = "LEVEL " + UserLevelSystem.Instance.CurrUserLevelData.Level;
	}


	private void UpdateLevelDataMessage(UserDataLoadEvent loadms)
	{
		_upupNum = UserLevelSystem.Instance.CurrUserLevelData.LevelPoint;
	}

	// Use this for initialization
	public void Start()
	{
		_upupNum = UserLevelSystem.Instance.CurrUserLevelData.LevelPoint;
		CitrusEventManager.instance.AddListener<UserLevelUpEvent>(UserLevelUpMessageProcess);
		CitrusEventManager.instance.AddListener<UserDataLoadEvent>(UpdateLevelDataMessage);
	}

	public void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<UserLevelUpEvent>(UserLevelUpMessageProcess);
		CitrusEventManager.instance.RemoveListener<UserDataLoadEvent>(UpdateLevelDataMessage);
	}

	private void UserLevelUpMessageProcess(UserLevelUpEvent upms)
	{
		CoinsNum.text = "+" + upms.CurrLevelConfigData.LevelUpBonusCredits;

		if(_isShowingEffect)
		{
			return;
		}
		AudioManager.Instance.PlaySound(AudioType.LevelUp);
		_isShowingEffect = true;
		EffectGameObject.SetActive(true);
		StartCoroutine(ScriptEffect.FadeInAndOut(this, (co) =>
		{
			Color ca = (LevelUpImage.color); ca.a = co; LevelUpImage.color = ca; Color ta = CoinsNum.color; ta.a = co;
			CoinsNum.color = ta;
		}, ShowTime, StayTime, 0f, 1,
												 () => { EffectGameObject.SetActive(false); _isShowingEffect = false; }));
	}
}

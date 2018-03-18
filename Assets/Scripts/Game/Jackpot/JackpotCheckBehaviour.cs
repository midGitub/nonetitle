using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JackpotCheckBehaviour : MonoBehaviour {
	// 勾
	public Image _canJackpot;
	// cha
	public Image _cantJackpot;
	// 背景
	public Image _backgournd;
	// 四jackpot的图片
	public Image[] _poolTitleArray;
	// jackpot字样
	public Image _title;
	// 显示分数
	public Text[] _scoreTextArray;
	// 默认字体颜色
	private Color[] _scoreTextColors;
	// 转圈特效
	public GameObject _effect;
	// hide时候特效是否正在显示
	private bool _effectPreviousActiveState;

	private static readonly Color _defaultColor = new Color(1.0f, 0.0f, 0.0f);
	private static readonly Color _darkColor = new Color(0.5f, 0.5f, 0.5f);

	void Awake () {
		_scoreTextColors = new Color[_scoreTextArray.Length];
		for (int i = 0; i < _scoreTextColors.Length; ++i) {
			_scoreTextColors [i] = _scoreTextArray [i].color;
		}
		SetScoreColor (_darkColor);
	}

	public void EnableJackpot(bool isJackpot){
		if (_canJackpot != null) {
			_canJackpot.enabled = isJackpot;
		}

		if (_cantJackpot != null) {
			_cantJackpot.enabled = !isJackpot;
		}

		ShadowImage (!isJackpot);
		if (_effect != null){
			_effect.SetActive (isJackpot);
		}
	}

	private void ShadowImage(bool shadow){
		Color color = Color.white;
		SetScoreColorDefault ();

		if (shadow) {
			color = Color.gray;
			SetScoreColor (_darkColor);
		}

		if (_canJackpot != null) {
			_canJackpot.color = color;
		}

		if (_cantJackpot != null) {
			_cantJackpot.color = color;
		}

		if (_backgournd != null) {
			_backgournd.color = color;
		}

		if (_title != null) {
			_title.color = color;
		}

		for (int i = 0; i < _poolTitleArray.Length; ++i) {
			if (_poolTitleArray[i] != null){
				_poolTitleArray [i].color = color;
			}
		}
	}

	private void SetScoreColor(Color c){
		ListUtility.ForEach (_scoreTextArray, (Text t) => {
			t.color = c;
		});
	}

	private void SetScoreColorDefault(){
		for (int i = 0; i < _scoreTextColors.Length; ++i) {
			_scoreTextArray [i].color = _scoreTextColors [i];
		}
	}

	public void Hide(){
		gameObject.transform.localPosition = new Vector3(9999.0f, 9999.0f, 0.0f);
		if (_effect != null) {
			_effectPreviousActiveState = _effect.activeInHierarchy;
			if (_effectPreviousActiveState){
				_effect.SetActive(false);
			}
		}
	}
	public void Show(){
		gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		if (_effect != null){
			if (_effectPreviousActiveState){
				_effect.SetActive(true);
			}
		}
	}
}

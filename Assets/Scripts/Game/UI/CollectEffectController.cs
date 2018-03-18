using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectEffectType{
	Once, 
	Complete,
	Hint,
	Always,// 一直存在的特效
	Max,
}

public class CollectEffectController : MonoBehaviour {
	// 收集一次时的特效
	private GameObject _collectEffect;
	// 收集完成时特效
	private GameObject _collectCompleteEffect;
	// 收集提示特效
	private GameObject _collectHintEffect;
	// 收集特效（一直存在）
	private GameObject _collectAlwaysEffect;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetCollectEffect(GameObject effect){
		_collectEffect = effect;
	}

	public void SetCompleteEffect(GameObject effect){
		_collectCompleteEffect = effect;
	}

	public void SetCollectHintEffect(GameObject effect){
		_collectHintEffect = effect;
	}

	public void SetAlwaysEffect(GameObject effect){
		_collectAlwaysEffect = effect;
	}

	public void ShowCollectEffect(bool show){
		if (_collectEffect != null) {
			_collectEffect.SetActive (show);
		}
	}

	public void ShowCompleteEffect(bool show){
		if (_collectCompleteEffect != null) {
			_collectCompleteEffect.SetActive (show);
		}
	}

	public void ShowCollectHintEffect(bool show){
		if (_collectHintEffect != null) {
			_collectHintEffect.SetActive (show);
		}
	}

	public void UpdateCollectHintEffect(float value, float height, float offsetY = 0.0f){
		if (_collectHintEffect != null) {
			_collectHintEffect.SetActive (value != 0.0f);
			_collectHintEffect.transform.localPosition = new Vector3 (0, offsetY + value * height, 0);
		}
	}

	public void CreateCollectEffect(string path, GameObject parent, CollectEffectType type, string name){
		GameObject effectObj = UGUIUtility.CreateMachineAsset(path, name, parent);
		if (type == CollectEffectType.Once) {
			SetCollectEffect (effectObj);
			ShowCollectEffect (false);
		} else if (type == CollectEffectType.Complete) {
			SetCompleteEffect (effectObj);
			ShowCompleteEffect (false);
		} else if (type == CollectEffectType.Hint) {
			SetCollectHintEffect (effectObj);
			ShowCollectHintEffect (false);
		}
	}

	public void ShowEffect(CollectEffectType type, bool isShow){
		if (type == CollectEffectType.Once) {
			ShowCollectEffect (isShow);
		} else if (type == CollectEffectType.Complete) {
			ShowCompleteEffect (isShow);
		} else if (type == CollectEffectType.Hint) {
			ShowCollectHintEffect (isShow);
		}
	}
}

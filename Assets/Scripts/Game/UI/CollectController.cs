using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CitrusFramework;

public class CollectController : MonoBehaviour {
	public delegate void EventHandler (GameObject obj);

	private EventHandler _flyEndCallback;
	private GameObject _trailEffect;
	private GameObject _animatorObject;

	public EventHandler FlyEndCallback{
		get { return _flyEndCallback; }
		set { _flyEndCallback = value; }
	}

	public GameObject TrailEffect{
		get { return _trailEffect; }
		set { _trailEffect = value; }
	}

	public GameObject AnimatorObject{
		get { return _animatorObject; }
		set { _animatorObject = value; }
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartFly(GameObject obj, float duration, float animatorDuration){
		if (_animatorObject == null){
			Fly(obj, duration);
		}else{
			_animatorObject.SetActive(true);
			UnityTimer.Start(this, animatorDuration, ()=>{
				_animatorObject.SetActive(false);
				Fly(obj, duration);
			});
		}
	}

	private void Fly(GameObject obj, float duration){
		//  从自己当前的位置，飞往obj位置
		Tweener tweener = transform.DOMove(obj.transform.position, duration);
//		tweener.SetUpdate (true);
		tweener.SetEase (Ease.Linear);
//		tweener.SetEase (Ease.InExpo);
		tweener.OnComplete (()=>{
			if (_flyEndCallback != null) {
				// 飞完后销毁自己，增长进度条1格
				_flyEndCallback (gameObject);
			}
		});
		if (_trailEffect != null) {
			_trailEffect.SetActive (true);
		}
	}
}

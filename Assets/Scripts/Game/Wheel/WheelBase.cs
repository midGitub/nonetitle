using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WheelBase : MonoBehaviour {
	// 点击旋转按钮
	public List<Button> _clickButtons;
	// 转盘实体
	public WheelController[] _rotaryControllers;

	protected PuzzleMachine _puzzleMachine;

	// Use this for initialization
	public virtual void Start () {
		ListUtility.ForEach(_clickButtons, (Button b) => {
			EventTriggerListener.Get(b.gameObject).onClick = ButtonPress;
		});
	}
	
	// Update is called once per frame
	public virtual void Update () {
		
	}

	public virtual void ButtonPress(GameObject obj){
		obj.GetComponent<Button>().interactable = false;
	}

	public virtual void OnEnable(){
		ListUtility.ForEach(_clickButtons, (Button b) => {
			b.interactable = true;
		});
	}

	public virtual void StartAnimation(){}

	public virtual void EndAnimation(){}

	public virtual bool IsRotating(){
		return ListUtility.IsAnyElementSatisfied (_rotaryControllers, (WheelController controller) => {
			return controller.IsRotating;
		});
	}

	public virtual void Init(PuzzleMachine puzzle)
	{
		_puzzleMachine = puzzle;
	}

	public virtual void OnDestroy(){}
}

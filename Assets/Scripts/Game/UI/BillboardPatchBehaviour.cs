using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillboardPatchBehaviour : MonoBehaviour {
	private ScrollRect _rect;
	private ScrollRect.MovementType _originalMovementType;
	// Use this for initialization
	void Awake () {
		_rect = gameObject.GetComponent<ScrollRect>();
		_originalMovementType = _rect.movementType;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Enable2Image(){
		_rect.movementType = ScrollRect.MovementType.Clamped;
	}

	public void Disable2Image(){
		_rect.movementType = _originalMovementType;
	}
}

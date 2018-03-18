using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutGroupAdapterBehaviour : MonoBehaviour {
	public Vector3 _scaleIpad = new Vector3(1.1f, 1.1f, 1.0f);
	public float _spacing = -26;
	public GridLayoutGroup _layoutGroup;
	public HorizontalLayoutGroup _horizontalLayoutGroup;

	// Use this for initialization
	void Start () {
		if (_layoutGroup != null){
			if (DeviceUtility.IsIPadResolution()){
				Vector2 cellSize = _layoutGroup.cellSize;
				_layoutGroup.cellSize = new Vector2(cellSize.x * _scaleIpad.x, cellSize.y * _scaleIpad.y);
			}
		}

		if (_horizontalLayoutGroup != null){
			if (DeviceUtility.IsIPadResolution()){
				_horizontalLayoutGroup.spacing = _spacing;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

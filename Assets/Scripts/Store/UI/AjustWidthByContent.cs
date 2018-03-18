using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AjustWidthByContent : MonoBehaviour {

    [SerializeField]
    private RectTransform _target;

    [SerializeField]
    private int _defaultValLength;

    private Vector2 _originalSizeDelta;

    private bool _isThefirstTime = true;

    public void AjustTargetWidth(int nowValLength)
    {
        if (_isThefirstTime)
        {
            _isThefirstTime = false;
            _originalSizeDelta = _target.sizeDelta;
        }

//        LogUtility.Log("Before ajust " + _originalSizeDelta.x , Color.magenta);
//        LogUtility.Log("**** " + nowValLength + " " + _target.anchoredPosition + " " + _target.sizeDelta + " " + _target.rect, Color.magenta);
        float width = _originalSizeDelta.x / _defaultValLength * nowValLength;
        _target.sizeDelta = new Vector2(width, _originalSizeDelta.y);
//        LogUtility.Log("After ajust " + _target.rect.width, Color.magenta);
    }
}

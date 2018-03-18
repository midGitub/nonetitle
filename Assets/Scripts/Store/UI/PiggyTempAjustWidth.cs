using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiggyTempAjustWidth : MonoBehaviour {

    [SerializeField]
    private RectTransform _target;

    private bool _isThefirstTime = true;

    private float _defaultWidth;

    public void AjustTargetWidth(Text nowText)
    {
		if (nowText.preferredWidth + nowText.preferredWidth/nowText.text.Length > nowText.rectTransform.sizeDelta.x) 
		{
			float width = (nowText.preferredWidth - nowText.rectTransform.sizeDelta.x + nowText.preferredWidth/nowText.text.Length) * 2;
			_target.sizeDelta = new Vector2(width+_target.sizeDelta.x, _target.sizeDelta.y);
			nowText.rectTransform.sizeDelta = new Vector2(nowText.preferredWidth + nowText.preferredWidth/nowText.text.Length,nowText.rectTransform.sizeDelta.y);
		}

		/*
		string nowText=NowText.text;
        int length = nowText.Length;
        if (nowText.Contains("￥"))
            length = length + 2;

        float width;
        switch (length)
        {
            case 5:
                width = 527.3f;
                break;
            case 6:
                width = 580f;
                break;
            case 7:
                width = 650f;
                break;
            case 8:
                width = 720f;
                break;
            case 9:
                width = 800f;
                break;
            case 10:
                width = 880f;
                break;
            default:
                width = 527.3f;
                break;
        }
        */
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CloseGameObject : MonoBehaviour 
{
	public WidgetJumpController _jumpController;

    public void Close(GameObject go, Action callBack)
	{
		if (_jumpController != null) {
			AudioManager.Instance.PlaySound(AudioType.Click);
			_jumpController.Open (false, () => {
				go.SetActive(false);
                callBack();
			});
		} else {
			go.SetActive(false);
			AudioManager.Instance.PlaySound(AudioType.Click);
            callBack();
		}
	}

    public void Close(GameObject go)
    {
        if (_jumpController != null) {
            AudioManager.Instance.PlaySound(AudioType.Click);
            _jumpController.Open (false, () => {
                go.SetActive(false);
            });
        } else {
            go.SetActive(false);
            AudioManager.Instance.PlaySound(AudioType.Click);
        }
    }
}

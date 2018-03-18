using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ScriptButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	public ButtonObject ButtonShowGameObject;

	public UnityEvent onclick = new UnityEvent();

	public UnityEvent OnPoinDown = new UnityEvent();
	public UnityEvent OnPoinUp = new UnityEvent();



	public void OnPointerClick(PointerEventData eventData)
	{
		onclick.Invoke();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		OnPoinDown.Invoke();
		ButtonShowGameObject.UpdateGameObject(true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		OnPoinUp.Invoke();
		ButtonShowGameObject.UpdateGameObject(false);
	}

	[Serializable]
	public class ButtonObject
	{
		public GameObject UpObject;
		public GameObject DownObject;

		public void UpdateGameObject(bool isDown)
		{
			if(UpObject != null)
			{
				UpObject.SetActive(!isDown);
			}
			if(DownObject != null)
			{
				DownObject.SetActive(isDown);
			}
		}
	}
}

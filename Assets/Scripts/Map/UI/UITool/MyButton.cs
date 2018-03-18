using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton :MonoBehaviour, IPointerClickHandler,IPointerUpHandler,IPointerDownHandler
{
	public UnityEvent Click = new UnityEvent();
	public UnityEvent Up = new UnityEvent();
	public UnityEvent Down = new UnityEvent();
	public ScrollRect sr;

	private bool ismoveing = false;

	public void Start()
	{
		sr.onValueChanged.AddListener(IsMoveing);
	}
	private void IsMoveing(Vector2 V2)
	{
		ismoveing = true;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if(ismoveing)
		{
			return;
		}
		Click.Invoke();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if(ismoveing)
		{
			return;
		}
		Down.Invoke();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if(ismoveing)
		{
			return;
		}
		Up.Invoke();
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class BillBoardScroll : MonoBehaviour,IBeginDragHandler,IEndDragHandler {

	public UnityEvent OnEndDragEvent;
	public UnityEvent OnBeginDragEvent;

	public void OnEndDrag(PointerEventData eventData)
	{
		if (OnEndDragEvent != null)
			OnEndDragEvent.Invoke();
	}



	public void OnBeginDrag(PointerEventData eventData)
	{
		if (OnBeginDragEvent != null)
			OnBeginDragEvent.Invoke();
	}
}

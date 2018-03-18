using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoreButtonEffect : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
	public GameObject ScalerObject;
	public float UpScaler = 1;
	public float DownScaler = 0.99f;
	public GameObject ImageObjet;

	public void OnPointerDown(PointerEventData eventData)
	{
		ScalerObject.transform.localScale = Vector3.one * DownScaler;
		if(ImageObjet != null)
		{
			ImageObjet.SetActive(true);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		ScalerObject.transform.localScale = Vector3.one * UpScaler;
		if(ImageObjet != null)
		{
			ImageObjet.SetActive(false);
		}
	}
}

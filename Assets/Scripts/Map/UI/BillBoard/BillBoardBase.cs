using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BillBoardBase :MonoBehaviour{
	public GameObject ThisGameObject;

	public virtual void SetParent()
	{
		ThisGameObject.transform.SetParent(GameObject.Find("BillParent").transform, false);
	}

	public virtual void Show()
	{
		ThisGameObject.SetActive(true);
	}

	public virtual void Hide()
	{
		ThisGameObject.SetActive(false);
	}

	public virtual void Remove()
	{
		ThisGameObject.SetActive (false);
		Destroy(ThisGameObject);
		ThisGameObject = null;
	}

	public virtual void Add()
	{
		BillBoardManager.Instance.Add(this);
	}
}

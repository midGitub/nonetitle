using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class FaceBookButton : MonoBehaviour,IPointerClickHandler
{

	public void OnPointerClick(PointerEventData eventData)
	{
		// 没联网
		if(Application.internetReachability == NetworkReachability.NotReachable)
		{
			NoCollectionErrorObject.Instance.Show();
			return;
		}

		Debug.Log( FacebookHelper.IsLoggedIn);
       #if Trojan_FB
		if (!FacebookHelper.IsLoggedIn)
		{
			FacebookHelper.LoginWithFB();
		}
		#endif
	}

}

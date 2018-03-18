using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ADSAndChestSelector : MonoBehaviour {

	[SerializeField]
	private GameObject ChestButton;

	[SerializeField]
	private GameObject ADSButton;



	// Use this for initialization
	void Start () {
		InitState();
		SetState ();
	}
	
	// Update is called once per frame

	void Update () {
	}

	void InitState()
	{
		ADSButton.SetActive (false);
		ChestButton.SetActive (false);
	}

	private void OnDestroy()
	{
		StoreManager.Instance.OnPurchaseCompletedEvent.RemoveListener(BuyProductCallBack);
	}

	private void SetState()
	{
		if(UserBasicData.Instance.IsPayUser)
		{
			StartCoroutine (TryActiveChestButton ());
		}
		else
		{
			ADSButton.SetActive (true);
			StoreManager.Instance.OnPurchaseCompletedEvent.AddListener(BuyProductCallBack);
		}
	}

	private IEnumerator TryActiveChestButton()
	{
		float delay = 1.0f;
		while (Application.internetReachability == NetworkReachability.NotReachable) 
		{
			yield return new WaitForSeconds(delay);
		}
		ChestButton.SetActive (true);
	}

	private void BuyProductCallBack(IAPData data)
	{
		ADSButton.SetActive(false);
		StartCoroutine(TryActiveChestButton());
		StoreManager.Instance.OnPurchaseCompletedEvent.RemoveListener(BuyProductCallBack);
	}
}

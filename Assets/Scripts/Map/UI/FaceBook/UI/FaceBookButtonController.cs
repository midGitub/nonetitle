using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceBookButtonController : MonoBehaviour
{
	private static FaceBookButtonController _instance;
	public GameObject FaceBookButton;

	public static FaceBookButtonController Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<FaceBookButtonController>();
			}

			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Update()
	{
		UpdateButtonType();
	}

	public void UpdateButtonType()
	{
		if(FaceBookButton != null && FacebookHelper.IsLoggedIn)
		{
			FaceBookButton.SetActive(false);
		}
		else
		{
			FaceBookButton.SetActive(true);
		}
	}

	public void ShowCollectionEffect()
	{
		//FindObjectOfType<BonusButton>().ShowCollectioningCoinsEffect();
	}


}

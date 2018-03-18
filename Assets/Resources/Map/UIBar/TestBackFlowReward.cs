using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBackFlowReward : MonoBehaviour {

	public Button _subLoginDay;
	public Button _subDayBonu;
	public Button _addPaidAmound;
	public Button _subPaidAmound;

	public Button _testButton;

	public Text _lastLoginDay;
	public Text _lastDayBonu;
	public Text _paidAmoundADD;
	public Text _paidAmoundSUB;
	// Use this for initialization
	void Start () {
		_subLoginDay.onClick.AddListener (SubLoginDayClick );
		_subDayBonu.onClick.AddListener (SubDayBonuClick);
		_addPaidAmound.onClick.AddListener (AddPaidAmoundClick);
		_subPaidAmound.onClick.AddListener (SubPaidAmoundClick);
		_testButton.onClick.AddListener (ShowOrHideButton);
	}
	
	// Update is called once per frame
	void Update () {
		_lastLoginDay.text = UserBasicData.Instance.LastLoginDateTime.ToString ();
		_lastDayBonu.text = UserBasicData.Instance.LastDayBonusDateTime.ToString ();
		_paidAmoundADD.text = string.Format ("PaidAmound:{0}", UserBasicData.Instance.TotalPayAmount.ToString ()); 
		_paidAmoundSUB.text = string.Format ("PaidAmound:{0}", UserBasicData.Instance.TotalPayAmount.ToString ()); 
	}

	private void SubLoginDayClick()
	{
		UserBasicData.Instance.SetLastLoginDateTime (UserBasicData.Instance.LastLoginDateTime.AddDays (-1));

	}

	private void SubDayBonuClick()
	{
		UserBasicData.Instance.SetLastGetDayBonusDate (UserBasicData.Instance.LastDayBonusDateTime.AddDays (-1));
	}

	private void AddPaidAmoundClick()
	{
	}
	private void SubPaidAmoundClick()
	{
	}

	private void ShowOrHideButton()
	{
		SetContrastActive (_subLoginDay.gameObject);
		SetContrastActive (_subDayBonu.gameObject);
		SetContrastActive (_subPaidAmound.gameObject);
		SetContrastActive (_addPaidAmound.gameObject);
	}

	private void SetContrastActive(GameObject gameobject)
	{
		gameobject.SetActive (!gameobject.activeSelf);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EventDropdown : MonoBehaviour
{
	public List<SendMessageData> CachedMethods = new List<SendMessageData>();

	public InputField TextValue;
	public Button button;
	public GameObject Return;
	public Text ReturnText;
	//public List<MonoBehaviour> currMonoBehaviourRegistered = new List<MonoBehaviour>();

	//public List<SendMessageData> MessageData = new List<SendMessageData>();

	public Dropdown dropdown;
	// Use this for initialization
	void Start()
	{
		dropdown.onValueChanged.AddListener(ShowOrInhind);
		Init();
	}

	void ShowOrInhind(int indix)
	{
		if(indix == 0)
		{
			TextValue.gameObject.SetActive(false);
			button.gameObject.SetActive(false);
			Return.SetActive(false);
		}
		else { TextValue.gameObject.SetActive(true); button.gameObject.SetActive(true); Return.SetActive(true); }
		TextValue.text = string.Empty;
	}

	private void Init()
	{
		Dropdown.OptionData dd = new Dropdown.OptionData("Select Event type");
		dropdown.options.Add(dd);
		for(int i = 0; i < CachedMethods.Count; i++)
		{
			Dropdown.OptionData dod = new Dropdown.OptionData(CachedMethods[i].MethodName);
			dropdown.options.Add(dod);
		}


		dropdown.value = 0;
		dropdown.RefreshShownValue();
		//ShowDefultItem(dropdown);
	}

	private void ShowDefultItem(Dropdown dd)
	{
		Dropdown.OptionData dod = new Dropdown.OptionData("Select Event type");
		dd.options.Add(dod);
		dd.value = dd.options.Count - 1;
		dd.RefreshShownValue();
		dd.options.RemoveAt(dd.options.Count - 1);
	}

	public void InvokeSelectEvent()
	{
		if(dropdown.value < 0)
		{
			return;
		}
		int index = dropdown.value - 1;
		var go = CachedMethods[index].target;
		LogUtility.Log("参数" + TextValue.text);
		if(TextValue.text == "")
		{

			go.SendMessage(CachedMethods[index].MethodName, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			go.SendMessage(CachedMethods[index].MethodName, TextValue.text, SendMessageOptions.DontRequireReceiver);
		}

		ReturnText.text = CachedMethods[index].MethodName + "调用完成";

	}

}

[Serializable]
public class SendMessageData
{
	public MonoBehaviour target;
	public string MethodName;
}

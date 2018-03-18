using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public enum UserChouseDataType
{
	UseServerData,
	UseLocalData,
}

public class UserDataUIController : Singleton<UserDataUIController>
{
	public Canvas CanvasGameObject;
	public GameObject LoadingObject;
	public GameObject ErrorObject;
	public GameObject SuccessObject;
	public GameObject AskObject;

	// Use this for initialization
	void Start()
	{
		CitrusEventManager.instance.AddListener<UserDataServerEvent>(UserDataServerMessageProcess);
		CitrusEventManager.instance.AddListener<SaveUserDataToServerEvent>(SaveUserDataToServerMessageProcess);
		CitrusEventManager.instance.AddListener<UserLogoutEvent>(UserLogoutMessageProcess);
	}

	private void UpdateUIState(GameObject go = null)
	{
		LoadingObject.SetActive(false);
		ErrorObject.SetActive(false);
		SuccessObject.SetActive(false);
		AskObject.SetActive(false);
		if(go != null)
		{
			CanvasGameObject.gameObject.SetActive(true);
			go.SetActive(true);
		}
		else
		{
			CanvasGameObject.gameObject.SetActive(false);
		}
	}

	private void UserDataServerMessageProcess(UserDataServerEvent uds)
	{
		switch(uds.ShowUi)
		{
			case UserDataServerEvent.ShowUI.Error:
				if(MainMapBar.GameIsPlaying)
					UpdateUIState(ErrorObject);
				else
					UpdateUIState();
				Debug.LogError("UserDataServerMessageProcess: server error");
				break;

			case UserDataServerEvent.ShowUI.Loading:
				if(MainMapBar.GameIsPlaying)
					UpdateUIState(LoadingObject);
				else
					UpdateUIState();
				break;

			case UserDataServerEvent.ShowUI.Success:
				UpdateUIState();
				break;

			case UserDataServerEvent.ShowUI.Ask:
				//don't check MainMapBar.GameIsPlaying since Ask UI might appear in StartLoading scene
				UpdateUIState(AskObject);
				break;

			case UserDataServerEvent.ShowUI.NoOne:
				UpdateUIState();
				break;

			default:
				break;
		}
	}

	private void SaveUserDataToServerMessageProcess(SaveUserDataToServerEvent sudtm)
	{
		UserDataHelper.Instance.SaveUserDataToServer(false);
	}

	private void UserLogoutMessageProcess(UserLogoutEvent ulm)
	{
		UserDataHelper.Instance.UserSocialLogoutCallback();
	}

	public void UseServerData()
	{
		UpdateUIState();
		//for safety, check the return result
		bool isOverwrite = UserDataHelper.Instance.UseDFMD5ServerDataOverlayUserData();
		if(isOverwrite)
			UserDataHelper.Instance.ForceBindDevice();
		UserDataHelper.Instance.HandleUserChooseAskEnd();
		CitrusEventManager.instance.Raise(new UserChouseUserDataEvent(UserChouseDataType.UseServerData));
	}

	public void UseLocal()
	{
		UpdateUIState();
		UserDataHelper.Instance.SaveUserDataToServer(true);
		UserDataHelper.Instance.HandleUserChooseAskEnd();
		CitrusEventManager.instance.Raise(new UserChouseUserDataEvent(UserChouseDataType.UseLocalData));
	}
}

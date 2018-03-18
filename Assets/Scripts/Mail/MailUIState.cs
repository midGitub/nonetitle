using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class MailUIState : MonoBehaviour 
{
	public GameObject NewGameObject;

	public void Start()
	{
		UpdateState();
		CitrusEventManager.instance.AddListener<UpdateMailBarStateEvent>(UpdateMessageP);
	}

	public void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<UpdateMailBarStateEvent>(UpdateMessageP);
	}

	public void UpdateMessageP(UpdateMailBarStateEvent upevent)
	{
		UpdateState();
	}

	public void UpdateState()
	{
		bool show = TryGetMailToShow ();
		NewGameObject.SetActive (show);
	}

	private bool TryGetMailToShow()
	{
		var first = DictionaryUtility.DictionaryFirst(UserBasicData.Instance.MailInforDic, (obj) => {
			return obj.State == MailState.DoneConfirm; 
		});
		bool isNotnull = first != default(string);
		return isNotnull;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseAndOpenUI : MonoBehaviour
{
	public GameObject OpenGameObject;
	public GameObject CloseGameObject;

	public void UpdateState(bool Open)
	{
		if(OpenGameObject != null)
		{
			OpenGameObject.SetActive(Open);
		}
		if(CloseGameObject != null)
		{
			CloseGameObject.SetActive(!Open);
		}
	}
}

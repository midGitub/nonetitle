using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class NoCollectionErrorObject : Singleton<NoCollectionErrorObject>
{
	public Canvas NoCanvas;

	public void Show()
	{
		NoCanvas.gameObject.SetActive(true);
	}
}

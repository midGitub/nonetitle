using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;

public class MakeSpriteForFaceBook : MonoBehaviour
{
	public Image icon;

	public void GetIcon(string uid)
	{
		StartCoroutine(NetWorkHelper.GetDownloadingPicture(this, uid, (Sprite obj) =>
		{ icon.sprite = obj; },null,100));
	}
}

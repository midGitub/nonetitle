using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class MenuExtension
{
	//reference:
	//http://www.xuanyusong.com/archives/4006
	[MenuItem("GameObject/UI/Image")]
	static void CreatImage()
	{
		if(Selection.activeTransform)
		{
			if(Selection.activeTransform.GetComponentInParent<Canvas>())
			{
				GameObject go = new GameObject("image",typeof(Image));
				go.GetComponent<Image>().raycastTarget = false;
				go.transform.SetParent(Selection.activeTransform);
			}
		}
	}

	// 复制资源路径到剪贴板  
	[MenuItem("Assets/Copy Asset Path to ClipBoard")]
	static void CopyAssetPath2Clipboard()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
		TextEditor text2Editor = new TextEditor();
		text2Editor.text = path;
		text2Editor.OnFocus();
		text2Editor.Copy();
	}
}

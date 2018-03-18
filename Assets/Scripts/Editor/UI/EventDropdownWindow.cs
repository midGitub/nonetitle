using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EventDropdownWindow : EditorWindow
{
	private GUIStyle dropBox;
	public EventDropdown EventDropdown;
	private SerializedObject _serializedObject;

	void Awake()
	{
		dropBox = new GUIStyle();
		dropBox.normal.background = MakeTex(new Color(1, 1, 1, 0.5f));
		dropBox.margin = new RectOffset(4, 4, 4, 4);
		dropBox.alignment = TextAnchor.MiddleCenter;
		dropBox.fontSize = 14;
		dropBox.normal.textColor = Color.black;
	}

	void OnGUI()
	{
		_serializedObject = new SerializedObject(EventDropdown);

		DropArea();
	}

	private void DropArea()
	{
		GUILayout.Box("DropNeedRegisteredScriptHere", dropBox, GUILayout.ExpandWidth(true), GUILayout.Height(35));

		EventType eventType = Event.current.type;
		bool isAccepted = false;

		if(eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if(eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				isAccepted = true;
			}
			Event.current.Use();
		}

		if(isAccepted)
		{
			foreach(var item in DragAndDrop.objectReferences)
			{
				Debug.Log(item.name);
			}
			var mons = DragAndDrop.objectReferences
			                      .Where(obj => obj.GetType() == typeof(MonoBehaviour))
			                      .Cast<MonoBehaviour>();
			foreach(var go in mons)
			{
				var mos = go.GetComponents<MonoBehaviour>();

				for(int i = 0; i < mons.Count(); i++)
				{
					EditorGUILayout.BeginHorizontal();



					EditorGUILayout.EndHorizontal();
				}
			}
			// EventDropdown.currMonoBehaviourRegistered.AddRange(mons);

			// EditorUtility.SetDirty(target);
			_serializedObject.ApplyModifiedProperties();
		}
	}

	static readonly string[] ignoredMethodNames = new string[] {
		"Start", "Awake", "OnEnable", "OnDisable",
		"Update", "LateUpdate", "FixedUpdate"
	};

	/// <summary>
	/// 从 MonoBehaviour 里得到所有方法名
	/// </summary>
	/// <param name="go"></param>
	/// <param name="parameterType"></param>
	private List<SendMessageData> CacheMethodsForGameObject(MonoBehaviour go)
	{
		List<SendMessageData> cachedMethods = new List<SendMessageData>();

		List<System.Type> addedTypes = new List<System.Type>();
		System.Type type = go.GetType();
		if(addedTypes.IndexOf(type) == -1)
		{
			System.Reflection.MethodInfo[] methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
			foreach(System.Reflection.MethodInfo method in methods)
			{
				// Only add variables added by user, i.e. we don't want functions from the base UnityEngine baseclasses or lower
				string moduleName = method.DeclaringType.Assembly.ManifestModule.Name;
				if(!moduleName.Contains("UnityEngine") && !moduleName.Contains("mscorlib") &&
					!method.ContainsGenericParameters &&
					System.Array.IndexOf(ignoredMethodNames, method.Name) == -1)
				{
					System.Reflection.ParameterInfo[] paramInfo = method.GetParameters();
					if(paramInfo.Length == 0)
					{
						cachedMethods.Add(new SendMessageData { target = go, MethodName = method.Name });
					}
					else if(paramInfo.Length == 1)
					{
						cachedMethods.Add(new SendMessageData { target = go, MethodName = method.Name });
					}
				}
			}
		}


		return cachedMethods;
	}

	private Texture2D MakeTex(Color col)
	{
		Color[] pix = new Color[1 * 1];

		for(int i = 0; i < pix.Length; i++)
			pix[i] = col;

		Texture2D result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		result.hideFlags = HideFlags.HideAndDontSave;
		result.SetPixels(pix);
		result.Apply();

		return result;
	}

	void OnInspectorUpdate()
	{
		this.Repaint();  //重新画窗口  
	}
}

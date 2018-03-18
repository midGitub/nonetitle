using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventDropdown))]
public class EventDropdownEditor : Editor
{
	private EventDropdown _eventDropdown;

	public override void OnInspectorGUI()
	{
		_eventDropdown = (EventDropdown)target;
		base.OnInspectorGUI();

		var targetScript = _eventDropdown.GetComponent<NeedTestFunction>();

		_eventDropdown.CachedMethods = CacheMethodsForGameObject(targetScript);

		EditorUtility.SetDirty(target);

		//System.Type type = _eventDropdown.EventList.GetType();
		//MethodInfo[] methods = type.GetMethods();
		//MethodInfo addlinister;
		//foreach(var item in methods)
		//{
		//	if(item.IsPublic)
		//	{
		//		continue;
		//	}
		//	else
		//	{
		//		if(item.Name=="AddListener")
		//		{
		//			addlinister = item;
		//		}
		//	}
		//}

	}

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
		System.Reflection.MethodInfo[] methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
		foreach(System.Reflection.MethodInfo method in methods)
		{
			if(method.DeclaringType == type)
			{
				System.Reflection.ParameterInfo[] paramInfo = method.GetParameters();
				if(paramInfo.Length == 0 || paramInfo.Length == 1)
					cachedMethods.Add(new SendMessageData { target = go, MethodName = method.Name });
			}
		}

		return cachedMethods;
	}

}

// For performance consideration, only use the cool dynamic features in editor mode
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

// Provide convenient functions to use reflection features.
// More features related to System.Type could be implemented in the future, including:
// ConstructInfo, EventInfo, InterfaceInfo, MemberInfo
// But they're not needed for now
public static class ObjectExtension
{
	// Dynamic call method, it simulates Object#send in Ruby
	public static object Send(this object obj, string methodName, object[] paras)
	{
		object result = null;
		Type t = obj.GetType();
		MethodInfo info = t.GetMethod(methodName);
		Debug.Assert(info != null);
		result = info.Invoke(obj, paras);
		return result;
	}

	public static void SetFieldValue(this object obj, string fieldName, object value)
	{
		Type t = obj.GetType();
		FieldInfo info = t.GetField(fieldName);
		Debug.Assert(info != null);
		info.SetValue(obj, value);
	}

	public static object GetFieldValue(this object obj, string fieldName)
	{
		Type t = obj.GetType();
		FieldInfo info = t.GetField(fieldName);
		Debug.Assert(info != null);
		object result = info.GetValue(obj);
		return result;
	}

	public static void SetPropertyValue(this object obj, string propertyName, object value)
	{
		Type t = obj.GetType();
		PropertyInfo info = t.GetProperty(propertyName);
		Debug.Assert(info != null);
		info.SetValue(obj, value, null);
	}

	public static object GetPropertyValue(this object obj, string propertyName)
	{
		Type t = obj.GetType();
		PropertyInfo info = t.GetProperty(propertyName);
		Debug.Assert(info != null);
		object result = info.GetValue(obj, null);
		return result;
	}
}

#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ExtensionUtility
{
	//Breadth-first search
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		var result = aParent.Find(aName);
		if (result != null)
			return result;
		foreach(Transform child in aParent)
		{
			result = child.FindDeepChild(aName);
			if (result != null)
				return result;
		}
		return null;
	}

	public static bool Contains<T>(this T[] array, T element)
	{
		int index = Array.IndexOf(array, element);
		return index >= 0;
	}

	/*
     //Depth-first search
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		foreach(Transform child in aParent)
		{
			if(child.name == aName )
				return child;
			var result = child.FindDeepChild(aName);
			if (result != null)
				return result;
		}
		return null;
	}
	*/
}

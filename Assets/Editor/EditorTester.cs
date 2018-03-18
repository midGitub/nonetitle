using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public static class EditorTester
{
	[MenuItem("Build/Test")]
	public static void Test()
	{
		Debug.Log("Debug log");
		Console.WriteLine("Console log");

		foreach(var arg in Environment.GetCommandLineArgs())
		{
			Debug.Log("arg is:" + arg);
		}
	}
}


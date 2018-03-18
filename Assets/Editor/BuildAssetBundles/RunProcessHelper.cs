using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEditor;
using System.IO;

public static class RunProcessHelper
{
	public static void Run(string arguments, string workingDir)
	{
		Process process = new Process();
		if(Application.platform == RuntimePlatform.OSXEditor)
			process.StartInfo.FileName = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
		else if(Application.platform == RuntimePlatform.WindowsEditor)
			process.StartInfo.FileName = "cmd.exe";
		else
			UnityEngine.Debug.Assert(false, "Platform error");

		process.StartInfo.Arguments = arguments;
		process.StartInfo.WorkingDirectory = workingDir;
//		process.StartInfo.CreateNoWindow = true;
//		process.StartInfo.UseShellExecute = false;
//		process.StartInfo.RedirectStandardInput = true;
//		process.StartInfo.RedirectStandardOutput = true;

		process.Start();

//		process.WaitForExit();
	}
}


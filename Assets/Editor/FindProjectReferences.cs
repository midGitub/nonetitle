/*
MIT License

Copyright (c) 2016 Jesse Ringrose

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

// Reference:
// https://gist.github.com/jringrose/617d4cba87757591ce28
// Based on the online solution, I changed the implementation from sync to async, so that no blocking happens

public class FindProject {
	
	#if UNITY_EDITOR_OSX
	
	[MenuItem("Assets/Find References In Project", false, 2000)]
	private static void FindProjectReferences()
	{
		string appDataPath = Application.dataPath;
		string output = "";
		string selectedAssetPath = AssetDatabase.GetAssetPath (Selection.activeObject);
		List<string> references = new List<string>();
		
		string guid = AssetDatabase.AssetPathToGUID (selectedAssetPath);
		
		var psi = new System.Diagnostics.ProcessStartInfo();
		psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
		psi.FileName = "/usr/bin/mdfind";
		psi.Arguments = "-onlyin " + Application.dataPath + " " + guid;
		psi.UseShellExecute = false;
		psi.RedirectStandardOutput = true;
		psi.RedirectStandardError = true;
		
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo = psi;
		
		process.OutputDataReceived += (sender, e) => {
			if(string.IsNullOrEmpty(e.Data))
				return;
			
			string relativePath = "Assets" + e.Data.Replace(appDataPath, "");
			
			// skip the meta file of whatever we have selected
			if(relativePath == selectedAssetPath + ".meta")
				return;
			
			references.Add(relativePath);
		};

		process.ErrorDataReceived += (sender, e) => {
			if(string.IsNullOrEmpty(e.Data))
				return;
			
			Debug.Log("Error: " + e.Data);
		};

		process.Exited += (object sender, System.EventArgs e) => {
			Debug.Log("### " + references.Count + " references found");

			foreach(var file in references){
				// Note:
				// If you call any UnityEditor function here, there would be some error messages like this:
				// LoadMainAssetAtPath can only be called from the main thread.
//				Debug.Log(file, AssetDatabase.LoadMainAssetAtPath(file));
				Debug.Log(file);
			}
		};

		process.EnableRaisingEvents = true;
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
	}
	
	#endif
}
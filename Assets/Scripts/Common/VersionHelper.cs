using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class VersionHelper {

	public static void WriteVersion()
	{
		if(!IsVersionExist())
		{
			StreamWriter sw = new StreamWriter (GetVersionPath (),true);  
			sw.WriteLine(BuildUtility.GetBundleVersion());	 	  	 
			sw.Flush();	 	 
			sw.Close();	 	
		}
	}

	public static bool IsVersionExist(string version=null)
	{
		if (version.IsNullOrEmpty ())
			version = BuildUtility.GetBundleVersion ();
		foreach (string oldVersion in File.ReadAllLines(GetVersionPath())) 
		{
			if (oldVersion == version)
				return true;
		}
		return false;
	}

	public static List<string> GetAllVersion()
	{
		return new List<string>(File.ReadAllLines(GetVersionPath()));
	}
		
	private static string GetVersionPath()
	{
		string dir=Path.Combine (Application.persistentDataPath,"HistoryVersion");
		if (!Directory.Exists (dir))
			Directory.CreateDirectory (dir);
		string filepath = Path.Combine (dir, "HistoryVersion");
		if (!File.Exists (filepath))
			File.Create (filepath).Dispose();
		return filepath;
	}
}

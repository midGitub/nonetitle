using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileHelper
{
	public static bool CopyFile(string oldPath, string newPath)
	{
		if(File.Exists(oldPath) && !File.Exists(newPath))
		{
			File.Copy(oldPath, newPath);
			return true;
		}
		Debug.LogError("已经存在文件不能copy");
		return false;
	}

	public static string CreateFolder(string path, string FolderName)
	{
		string FolderPath = Path.Combine(path, FolderName);
		if(!Directory.Exists(FolderPath))
			Directory.CreateDirectory(FolderPath);
		return FolderPath;
	}

	public static bool RenameFile(string oldpath, string newpath)
	{
		if(File.Exists(oldpath) && !File.Exists(newpath))
		{
			File.Move(newpath, newpath);
			return true;
		}

		return false;
	}

	//This is ugly due to Unity's API design
	//Reference:
	//http://answers.unity3d.com/questions/1225077/using-streamingassets-in-android.html
	//http://www.xuanyusong.com/archives/4033
	public static byte[] ReadFromStreamingAsset(string path)
	{
		byte[] result = null;
		if (Application.platform == RuntimePlatform.Android)
		{
			WWW reader = new WWW(path);
			while (!reader.isDone) { }

			result = reader.bytes;
		}
		else
		{
			if(File.Exists(path))
				result = File.ReadAllBytes(path);
		}

		return result;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class TextureUtility
{
	public static void SaveTexture(string path, Texture2D tex)
	{
		byte[] data = tex.EncodeToPNG();
		File.WriteAllBytes(path, data);
	}

	public static Texture2D LoadTexture(string path)
	{
		Texture2D tex = null;
		if(File.Exists(path))
		{
			byte[] data = File.ReadAllBytes(path);
			tex = new Texture2D(32, 32);
			tex.LoadImage(data);
		}
		return tex;
	}
}


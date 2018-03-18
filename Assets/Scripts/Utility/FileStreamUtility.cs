using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class FileStreamUtility
{
	public static StreamWriter CreateFileStream(string filePath)
	{
		StreamWriter stream = new System.IO.StreamWriter(filePath, true);
		return stream;
	}

	public static void WriteFile(StreamWriter stream, string content)
	{
		stream.WriteLine(content);
	}

	public static void CloseFile(StreamWriter stream)
	{
		stream.Close();
	}

	public static void DeleteFile(string filePath)
	{
		File.Delete(filePath);
	}
}

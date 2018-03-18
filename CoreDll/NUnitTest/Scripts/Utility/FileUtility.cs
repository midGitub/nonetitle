using System.Collections;
using System.IO;

public class FileUtility
{
	#region Write

	public static StreamWriter CreateFileStreamWriter(string filePath)
	{
		StreamWriter stream = new System.IO.StreamWriter(filePath, true);
		return stream;
	}

	public static void WriteFile(StreamWriter stream, string content)
	{
		stream.WriteLine(content);
	}

	public static void CloseWriteStream(StreamWriter stream)
	{
		stream.Close();
	}

	public static void DeleteFile(string filePath)
	{
		File.Delete(filePath);
	}

	public static void WriteFile(string filePath, string content)
	{
		StreamWriter stream = CreateFileStreamWriter(filePath);
		WriteFile(stream, content);
		CloseWriteStream(stream);
	}

	#endregion

	#region Read

	public static StreamReader CreateFileStreamReader(string filePath)
	{
		StreamReader stream = new System.IO.StreamReader(filePath, true);
		return stream;
	}

	public static string ReadFile(StreamReader stream)
	{
		return stream.ReadToEnd();
	}

	public static void CloseReadStream(StreamReader stream)
	{
		stream.Close();
	}

	public static string ReadFile(string filePath)
	{
		StreamReader stream = CreateFileStreamReader(filePath);
		string result = ReadFile(stream);
		CloseReadStream(stream);
		return result;
	}

	#endregion
}


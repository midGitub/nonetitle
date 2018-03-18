using UnityEngine;
using System;
using System.Collections;
using System.IO;
using CitrusFramework;

public class CoreMachineTest
{
	public CoreMachineTest()
	{
	}

	public void Run()
	{
		TestAllMachines();
	}

	private void TestAllMachines()
	{
		TestMachine("M1", 10000);
	}

	private void TestMachine(string name, int count)
	{
		string fileName = name + ".txt";
		DeleteFile(fileName);
		StreamWriter stream = CreateFileStream(fileName);

		CoreMachine machine = new CoreMachine(name, CoreDefine.DefaultMachineRandSeed);
		for(int i = 0; i < count; i++)
		{
			CoreSpinResult spinResult = machine.Spin(new CoreSpinInput(0, machine.MachineConfig.BasicConfig.ReelCount, false));
			string content = spinResult.ToString();
			WriteFile(stream, content);
		}

		CloseFile(stream);
	}

	private string GetFileFullPath(string fileName)
	{
		string appPath = Application.dataPath;
		string filePath = appPath + "/Test/" + fileName;
		return filePath;
	}

	private StreamWriter CreateFileStream(string fileName)
	{
		string filePath = GetFileFullPath(fileName);
		StreamWriter stream = new System.IO.StreamWriter(filePath, true);
		return stream;
	}

	private void WriteFile(StreamWriter stream, string content)
	{
		stream.WriteLine(content);
	}

	private void CloseFile(StreamWriter stream)
	{
		stream.Close();
	}

	private void DeleteFile(string fileName)
	{
		string filePath = GetFileFullPath(fileName);
		File.Delete(filePath);
	}
}

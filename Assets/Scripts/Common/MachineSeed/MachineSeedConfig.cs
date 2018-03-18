using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MachineSeedConfig
{
	public static readonly char SeedFileDelimitor = '\n';
	public static readonly string SeedFileDir = "MachineSeeds/";

	private static readonly string _seedFilePostFix = "_seeds";

	public static string GetSeedFileName(string machineName, bool withExtension)
	{
		string fileName = machineName + "_seeds";
		if(withExtension)
			fileName += ".csv";
		return fileName;
	}
}

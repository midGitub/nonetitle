using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionInfo
{
	public int _major;
	public int _minor;
	public int _revision;
}

public static class VersionUtility
{
	public static VersionInfo GetVersionInfo(string version)
	{
		VersionInfo info = new VersionInfo();
		string[] array = version.Split('.');
		Debug.Assert(array.Length >= 3);

		info._major = int.Parse(array[0]);
		info._minor = int.Parse(array[1]);
		info._revision = int.Parse(array[2]);

		return info;
	}

	public static string GetVersionString(VersionInfo info)
	{
		string result = string.Format("{0}.{1}.{2}", info._major, info._minor, info._revision);
		return result;
	}

	public static bool CanVersionUpdate(string v1, string v2)
	{
		bool result = false;
		VersionInfo info1 = VersionUtility.GetVersionInfo(v1);
		VersionInfo info2 = VersionUtility.GetVersionInfo(v2);

		if(info1._major == info2._major
			&& info1._minor == info2._minor
			&& info1._revision < info2._revision)
		{
			result = true;
		}
		return result;
	}

	public static bool IsVersionHigher(string lowVer, string highVer)
	{
		VersionInfo lowInfo = VersionUtility.GetVersionInfo(lowVer);
		VersionInfo highInfo = VersionUtility.GetVersionInfo(highVer);

		bool result = (lowInfo._major < highInfo._major)
			|| (lowInfo._major == highInfo._major && lowInfo._minor < highInfo._minor)
			|| (lowInfo._major == highInfo._major && lowInfo._minor == highInfo._minor && lowInfo._revision < highInfo._revision);
		return result;
	}
}

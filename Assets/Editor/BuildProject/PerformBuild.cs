using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

class PerformBuild
{
	static readonly char[] _settingDelimitor = new char[]{'-'};

	static string[] GetBuildScenes()
	{
		List<string> names = new List<string>();

		foreach(UnityEditor.EditorBuildSettingsScene e in UnityEditor.EditorBuildSettings.scenes)
		{
			if(e==null)
				continue;

			if(e.enabled)
				names.Add(e.path);
		}
		return names.ToArray();
	}

	static string GetIOSBuildPath()
	{
		string dirPath = Application.dataPath +"/../xcode";
		if(!System.IO.Directory.Exists(dirPath)){
			System.IO.Directory.CreateDirectory(dirPath);
		}
		return dirPath;
	}

	static string[] GetCommandLineSettingArgs()
	{
		string[] result = null;
		foreach(string arg in System.Environment.GetCommandLineArgs())
		{
			if(arg.StartsWith("setting"))
			{
				result = arg.Split(_settingDelimitor);
				break;
			}
		}
		return result;
	}

	static string GetVersionNumber()
	{
		string[] args = GetCommandLineSettingArgs();
		return args[1];
	}

	static void WriteIOSVersionCode()
	{
		File.WriteAllText (Path.Combine(Application.dataPath,"Resources/versionCode.txt"), PlayerSettings.iOS.buildNumber.ToString());
	}

	static void WriteAndroidVersionCode()
	{
		File.WriteAllText (Path.Combine(Application.dataPath,"Resources/versionCode.txt"), PlayerSettings.Android.bundleVersionCode.ToString());
	}

	static string GetVersionCode()
	{
		string[] args = GetCommandLineSettingArgs();
		return args[2];
	}

	static string GetBuildSymbol()
	{
		string[] args = GetCommandLineSettingArgs();
		return args[3];
	}

	static string GetAndroidBuildPath()
	{
		string[] args = GetCommandLineSettingArgs();
		return args[4];
	}

    static string GetiOSChannelType()
    {
        string[] args = GetCommandLineSettingArgs();
        return args[4];
    }

    static void SetIOSBuildVersion()
	{
		PlayerSettings.bundleVersion = GetVersionNumber();
		PlayerSettings.iOS.buildNumber = GetVersionCode();
	}

    static void SetIOSBuildBundleId(BasePackageConfig packageConfig)
    {
        PlayerSettings.bundleIdentifier = packageConfig.iOSBundleIdentifier;
    }

    static void SetiOSProductName(BasePackageConfig packageConfig)
    {
        PlayerSettings.productName = packageConfig.ProductName;
    }

    static void SetAndroidBuildVersion()
	{
		PlayerSettings.bundleVersion = GetVersionNumber();
		string str = GetVersionCode();
		PlayerSettings.Android.bundleVersionCode = int.Parse(str);
	}

	static void CheckScenes(string []scenes)
	{
		if(scenes == null || scenes.Length == 0)
		{
			Debug.LogError("Error: No scenes or path to build");
			return;
		}

		for(int i = 0; i < scenes.Length; ++i)
			Debug.Log(string.Format("Scene[{0}]: \"{1}\"", i, scenes[i]));
	}

	[MenuItem("Build/Build iPhone")]
	static void CommandLineBuildIphone ()
	{
		SwitchBuildPlatform(BuildTarget.iOS);

		Debug.Log("\n------------------\nStart build iOS from command line\n------------------\n");

		string[] scenes = GetBuildScenes();
		string path = GetIOSBuildPath();
		CheckScenes(scenes);

        BasePackageConfig packageConfig =
           PackageConfigManager.Instance.GetiOSBuildConfig(GetiOSChannelType() == ChannelType.iOS_IW.ToString());

        SetIOSBuildVersion();
        SetIOSBuildBundleId(packageConfig);
        SetiOSProductName(packageConfig);
		WriteIOSVersionCode ();

        string symbol = GetBuildSymbol();
	    string defineSymbols = "";
        bool isiOSIWChannel = GetiOSChannelType() == ChannelType.iOS_IW.ToString();
        if (symbol == "Debug")
		{
			EditorUserBuildSettings.development = true;
            defineSymbols = isiOSIWChannel ? "DEBUG;Trojan_FB;USE_iOS_IW" : "DEBUG;Trojan_FB";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbols);
        }
		else if(symbol == "Release")
		{
			EditorUserBuildSettings.development = false;
            defineSymbols = isiOSIWChannel ? "RELEASE;Trojan_FB;USE_iOS_IW" : "RELEASE;Trojan_FB";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbols);
		}
		else if(symbol == "Release_AppStore")
		{
			EditorUserBuildSettings.development = false;
            defineSymbols = isiOSIWChannel ? "RELEASE;RELEASE_APPSTORE;Trojan_FB;USE_iOS_IW" : "RELEASE;RELEASE_APPSTORE;Trojan_FB";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbols);
		}
		else
		{
			Debug.LogError("Error: no Debug or Release symbol is indicated");
		}

		SetFacebookAppId(packageConfig);
        ConfigureChannelFiles(isiOSIWChannel);

        //DeleteFabricIcon ();

		PreProcessBuildPlayer(BuildTarget.iOS);

		string message = UnityEditor.BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, BuildOptions.None);
		if(message.IsNullOrEmpty())
			System.Console.WriteLine("BuildPlayer Success!");
		else
			System.Console.WriteLine("BuildPlayer error message: " + message.ToString());

		PostProcessBuildPlayer();
	}

	[MenuItem("Build/Local Build Debug Android")]
	static void LocalBuildDebugAndroid ()
	{
		SwitchBuildPlatform(BuildTarget.Android);
		
		Debug.Log("\n------------------\nStart build Android from command line\n------------------\n");

		PlayerSettings.Android.keystoreName = Application.dataPath + "/Plugins/Android/android.keystore";
		PlayerSettings.Android.keyaliasName = "trojan";
		PlayerSettings.keystorePass = "123456";
		PlayerSettings.keyaliasPass = "123456";
		WriteAndroidVersionCode ();
		string[] scenes = GetBuildScenes();
		CheckScenes(scenes);

		string path = "Build/";
		if(!System.IO.Directory.Exists(path))
			System.IO.Directory.CreateDirectory(path);

		PlayerSettings.bundleIdentifier = "com.citrusjoy.trojan";
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "DEBUG;Trojan_FB");
		EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

		PreProcessBuildPlayer(BuildTarget.Android);

		path += "Trojan_" + PlayerSettings.bundleVersion + "_" + "Debug" + ".apk";

		string message = UnityEditor.BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);
		if(message.IsNullOrEmpty())
			System.Console.WriteLine("BuildPlayer Success!");
		else
			System.Console.WriteLine("BuildPlayer error message: " + message.ToString());
		
		PostProcessBuildPlayer();
	}

	[MenuItem("Build/Local Build Release Android")]
	static void LocalBuildReleaseAndroid ()
	{
		SwitchBuildPlatform(BuildTarget.Android);

		Debug.Log("\n------------------\nStart build Android from command line\n------------------\n");

		PlayerSettings.Android.keystoreName = Application.dataPath + "/Plugins/Android/android.keystore";
		PlayerSettings.Android.keyaliasName = "trojan";
		PlayerSettings.keystorePass = "123456";
		PlayerSettings.keyaliasPass = "123456";
		WriteAndroidVersionCode ();
		string[] scenes = GetBuildScenes();
		CheckScenes(scenes);

		string path = "Build/";
		if(!System.IO.Directory.Exists(path))
			System.IO.Directory.CreateDirectory(path);

		PlayerSettings.bundleIdentifier = "com.citrusjoy.trojan";
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "RELEASE;Trojan_FB");
		EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

		PreProcessBuildPlayer(BuildTarget.Android);

		path += "Trojan_" + PlayerSettings.bundleVersion + "_" + "Release" + ".apk";

		string message = UnityEditor.BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);
		if(message.IsNullOrEmpty())
			System.Console.WriteLine("BuildPlayer Success!");
		else
			System.Console.WriteLine("BuildPlayer error message: " + message.ToString());

		PostProcessBuildPlayer();
	}

	[MenuItem("Build/Build Android")]
	static void CommandLineBuildAndroid ()
	{
		SwitchBuildPlatform(BuildTarget.Android);
		
		Debug.Log("\n------------------\nStart build Android from command line\n------------------\n");

		PlayerSettings.Android.keystoreName = Application.dataPath + "/Plugins/Android/android.keystore";
		PlayerSettings.Android.keyaliasName = "trojan";
		PlayerSettings.keystorePass = "123456";
		PlayerSettings.keyaliasPass = "123456";

		string[] scenes = GetBuildScenes();
		string path = GetAndroidBuildPath();
		CheckScenes(scenes);

		SetAndroidBuildVersion();
		WriteAndroidVersionCode ();
		EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

		PlayerSettings.bundleIdentifier = "com.citrusjoy.trojan";
		string symbol = GetBuildSymbol();

		if(symbol == "Debug")
		{
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "DEBUG;Trojan_FB");
		}
		else if(symbol == "Release")
		{
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "RELEASE;Trojan_FB");
		}
		else
		{
			Debug.LogError("Error: no Debug or Release symbol is indicated");
		}

		PreProcessBuildPlayer(BuildTarget.Android);

		path += "_" + PlayerSettings.bundleVersion + "." + GetVersionCode() + "_" + symbol + ".apk";

		string message = UnityEditor.BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);
		if(message.IsNullOrEmpty())
			System.Console.WriteLine("BuildPlayer Success!");
		else
			System.Console.WriteLine("BuildPlayer error message: " + message.ToString());

		PostProcessBuildPlayer();
	}

	static void SetFacebookAppId(BasePackageConfig packageConfig)
	{
        Facebook.Unity.Settings.FacebookSettings.SelectedAppIndex = packageConfig.FacebookAppIdIndex;
		Debug.Log("Select Facebook AppId: " + Facebook.Unity.Settings.FacebookSettings.AppId);
	}

    static void ConfigureChannelFiles(bool isIWChannel)
    {
        if (isIWChannel)
        {
            BasePackageConfig iwBuildConfig = PackageConfigManager.Instance.GetiOSBuildConfig(true);
            BasePackageConfig normalBuildConfig = PackageConfigManager.Instance.GetiOSBuildConfig(false);
            Dictionary<string, string> pathInfoDic = new Dictionary<string, string>
            {
                { Application.dataPath + iwBuildConfig.LoadingTexPath + "loading0000000000.jpg", Application.dataPath + normalBuildConfig.LoadingTexPath + "loading0000000000.jpg"},
                { Application.dataPath + iwBuildConfig.SvUdidFilePath + "SvUDIDTools.m", Application.dataPath + normalBuildConfig.SvUdidFilePath + "SvUDIDTools.m"}
            };

            foreach (var pathInfo in pathInfoDic)
            {
                string srcPath = pathInfo.Key;
                string desPath = pathInfo.Value;
                FileInfo file = new FileInfo(srcPath);
                if (file.Exists)
                    file.CopyTo(desPath, true);
            }

            AssetDatabase.Refresh();
        }
    }

//    static void DeleteFabricIcon()
//	{
//		string dir = Path.Combine(Application.dataPath, "Plugins/Android/fabric/res/");
//		DirectoryInfo info = new DirectoryInfo(dir);
//		if(info.Exists)
//		{
//			FileInfo[] infos = info.GetFiles("*.png", SearchOption.AllDirectories);
//			foreach(FileInfo i in infos)
//			{
//				File.Delete(i.FullName);
//			}
//
//			AssetDatabase.Refresh ();
//		}
//	}

	static void PreProcessBuildPlayer(BuildTarget target)
	{
		if(target == BuildTarget.iOS)
			BuildExcelAssetBundles.BuildiOSExcels();
		else if(target == BuildTarget.Android)
			BuildExcelAssetBundles.BuildAndroidExcels();
		else
			Debug.Assert(false);
		
		BuildExcelAssetBundles.PreProcessBuildPlayer();
		MoveOutSvnOfStreamingAssets();
	}

	static void PostProcessBuildPlayer()
	{
		BuildExcelAssetBundles.PostProcessBuildPlayer();
		MoveInSvnOfStreamingAssets();
	}

	#region Handle .svn folder in StreamingAssets

	static string _tempSvnDir = "../StreamingAssets_Temp/";
	static string _svnFolderName = ".svn";
	static bool _shouldMoveSvn = false;

	static void MoveOutSvnOfStreamingAssets()
	{
		string src = GetStreamingAssetsSvnPath();
		if(Directory.Exists(src))
		{
			_shouldMoveSvn = true;

			string tempDir = GetTempSvnDir();
			if(!Directory.Exists(tempDir))
				Directory.CreateDirectory(tempDir);

			string dest = GetTempSvnPath();
			if(!Directory.Exists(dest))
				Directory.Move(src, dest);
		}
	}

	static void MoveInSvnOfStreamingAssets()
	{
		if(_shouldMoveSvn)
		{
			string src = GetTempSvnPath();
			string dest = GetStreamingAssetsSvnPath();
			if(Directory.Exists(src) && !Directory.Exists(dest))
				Directory.Move(src, dest);

			string tempDir = GetTempSvnDir();
			if(Directory.Exists(tempDir))
				Directory.Delete(tempDir);

			_shouldMoveSvn = false;
		}
	}

	static string GetTempSvnDir()
	{
		return Path.Combine(Application.dataPath, _tempSvnDir);
	}

	static string GetTempSvnPath()
	{
		return Path.Combine(GetTempSvnDir(), _svnFolderName);
	}

	static string GetStreamingAssetsSvnPath()
	{
		return Path.Combine(Application.streamingAssetsPath, _svnFolderName);
	}

	#endregion

	#region Utility

	public static void SwitchBuildPlatform(BuildTarget target)
	{
		if(EditorUserBuildSettings.activeBuildTarget != target)
			EditorUserBuildSettings.SwitchActiveBuildTarget(target);
	}

	#endregion
}


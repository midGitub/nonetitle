using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;
using System.Collections.Generic;

public static class XCodePostProcess
{

#if UNITY_EDITOR
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		if (target != BuildTarget.iOS) {
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}

		string path = Path.GetFullPath (pathToBuiltProject);

		bool isDebugBuild = GetIsAppBuildSymbol ("Debug");
		bool isReleaseBuild = GetIsAppBuildSymbol ("Release");

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

		// Find and run through all projmods files to patch the project.
		// Please pay attention that ALL projmods files in your project folder will be excuted!
		string[] files = Directory.GetFiles( Application.dataPath, "*.projmods", SearchOption.AllDirectories );
		foreach( string file in files ) {
			UnityEngine.Debug.Log("ProjMod File: "+file);
			if (file.Contains ("Debug")) {
				if (isDebugBuild) {
					Debug.Log("debug applymod is " + file);
					project.ApplyMod( file );
				}

			} else if (file.Contains ("Release")) {
				if (isReleaseBuild) {
					Debug.Log("release applymod is " + file);
					project.ApplyMod( file );
				}
			} else {
				Debug.Log("applymod is " + file);
				project.ApplyMod( file );
			}
		}

		project.overwriteBuildSetting("GCC_ENABLE_CPP_RTTI", "YES");
		//if OC exceptions is disabled, sensor iOS compile will fail
		project.overwriteBuildSetting("GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

		SetProvisioningStyle(project);

		//todo: should enable Bitcode for release?
		project.overwriteBuildSetting("ENABLE_BITCODE", "NO");

		project.overwriteBuildSetting("CODE_SIGN_IDENTITY", "iPhone Developer", "Debug");
		project.overwriteBuildSetting("CODE_SIGN_IDENTITY", "iPhone Developer", "ReleaseForProfiling");
		project.overwriteBuildSetting("CODE_SIGN_IDENTITY", "iPhone Developer", "ReleaseForRunning");

        BasePackageConfig packageConfig = PackageConfigManager.Instance.GetiOSBuildConfig(GetIsIOSIWBuild());
        project.overwriteBuildSetting("CODE_SIGN_IDENTITY", packageConfig.DistributionCertification, "Release");
	    
		string provisionDev = "";
        provisionDev = packageConfig.iOSProvisionDev;

        project.overwriteBuildSetting("PROVISIONING_PROFILE", provisionDev, "Debug");
		project.overwriteBuildSetting("PROVISIONING_PROFILE", provisionDev, "ReleaseForProfiling");
		project.overwriteBuildSetting("PROVISIONING_PROFILE", provisionDev, "ReleaseForRunning");

		//AdHoc
		//project.overwriteBuildSetting("PROVISIONING_PROFILE", "ea67fd45-afa7-4314-87a6-d7294f32bd1a", "Release");
		//AppStore
		//project.overwriteBuildSetting("PROVISIONING_PROFILE", "5488674a-c55f-4c7f-8538-f44cae39cb06", "Release");

		string releaseProvisionFile = "";
		bool isAppStore = GetIsAppBuildSymbol("Release_AppStore");
		if(isAppStore)
		{
            releaseProvisionFile = packageConfig.iOSProvisionAppstore;
        }
		else
		{
            releaseProvisionFile = packageConfig.iOSProvisionAdhoc;
		}
		project.overwriteBuildSetting("PROVISIONING_PROFILE", releaseProvisionFile, "Release");

        //		project.overwriteBuildSetting("PROVISIONING_PROFILE_SPECIFIDER", "MJ42U9V8CF/20170119_trojan_dev", "Debug");
        //		project.overwriteBuildSetting("PROVISIONING_PROFILE_SPECIFIDER", "MJ42U9V8CF/20170119_trojan_dev", "ReleaseForProfiling");
        //		project.overwriteBuildSetting("PROVISIONING_PROFILE_SPECIFIDER", "MJ42U9V8CF/20170119_trojan_dev", "ReleaseForRunning");
        //		project.overwriteBuildSetting("PROVISIONING_PROFILE_SPECIFIDER", "MJ42U9V8CF/20170120_trojan_appstore", "Release");

        project.overwriteBuildSetting("DEVELOPMENT_TEAM", packageConfig.CertificationId);
        project.overwriteBuildSetting("CLANG_ENABLE_MODULES", "YES");

//		project.overwriteBuildSetting ("VALID_ARCHS", "arm64 armv7s");

//		project.overwriteBuildSetting ("GCC_GENERATE_DEBUGGING_SYMBOLS", "NO");

		//keychain setting
		project.overwriteBuildSetting ("CODE_SIGN_ENTITLEMENTS", "KeychainAccessGroups.plist");

	    string keychainSrcPath = "";
	    string keychainDestPath = path + "/KeychainAccessGroups.plist";
	    if (isDebugBuild)
	        keychainSrcPath = packageConfig.KeychainPlistDebugPath;
		else if (isReleaseBuild)
	        keychainSrcPath = packageConfig.KeychainPlistReleasePath;

            FileCopy(path + keychainSrcPath, keychainDestPath);

//		SetKeychain(project);
		SetPushEnable(project);

		EditorPlist(path, packageConfig);
		EditorCode(path);

		CopySplashImages(path, packageConfig);
		CopyIcons(path, packageConfig);

		//AddIcons(pathToBuiltProject);
		
		// Finally save the xcode project
		project.Save();
		UnityEngine.Debug.Log("XCodePostProcess Sucess");
	}

	private static void SetPushEnable(XCProject project){
		var pbxproj = project.project;
		var attrs = pbxproj.attributes;
		var targetAttrs = (PBXDictionary)attrs["TargetAttributes"];

		var targets = pbxproj.targets;
		foreach (var t in targets)
		{
			var targetID = (string)t;
			if (targetAttrs.ContainsKey (targetID))
			{
				var TargetAttr = (PBXDictionary)targetAttrs [targetID];
				if (!TargetAttr.ContainsKey ("SystemCapabilities")) {
					TargetAttr ["SystemCapabilities"] = new PBXDictionary ();
				}
				PBXDictionary dict = (PBXDictionary)TargetAttr["SystemCapabilities"];
				dict["com.apple.Push"] = new PBXDictionary ();
				PBXDictionary dict2 = (PBXDictionary)dict ["com.apple.Push"];
				dict2 ["enabled"] = 1;

				dict["com.apple.BackgroundModes"] = new PBXDictionary ();
				dict2 = (PBXDictionary)dict ["com.apple.BackgroundModes"];
				dict2 ["enabled"] = 1;
			}
		}
	}


	private static void SetKeychain(XCProject project){
		var pbxproj = project.project;
		var attrs = pbxproj.attributes;
		var targetAttrs = (PBXDictionary)attrs["TargetAttributes"];

		var targets = pbxproj.targets;
		foreach (var t in targets)
		{
			var targetID = (string)t;
			if (targetAttrs.ContainsKey (targetID))
			{
				var TargetAttr = (PBXDictionary)targetAttrs [targetID];
				if (!TargetAttr.ContainsKey ("SystemCapabilities")) {
					TargetAttr ["SystemCapabilities"] = new PBXDictionary ();
				}
				PBXDictionary dict = (PBXDictionary)TargetAttr["SystemCapabilities"];
				dict["com.apple.Keychain"] = new PBXDictionary ();
				PBXDictionary dict2 = (PBXDictionary)dict ["com.apple.Keychain"];
				dict2 ["enabled"] = 1;
			}
		}
	}

	private static void SetProvisioningStyle(XCProject project)
	{
		//Important: set ProvisioningStyle to Manual
		//Code is based on the reference and I made some simplifications
		//http://blog.icodeten.com/game/2016/12/19/unity-xcode8/

		var pbxproj = project.project;
		var attrs = pbxproj.attributes;
		var targetAttrs = (PBXDictionary)attrs["TargetAttributes"];

		var targets = pbxproj.targets;
		foreach (var t in targets)
		{
			var targetID = (string)t;
			if (targetAttrs.ContainsKey (targetID))
			{
				var TargetAttr = (PBXDictionary)targetAttrs [targetID];
				TargetAttr["ProvisioningStyle"] = "Manual";
			}
		}
	}

	private static void EditorPlist(string filePath, BasePackageConfig config)
	{
		XCPlistCustom list = new XCPlistCustom(filePath);
        string applovinSdkKey = config.AppLovinSdkKey;
		string fbAppId = config.FacebookAppId;

        string plistAdd = @"
			<key>CFBundleURLTypes</key>
			<array>
				<dict>
					<key>CFBundleURLName</key>
					<string>HugeWinSlots</string>
					<key>CFBundleURLSchemes</key>
					<array>
						<string>hugewinslots</string>
					</array>
				</dict>
				<dict>
					<key>CFBundleURLName</key>
					<string>facebook-unity-sdk</string>
					<key>CFBundleURLSchemes</key>
					<array>
						<string>fb" + fbAppId + @"</string>
					</array>
				</dict>
			</array>
			<key>AppLovinSdkKey</key>
			<string>"+ applovinSdkKey + @"</string>
			<key>ITSAppUsesNonExemptEncryption</key>
			<false/>
			<key>UIBackgroundModes</key>
			<array>
			<string>remote-notification</string>
			</array>";

		list.AddKey(plistAdd);
		list.Save();
	}

	private static void FileCopy(string srcFile, string destPath){
		Debug.Log ("FileCopy src " + srcFile + " dest " + destPath);
		FileInfo file = new FileInfo(srcFile);
		if (file.Exists) {
			FileInfo f = file.CopyTo (destPath, true);
			if (f.Exists) {
				Debug.Log ("file create success");
			} else {
				Debug.Log ("file create failed Dest = " + destPath);
			}
		} else {
			Debug.Log ("file not exist src = "+srcFile);
		}
	}

	private static void EditorCode(string filePath)
	{
		XClass UnityAppController = new XClass(filePath + "/Classes/UnityAppController.mm");

		UnityAppController.WriteBelow("::printf(\"WARNING -> applicationDidReceiveMemoryWarning()\\n\");",  "UnitySendMessage(\"MemoryManager\",\"MemoryWarning\", \"\");");
	}

	static bool GetIsAppBuildSymbol(string build){
		bool result = false;
		string path = Path.Combine(Application.dataPath, "../buildSymbol.txt");
		Debug.Log("GetIsAppBuild path: " + path + " buildSymbol =" + build);
		if (File.Exists (path)) {
			string content = File.ReadAllText (path);
			Debug.Log ("Read content: " + content);
			if (content.Contains (build))
				result = true;
		}
		return result;
	}

	static bool GetIsIOSIWBuild()
	{
		bool result = false;
	#if UNITY_IOS
		string path = Path.Combine(Application.dataPath, "../buildChannel.txt");
		if(File.Exists(path))
		{
			string content = File.ReadAllText(path);
			Debug.Log("Read buildChannel.txt content:" + content);
			if(content.Contains(ChannelType.iOS_IW.ToString()))
				result = true;
		}
	#endif
		return result;
	}

	static void CopySplashImages(string path, BasePackageConfig config)
	{
        string srcDir = path + config.SplashIconsPath;
		string destDir = path + "/Unity-iPhone/Images.xcassets/LaunchImage.launchimage/";
		string[] splashNames = new string[] {
			//landscape
			"Default-Landscape.png",
			"Default-Landscape@2x.png",
			"Default-Landscape@3x.png",
			//portrait
			"Default-568h@2x.png",
			"Default-667h@2x.png",
			"Default-Portrait.png",
			"Default-Portrait@2x.png",
			"Default-Portrait@3x.png",
			"Default.png",
			"Default@2x.png"
		};

		CopyImages(srcDir, destDir, splashNames);

		string destDir2 = path + "/";
		string[] screenNames = new string[] {
			//screen
			"LaunchScreen-iPad.png",
			"LaunchScreen-iPhoneLandscape.png",
			"LaunchScreen-iPhonePortrait.png"
		};

		CopyImages(srcDir, destDir2, screenNames);
	}

	static void CopyIcons(string path, BasePackageConfig config)
	{
        string srcDir = path + config.AppIconsPath;
		string destDir = path + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/";

		string[] iconNames = new string[] {
			"Icon-72.png",
			"Icon-76.png",
			"Icon-120.png",
			"Icon-144.png",
			"Icon-152.png",
			"Icon-167.png",
			"Icon-180.png",
			"Icon-Small-40.png",
			"Icon-Small-80.png",
			"Icon-Small-120.png",
			"Icon-Small.png",
			"Icon-Small@2x.png",
			"Icon-Small@3x.png",
			"Icon.png",
			"Icon@2x.png",
			"Icon-1024.jpg",
			"Contents.json"
		};

		CopyImages(srcDir, destDir, iconNames);
	}

    static void CopyImages(string srcDir, string destDir, string[] fileNames)
	{
		foreach(string name in fileNames)
		{
			string src = srcDir + name;
			string dest = destDir + name;
			FileCopy(src, dest);
		}
	}

//	public static void AddIcons(string buildPath)
//	{
////		FileUtil.CopyFileOrDirectory("PATH_TO_YOUR_ICONS/Icon-167.png", buildPath + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/Icon-167.png");
//
//		string xcodeAppIconContentsFile = buildPath + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset/Contents.json";
//		string xcodeAppIconContentsText = null;
//		try
//		{
//			xcodeAppIconContentsText = File.ReadAllText(xcodeAppIconContentsFile);
//		}
//		catch (System.Exception exception)
//		{
//			System.Console.WriteLine(string.Format("Failed to read app icon contents file at: {0} with error <{1}>", xcodeAppIconContentsFile, exception.Message));
//		}
//
//		string newString = xcodeAppIconContentsText.Replace("]", ", {\"filename\" : \"Icon-167.png\", \"idiom\" : \"ipad\", \"scale\" : \"1x\", \"size\" : \"167x167\"} ]");
//		try
//		{
//			File.WriteAllText(xcodeAppIconContentsFile, newString);
//		}
//		catch (System.Exception exception)
//		{
//			System.Console.WriteLine(string.Format("Failed to write app icon contents file at: {0} with error <{1}>", xcodeAppIconContentsFile, exception.Message));
//		}
//	}

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
#endif
    }

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

public class BuildAssetBundles
{
	//The same as in LiveUpdateConfig
	private static string _bundleInfoFileName = "BundleInfo.txt";
	private static string _assetMapFileName = "AssetMap.txt";
	private static string _resourceInfoFileName = "resourceinfo";

	public static bool BuildBundlesFromEditor(string path, string resourceVersion, PlatformType platformType, BuildTarget target, bool isBuildFileTable)
	{
		string output = Path.Combine("Assets", path);
		DirectoryInfo sourceDic = new DirectoryInfo(output);
		sourceDic.Create();

		AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(output, BuildAssetBundleOptions.None, target);
		if(manifest == null)
			Debug.LogError("BuildBundlesFromEditor fail");

		if(isBuildFileTable)
			BuildFileTable(manifest, path, resourceVersion, platformType, target);

		return manifest != null;
	}

	public static bool BuildBundlesFromMap(string path, string resourceVersion, PlatformType platformType, BuildTarget target, bool isBuildFileTable, 
		string bundleName, IList<string> assetNames)
	{
		bool result = false;
		string output = Path.Combine("Assets", path);
		DirectoryInfo sourceDic = new DirectoryInfo(output);
		sourceDic.Create();

		AssetBundleBuild[] assetMap = new AssetBundleBuild[1];
		assetMap[0].assetBundleName = bundleName;
		assetMap[0].assetNames = new string[assetNames.Count];
		assetNames.CopyTo(assetMap[0].assetNames, 0);

		AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(output, assetMap, BuildAssetBundleOptions.None, target);
		result = manifest != null;
		if(manifest == null)
			Debug.Log("Warn: BuildBundlesFromMap fail: " + bundleName);
		
		if(result && isBuildFileTable)
			BuildFileTable(manifest, path, resourceVersion, platformType, target);

		return result;
	}

    public struct BundleInfo
    {
		public string Hash;
        public BundleInfo(string h)
        {
            Hash = h;
        }
    }

	static void BuildFileTable(AssetBundleManifest manifest, string bundleRoot, string resourceVersion, PlatformType platformType, BuildTarget target)
	{
		Dictionary<string, string> assetTable = new Dictionary<string, string>();

		string[] bundles = manifest.GetAllAssetBundles();
		string bundleTablePath = Path.Combine("Assets", Path.Combine(bundleRoot, _bundleInfoFileName));
		FileInfo bundleTableFile = new FileInfo(bundleTablePath);
		Dictionary<string, BundleInfo> finalVersionDic = new Dictionary<string, BundleInfo>();

		foreach(var bundlePath in bundles)
		{
			string path = Path.Combine(bundleRoot, bundlePath);
			path = Path.Combine(Application.dataPath, path);
			var bytes = File.ReadAllBytes(path);
			var assetBundle = AssetBundle.LoadFromMemory(bytes);

			// log bundle info
			if(finalVersionDic.ContainsKey(bundlePath))
			{
				if(finalVersionDic[bundlePath].Hash != manifest.GetAssetBundleHash(bundlePath).ToString())
				{
					var newInfo = new BundleInfo(manifest.GetAssetBundleHash(bundlePath).ToString());
					finalVersionDic[bundlePath] = newInfo;

					Debug.Log("Bundle " + bundlePath);
				}
			}
			else
			{
				var newInfo = new BundleInfo(manifest.GetAssetBundleHash(bundlePath).ToString());

				finalVersionDic.Add(bundlePath, newInfo);

				Debug.Log("Bundle " + bundlePath);

			}
			var assetPaths = assetBundle.GetAllAssetNames();
			foreach(var ap in assetPaths)
			{
				var subAssetPath = ap;
				subAssetPath = subAssetPath.Split('.')[0];

				var subBundlePath = bundlePath.Split('.')[0];
				if(!assetTable.ContainsKey(subAssetPath))
					assetTable.Add(subAssetPath, subBundlePath);
			}

			assetBundle.Unload(true);
		}

		//1 BundleInfo.txt
		using(var writer = File.CreateText(bundleTableFile.FullName))
		{
			writer.WriteLine(resourceVersion);
			foreach(var line in finalVersionDic)
			{
				writer.WriteLine(line.Key + "," + line.Value.Hash);
			}
			writer.Close();
		}

		//2 AssetMap.txt
		var assetTablePath = Path.Combine("Assets", Path.Combine(bundleRoot, _assetMapFileName));
		FileInfo assetTableFile = new FileInfo(assetTablePath);
		using(var writer = File.CreateText(assetTablePath))
		{
			foreach(var pair in assetTable)
			{
				string s = string.Format("{0},{1}", pair.Key, pair.Value);
				writer.WriteLine(s);
			}
			writer.Close();
		}

		//Critical fix:
		//On official document: AssetDatabase.Refresh: Import any changed assets
		//Make Unity recognize the existence of generated BundleInfo.txt and AssetMap.txt
		//Otherwise, the resourceinfo generation would fail for the first time while work for the second time
		AssetDatabase.Refresh();

		//3 ResourceInfo
		var assetMap = new AssetBundleBuild[1];
		assetMap[0].assetBundleName = _resourceInfoFileName;
		assetMap[0].assetNames = new string[] {bundleTablePath, assetTablePath};

		//pack the bundleTablePath and assetTablePath to compress its size
		AssetBundleManifest resourceInfoManifest = BuildPipeline.BuildAssetBundles(assetTableFile.Directory.FullName, assetMap, BuildAssetBundleOptions.None, target);
		Debug.Log("Build resourceInfo result: " + (resourceInfoManifest != null).ToString());

		//4 make zip
		if(resourceInfoManifest != null)
			MakeZip(resourceVersion, platformType, bundles, _resourceInfoFileName, bundleRoot);
	}

	static void MakeZip(string resourceVersion, PlatformType platformType, string[] bundleNames, string resourceInfoName, string bundleRoot)
	{
		string platformStr = platformType.ToString();
		string bundlePath = Path.Combine(Application.dataPath, bundleRoot);

		using (ZipFile zip = new ZipFile())
		{
			foreach(string bundleName in bundleNames)
			{
				string p = Path.Combine(bundlePath, bundleName);
				zip.AddFile(p, platformStr);
			}

			string resourceInfoPath = Path.Combine(bundlePath, resourceInfoName);
			zip.AddFile(resourceInfoPath, platformStr);

			string zipFileName = platformStr + "_" + resourceVersion + ".zip";
			string zipPath = Path.Combine(bundlePath, "../Output/");

			DirectoryInfo dirInfo = new DirectoryInfo(zipPath);
			dirInfo.Create();
			
			zipPath = Path.Combine(zipPath, zipFileName);
			zip.Save(zipPath);
		}

		Debug.Log("Make zip done");
	}
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MultiLineExportEditorWindow : EditorWindow
{
	private static readonly string _configFilePath = "Assets/Editor/MultiLineExport/config.asset";
	private static readonly string _exportDataFilePath = "Assets/Editor/MultiLineExport/exportData.csv";

	private static MultiLineExportConfig _config;
	private static MultiLineExportEditorWindow _window;

	[MenuItem("Tools/MultiLine Export")]
	static void InitWindow()
	{
		InitConfig();
		ShowWindow();
	}

	static void InitConfig()
	{
		_config = AssetDatabase.LoadAssetAtPath<MultiLineExportConfig>(_configFilePath);
		if(_config == null)
		{
			_config = new MultiLineExportConfig();
			AssetDatabase.CreateAsset(_config, _configFilePath);
		}
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 600);
		_window = (MultiLineExportEditorWindow)EditorWindow.GetWindowWithRect(typeof(MultiLineExportEditorWindow), rect, true);
		_window.Show();
	}

	void OnGUI()
	{
		_config._machineName = EditorGUILayout.TextField("MachineName", _config._machineName);

		GUILayout.Space(10);

		if(GUILayout.Button("Save config", GUILayout.Width(160.0f)))
			SaveConfigButtonDown();

		if(GUILayout.Button("Export data", GUILayout.Width(160.0f)))
			RunButtonDown();
	}

	void SaveConfigButtonDown()
	{
		EditorUtility.SetDirty(_config);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void RunButtonDown()
	{
		bool result = ExportData();
		string str = result ? "OK" : "Fail";
		ShowNotification(new GUIContent(str));
	}

	bool ExportData()
	{
		MachineConfig machineConfig = new MachineConfig(_config._machineName);

		if(!machineConfig.BasicConfig.IsMultiLine)
			return false;

		CoreMultiLineChecker checker = new CoreMultiLineChecker(machineConfig);

		int symbolCount0 = machineConfig.ReelConfig.GetSingleReel(0).SymbolCount;
		int symbolCount1 = machineConfig.ReelConfig.GetSingleReel(1).SymbolCount;
		int symbolCount2 = machineConfig.ReelConfig.GetSingleReel(2).SymbolCount;
//		int symbolCount0 = 4;
//		int symbolCount1 = 4;
//		int symbolCount2 = 4;
		int total = symbolCount0 * symbolCount1 * symbolCount2;
		List<MultiLineExportData> exportDataList = new List<MultiLineExportData>(total);

		for(int i = 0; i < total; i++)
		{
			int index0 = i % symbolCount0;
			int r = i / symbolCount0;
			int index1 = r % symbolCount1;
			int index2 = r / symbolCount1;

			int[] indexes = new int[]{ index0, index1, index2 };
			CoreMultiLineCheckResult checkResult = checker.CheckResultWithStopIndexes(indexes);

//			#if DEBUG
//			string s = string.Format("indexes:[{0}, {1}, {2}]", index0, index1, index2);
//			Debug.Log(s);
//			checkResult.DebugPrint();
//			#endif

			MultiLineExportData exportData = new MultiLineExportData(checkResult.PayoutReward, checkResult.NearHitReward, indexes);
			exportDataList.Add(exportData);
		}

		PrintData(exportDataList);

		return true;
	}

	void PrintData(List<MultiLineExportData> dataList)
	{
		if(File.Exists(_exportDataFilePath))
			File.Delete(_exportDataFilePath);
		
		StreamWriter writer = FileStreamUtility.CreateFileStream(_exportDataFilePath);

		FileStreamUtility.WriteFile(writer, "PayoutReward,NearHitReward,Reel1,Reel2,Reel3");

		for(int i = 0; i < dataList.Count; i++)
		{
			MultiLineExportData data = dataList[i];
			string s = string.Format("{0},{1},{2},{3},{4}", data._payoutReward, data._nearHitReward, 
				data._stopIndexes[0], data._stopIndexes[1], data._stopIndexes[2]);

			FileStreamUtility.WriteFile(writer, s);
		}

		FileStreamUtility.CloseFile(writer);
	}
}

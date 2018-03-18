///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachineEditor.cs
///
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace UnityQuickSheet
{
    /// <summary>
    /// Custom editor script class for excel file setting.
    /// </summary>
    [CustomEditor(typeof(ExcelMachine))]
    public class ExcelMachineEditor : BaseMachineEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            machine = target as ExcelMachine;
            if (machine != null)
            {
                if (string.IsNullOrEmpty(ExcelSettings.Instance.RuntimePath) == false)
                    machine.RuntimeClassPath = ExcelSettings.Instance.RuntimePath;
                if (string.IsNullOrEmpty(ExcelSettings.Instance.EditorPath) == false)
                    machine.EditorClassPath = ExcelSettings.Instance.EditorPath;
            }
        }

		private string OpenExcelFilePanel(string initPath)
		{
			string folder = Path.GetDirectoryName(initPath);

			#if UNITY_EDITOR_WIN
			string path = EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
			#else // for UNITY_EDITOR_OSX
			string path = EditorUtility.OpenFilePanel("Open Excel file", folder, "xls");
			#endif

			return path;
		}

		private string TrimToRelativePath(string path)
		{
			string result = "";

			// the path should be relative not absolute one to make it work on any platform.
			int index = path.IndexOf("Assets");
			if(index >= 0)
				result = path.Substring(index);

			return result;
		}

		private void ImportExcelFile(string filePath)
		{
			AssetDatabase.ImportAsset(filePath);
		}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExcelMachine machine = target as ExcelMachine;

            GUILayout.Label("Excel Spreadsheet Settings:", headerStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Template Excel:", GUILayout.Width(90));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

            string path = string.Empty;
            if (string.IsNullOrEmpty(machine.excelFilePath))
                path = Application.dataPath;
            else
                path = machine.excelFilePath;

			machine.excelFilePath = GUILayout.TextField(path, GUILayout.Width(250));
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
				path = OpenExcelFilePanel(path);

                if (path.Length != 0)
                {
					// the path should be relative not absolute one to make it work on any platform.
					string trimPath = TrimToRelativePath(path);
					if(!string.IsNullOrEmpty(trimPath))
					{
						// set relative path
						machine.excelFilePath = trimPath;

						// pass absolute path
						machine.SheetNames = new ExcelQuery(path).GetSheetNames();
					}
					else
					{
						EditorUtility.DisplayDialog("Error",
							@"Wrong folder is selected.
                        Set a folder under the 'Assets' folder! \n
                        The excel file should be anywhere under  the 'Assets' folder", "OK");
						return;
					}
                }
            }

            GUILayout.EndHorizontal();

            // Failed to get sheet name so we just return not to make editor on going.
            if (machine.SheetNames.Length == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Error: Failed to retrieve the specified excel file.");
                EditorGUILayout.LabelField("If the excel file is opened, close it then reopen it again.");
                return;
            }

			// begin: add excel export path
			GUILayout.BeginHorizontal();
			GUILayout.Label("Export Folder:", GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			string exportPath = string.Empty;
			if (string.IsNullOrEmpty(machine.exportFilePath))
				exportPath = Application.dataPath;
			else
				exportPath = machine.exportFilePath;
			
			machine.exportFilePath = GUILayout.TextField(exportPath, GUILayout.Width(250));

			if (GUILayout.Button("...", GUILayout.Width(20)))
			{
				string folder = Path.GetDirectoryName(exportPath);
#if UNITY_EDITOR_WIN
				exportPath = EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
#else // for UNITY_EDITOR_OSX
				exportPath = EditorUtility.OpenFolderPanel("Open Excel file", folder, "xls");
#endif
				if (exportPath.Length != 0)
				{
					// the path should be relative not absolute one to make it work on any platform.
					int index = exportPath.IndexOf("Assets");
					if (index >= 0)
					{
						// set relative path
						machine.exportFilePath = exportPath.Substring(index);
					}
					else
					{
						EditorUtility.DisplayDialog("Error",
							@"Wrong folder is selected.
                        Set a folder under the 'Assets' folder! \n
                        The excel file should be anywhere under  the 'Assets' folder", "OK");
						return;
					}
				}
			}

			GUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			// end: add excel export path

            EditorGUILayout.Space();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Worksheet: ", GUILayout.Width(100));
                machine.CurrentSheetIndex = EditorGUILayout.Popup(machine.CurrentSheetIndex, machine.SheetNames);
                if (machine.SheetNames != null)
                    machine.WorkSheetName = machine.SheetNames[machine.CurrentSheetIndex];

                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    // reopen the excel file e.g) new worksheet is added so need to reopen.
                    machine.SheetNames = new ExcelQuery(machine.excelFilePath).GetSheetNames();

                    // one of worksheet was removed, so reset the selected worksheet index
                    // to prevent the index out of range error.
                    if (machine.SheetNames.Length <= machine.CurrentSheetIndex)
                    {
                        machine.CurrentSheetIndex = 0;

                        string message = "Worksheet was changed. Check the 'Worksheet' and 'Update' it again if it is necessary.";
                        EditorUtility.DisplayDialog("Info", message, "OK");
                    }
                }
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();

			if (machine.HasColumnHeader(machine.WorkSheetName))
            {
                if (GUILayout.Button("Update"))
                    Import();
                if (GUILayout.Button("Reimport"))
                    Import(true);
            }
            else
            {
                if (GUILayout.Button("Import"))
                    Import();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

			ColumnHeaderList curSheetConfig = machine.GetCurrentWorkSheetConfig();
			curSheetConfig.IsReuseOtherSheet = EditorGUILayout.Toggle("Is Reuse Other Sheet", curSheetConfig.IsReuseOtherSheet);

			if(curSheetConfig.IsReuseOtherSheet)
				curSheetConfig.ReuseSheetName = EditorGUILayout.TextField("Reuse Sheet Name: ", curSheetConfig.ReuseSheetName);

			EditorGUILayout.Separator();

            DrawHeaderSetting(machine);

			EditorGUILayout.Separator();

			curSheetConfig.onlyCreatePostProcessClass = EditorGUILayout.Toggle("Only Gen PostProcessor", curSheetConfig.onlyCreatePostProcessClass);

			//
			//Generate button
			//
			if (GUILayout.Button("Generate Scripts"))
			{
				ScriptPrescription sp = Generate(machine);
				if (sp != null)
				{
					Debug.Log("Successfully generated!");
				}
				else
					Debug.LogError("Failed to create a script from excel.");
			}

            EditorGUILayout.Separator();

			GUILayout.BeginHorizontal();

			//
			//Import Excels
			//
			GUILayout.Label("Import Excels:", headerStyle);

			GUILayout.EndHorizontal();

			for(int i = 0; i < machine.importFileList.Count; i++)
			{
				GUILayout.BeginHorizontal();

				string importFile = machine.importFileList[i];
				
				importFile = GUILayout.TextField(importFile, GUILayout.Width(200));

				if(GUILayout.Button("...", GUILayout.Width(20)))
				{
					if(string.IsNullOrEmpty(importFile))
						importFile = Application.dataPath;
					
					path = OpenExcelFilePanel(importFile);

					if (path.Length != 0)
					{
						// the path should be relative not absolute one to make it work on any platform.
						string trimPath = TrimToRelativePath(path);
						if(!string.IsNullOrEmpty(trimPath))
						{
							// set relative path
							machine.importFileList[i] = trimPath;
						}
						else
						{
							EditorUtility.DisplayDialog("Error",
								@"Wrong folder is selected.
                        Set a folder under the 'Assets' folder! \n
                        The excel file should be anywhere under  the 'Assets' folder", "OK");
							return;
						}
					}
				}

				if(GUILayout.Button("-", GUILayout.Width(20)))
				{
					machine.importFileList.RemoveAt(i);
				}

				if(GUILayout.Button("+", GUILayout.Width(20)))
				{
					machine.importFileList.Insert(i + 1, "");
				}

				if(GUILayout.Button("Export", GUILayout.Width(48)))
				{
					ImportExcelFile(machine.importFileList[i]);
				}

				GUILayout.EndHorizontal();
			}

			if(GUILayout.Button("Export All", GUILayout.Width(80)))
			{
				foreach(string filePath in machine.importFileList)
					ImportExcelFile(filePath);
			}

			EditorGUILayout.Separator();

            GUILayout.Label("Path Settings:", headerStyle);

            machine.TemplatePath = EditorGUILayout.TextField("Template: ", machine.TemplatePath);
            machine.RuntimeClassPath = EditorGUILayout.TextField("Runtime: ", machine.RuntimeClassPath);
            machine.EditorClassPath = EditorGUILayout.TextField("Editor:", machine.EditorClassPath);
            //machine.DataFilePath = EditorGUILayout.TextField("Data:", machine.DataFilePath);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(machine);
                //AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Import the specified excel file and prepare to set type of each cell.
        /// </summary>
        protected override void Import(bool reimport = false)
        {
            ExcelMachine machine = target as ExcelMachine;

            string path = machine.excelFilePath;
            string sheet = machine.WorkSheetName;

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "You should specify spreadsheet file first!",
                    "OK"
                );
                return;
            }

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "File at " + path + " does not exist.",
                    "OK"
                );
                return;
            }

            int startRowIndex = 0;
            string error = string.Empty;
            var titles = new ExcelQuery(path, sheet).GetTitle(startRowIndex, ref error);
            if (titles == null || !string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Error", error, "OK");
                return;
            }
            else
            {
                // check the column header is valid
                foreach(string column in titles)
                {
                    if (!IsValidHeader(column))
                    {
                        error = string.Format(@"Invalid column header name {0}. Any c# keyword should not be used for column header. Note it is not case sensitive.", column);
                        EditorUtility.DisplayDialog("Error", error, "OK");
                        return;
                    }
                }
            }

            List<string> titleList = titles.ToList();

			if(!machine.ColumnHeaderListDict.ContainsKey(sheet))
				machine.ColumnHeaderListDict.SetKeyValue(sheet, new ColumnHeaderList());

			List<ColumnHeader> curColumnHeaderList = machine.ColumnHeaderListDict.GetValue(sheet).list;

			if (machine.HasColumnHeader(sheet) && reimport == false)
            {
				var headerDic = curColumnHeaderList.ToDictionary(header => header.name);

                // collect non-changed column headers
                var exist = from t in titleList
                            where headerDic.ContainsKey(t) == true
                            select new ColumnHeader { name = t, type = headerDic[t].type, isArray = headerDic[t].isArray, OrderNO = headerDic[t].OrderNO };

                // collect newly added or changed column headers
                var changed = from t in titleList
                              where headerDic.ContainsKey(t) == false
                              select new ColumnHeader { name = t, type = CellType.Undefined, OrderNO = titleList.IndexOf(t) };

                // merge two list via LINQ
                var merged = exist.Union(changed).OrderBy(x => x.OrderNO);

				curColumnHeaderList.Clear();
				curColumnHeaderList = merged.ToList();
            }
            else
            {
				curColumnHeaderList.Clear();

                if (titles != null)
                {
                    int i = 0;
                    foreach (string s in titles)
                    {
						curColumnHeaderList.Add(new ColumnHeader { name = s, type = CellType.Undefined, OrderNO = i });
                        i++;
                    }
                }
                else
                {
                    Debug.LogWarning("The WorkSheet [" + sheet + "] may be empty.");
                }
            }

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Generate AssetPostprocessor editor script file.
        /// </summary>
        protected override void CreateAssetCreationScript(BaseMachine m, ScriptPrescription sp)
        {
            ExcelMachine machine = target as ExcelMachine;

			sp.workSheetName = machine.WorkSheetName;
			sp.dataClassName = machine.GetComposedDataClassName();
			sp.workSheetClassName = machine.GetComposedWorkSheetClassName();

            // where the imported excel file is.
			sp.importedFilePath = Path.GetDirectoryName(machine.excelFilePath);

            // path where the .asset file will be created.
//            string path = Path.GetDirectoryName(machine.excelFilePath);
//            path += "/" + machine.WorkSheetName + ".asset";
			sp.assetFilepath = TrimDirPrefix(machine.exportFilePath, "Assets");
            sp.assetPostprocessorClass = machine.WorkSheetName + "AssetPostprocessor";
            sp.template = GetTemplate("PostProcessor");

            // write a script to the given folder.
            using (var writer = new StreamWriter(TargetPathForAssetPostProcessorFile(machine.WorkSheetName)))
            {
                writer.Write(new ScriptGenerator(sp).ToString());
                writer.Close();
            }
        }

		private string TrimDirPrefix(string path, string prefix)
		{
			string result = path;
			int index = path.IndexOf(prefix);
			if (index >= 0)
			{
				// set relative path
				result = path.Substring(index);
			}
			else
			{
				EditorUtility.DisplayDialog("Error",
					@"TrimDirPrefix wrong, no prefix found", "OK");
			}
			return result;
		}
    }
}
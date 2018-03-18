using System.Collections;
using System.Collections.Generic;

public class CoreConfig : SimpleSingleton<CoreConfig>
{
	public static readonly string ExcelName = "CoreSetting";

	private LuckyConfig _luckyConfig;
	private MiscConfig _miscConfig;
	private JackpotSettingConfig _jackpotSettingConfig;

	public LuckyConfig LuckyConfig { get { return _luckyConfig; } }
	public MiscConfig MiscConfig { get { return _miscConfig; } }
	public JackpotSettingConfig JackpotSettingConfig { get { return _jackpotSettingConfig; } }

	public CoreConfig()
	{
		LoadRawData();

		//CoreDebugUtility.Log("CoreConfig constructor");
	}

	private void LoadRawData()
	{
		string path;

		LuckySheet luckySheet = CoreAssetManager.Instance.LoadExcelAsset<LuckySheet, LuckyData>(CoreConfigTable.SubDir, ExcelName, LuckyConfig.Name);
		_luckyConfig = new LuckyConfig(luckySheet, this);

		MiscSheet miscSheet = CoreAssetManager.Instance.LoadExcelAsset<MiscSheet, MiscData>(CoreConfigTable.SubDir, ExcelName, MiscConfig.Name);
		_miscConfig = new MiscConfig(miscSheet, this);

		JackpotSettingSheet jackpotSettingSheet = CoreAssetManager.Instance.LoadExcelAsset<JackpotSettingSheet, JackpotSettingData>(CoreConfigTable.SubDir, ExcelName, JackpotSettingConfig.Name);
		_jackpotSettingConfig = new JackpotSettingConfig(jackpotSettingSheet, this);
	}

	public void Reload()
	{
		CoreDebugUtility.Log("Reload CoreConfig");
		LoadRawData();
	}

	public bool IsCoreSettingAsset(string assetName)
	{
		bool result = assetName.StartsWith(ExcelName.ToLower());
		return result;
	}
}

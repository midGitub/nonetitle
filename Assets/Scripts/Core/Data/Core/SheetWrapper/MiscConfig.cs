using System.Collections;
using System.Collections.Generic;

public class MiscConfig
{
	public static readonly string Name = "Misc";

	private MiscSheet _sheet;
	private CoreConfig _coreConfig;

	public MiscSheet Sheet { get { return _sheet; } }

	private int _bigWinThreshold;
	private int _epicWinThreshold;
	private int _normalWinHighThreshold;
	private int _initCredits;
	private int _scoredInStoreReward;
	private int _facebookLikesReward;
	private float _multiLineHypeThreshold;

	public int BigWinThreshold { get { return _bigWinThreshold; } }
	public int EpicWinThreshold { get { return _epicWinThreshold; } }
	public int NormalWinHighThreshold { get { return _normalWinHighThreshold; } }
	public int InitCredits { get { return _initCredits; } }
	public int ScoredInStoreReward { get { return _scoredInStoreReward; } }
	public int FacebookLikesReward { get { return _facebookLikesReward; } }
	public float MultiLineHypeThreshold { get { return _multiLineHypeThreshold; } }

	public MiscConfig(MiscSheet sheet, CoreConfig coreConfig)
	{
		_sheet = sheet;
		_coreConfig = coreConfig;

		Init();
	}

	private void Init()
	{
		_bigWinThreshold = GetIntValueFromKey("BigWinThreshold");
		_epicWinThreshold = GetIntValueFromKey("EpicWinThreshold");
		_normalWinHighThreshold = GetIntValueFromKey("NormalWinHighThreshold");
		_initCredits = GetIntValueFromKey("InitCredits");
		_scoredInStoreReward = GetIntValueFromKey ("ScoredInStoreReward");
		_facebookLikesReward = GetIntValueFromKey("FacebookLikesReward");
		_multiLineHypeThreshold = GetFloatValueFromKey("MultiLineHypeThreshold");
	}

	private int GetIntValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		CoreDebugUtility.Assert(!string.IsNullOrEmpty(str));
		return int.Parse(str);
	}

	private float GetFloatValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		CoreDebugUtility.Assert(!string.IsNullOrEmpty(str));
		return float.Parse(str);
	}

	private string ValueFromKey(string key)
	{
		string result = "";
		int index = ListUtility.Find(_sheet.dataArray, (MiscData data) => {
			return data.Key == key;
		});
		if(index >= 0)
			result = _sheet.dataArray[index].Val;
		return result;
	}
}

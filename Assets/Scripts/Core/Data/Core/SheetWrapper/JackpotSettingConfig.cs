using System.Collections;
using System.Collections.Generic;

public class JackpotSettingConfig
{
	public static readonly string Name = "JackpotSetting";

	private JackpotSettingSheet _sheet;
	private CoreConfig _coreConfig;

	float _pointIncreaseFactor;
	float _winLuckyFactor;
	float _winCalculateFactor;

	float _singleJackpotPointThreshold;
	float _fourJackpotColossalPointThreshold;
	float _fourJackpotMegaPointThreshold;
	float _fourJackpotHugePointThreshold;
	float _fourJackpotBigPointThreshold;

	float _singleJackpotPointInit;
	float _fourJackpotColossalPointInit;
	float _fourJackpotMegaPointInit;
	float _fourJackpotHugePointInit;
	float _fourJackpotBigPointInit;

	float _startWinFactor;

	public JackpotSettingSheet Sheet { get { return _sheet; } }

	public float PointIncreaseFactor { get { return _pointIncreaseFactor; } }
	public float WinLuckyFactor { get { return _winLuckyFactor; } }
	public float WinCalculateFactor { get { return _winCalculateFactor; } }

	public float SingleJackpotPointThreshold { get { return _singleJackpotPointThreshold; } }
	public float FourJackpotColossalPointThreshold { get { return _fourJackpotColossalPointThreshold; } }
	public float FourJackpotMegaPointThreshold { get { return _fourJackpotMegaPointThreshold; } }
	public float FourJackpotHugePointThreshold { get { return _fourJackpotHugePointThreshold; } }
	public float FourJackpotBigPointThreshold { get { return _fourJackpotBigPointThreshold; } }

	public float SingleJackpotPointInit { get { return _singleJackpotPointInit; } }
	public float FourJackpotColossalPointInit { get { return _fourJackpotColossalPointInit; } }
	public float FourJackpotMegaPointInit { get { return _fourJackpotMegaPointInit; } }
	public float FourJackpotHugePointInit { get { return _fourJackpotHugePointInit; } }
	public float FourJackpotBigPointInit { get { return _fourJackpotBigPointInit; } }

	public float StartWinFactor { get { return _startWinFactor; } }

	public JackpotSettingConfig(JackpotSettingSheet sheet, CoreConfig coreConfig)
	{
		_sheet = sheet;
		_coreConfig = coreConfig;

		Init();
	}

	private void Init()
	{
		_pointIncreaseFactor = GetFloatValueFromKey("PointIncreaseFactor");
		_winLuckyFactor = GetFloatValueFromKey("WinLuckyFactor");
		_winCalculateFactor = GetFloatValueFromKey("WinCalculateFactor");

		_singleJackpotPointThreshold = GetFloatValueFromKey("SingleJackpotPointThreshold");
		_fourJackpotColossalPointThreshold = GetFloatValueFromKey("FourJackpotColossalPointThreshold");
		_fourJackpotMegaPointThreshold = GetFloatValueFromKey("FourJackpotMegaPointThreshold");
		_fourJackpotHugePointThreshold = GetFloatValueFromKey("FourJackpotHugePointThreshold");
		_fourJackpotBigPointThreshold = GetFloatValueFromKey("FourJackpotBigPointThreshold");

		_singleJackpotPointInit = GetFloatValueFromKey("SingleJackpotPointInit");
		_fourJackpotColossalPointInit = GetFloatValueFromKey("FourJackpotColossalPointInit");
		_fourJackpotMegaPointInit = GetFloatValueFromKey("FourJackpotMegaPointInit");
		_fourJackpotHugePointInit = GetFloatValueFromKey("FourJackpotHugePointInit");
		_fourJackpotBigPointInit = GetFloatValueFromKey("FourJackpotBigPointInit");

		_startWinFactor = GetFloatValueFromKey("StartWinFactor");
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
		int index = ListUtility.Find(_sheet.dataArray, (JackpotSettingData data) => {
			return data.Key == key;
		});
		if(index >= 0)
			result = _sheet.dataArray[index].Val;
		return result;
	}
}


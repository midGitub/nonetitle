using System.Collections;
using System.Collections.Generic;

public class MachineConfig
{
	private static MachineConfig _instance;

	private string _name;
	private BasicConfig _basicConfig;
	private SymbolConfig _symbolConfig;
	private ReelConfig _reelConfig;
	private PayoutConfig _payoutConfig;
	private PayoutConfig _luckyPayoutConfig;
	private NearHitConfig _nearHitConfig;
	private NearHitConfig _luckyNearHitConfig;

	//only for multi payline
	private PayoutDistConfig _payoutDistConfig;
	private PayoutDistConfig _luckyPayoutDistConfig;
	private NearHitDistConfig _nearHitDistConfig;
	private NearHitDistConfig _luckyNearHitDistConfig;
	private PaylineConfig _paylineConfig;
	private RewardResultConfig _rewardResultConfig;

	// only for wheel
	private Dictionary<string, WheelConfig> _wheelConfigDict;
	private Dictionary<string, WheelConfig> _luckyWheelConfigDict;

	public string Name { get { return _name; } }
	public BasicConfig BasicConfig { get { return _basicConfig; } }
	public SymbolConfig SymbolConfig { get { return _symbolConfig; } }
	public ReelConfig ReelConfig { get { return _reelConfig; } }
	public PayoutConfig PayoutConfig { get { return _payoutConfig; } }
	public PayoutConfig LuckyPayoutConfig { get { return _luckyPayoutConfig; } }
	public NearHitConfig NearHitConfig { get { return _nearHitConfig; } }
	public NearHitConfig LuckyNearHitConfig { get { return _luckyNearHitConfig; } }

	//only for multi payline
	public PayoutDistConfig PayoutDistConfig { get { return _payoutDistConfig; } }
	public PayoutDistConfig LuckyPayoutDistConfig { get { return _luckyPayoutDistConfig; } }
	public NearHitDistConfig NearHitDistConfig { get { return _nearHitDistConfig; } }
	public NearHitDistConfig LuckyNearHitDistConfig { get { return _luckyNearHitDistConfig; } }
	public PaylineConfig PaylineConfig { get { return _paylineConfig; } }
	public RewardResultConfig RewardResultConfig { get { return _rewardResultConfig; } }

	// only for wheel
//	public Dictionary<string, WheelConfig> WheelConfigDict { get { return _wheelConfigDict; } }
//	public Dictionary<string, WheelConfig> LuckyWheelConfigDict { get { return _luckyWheelConfigDict; } }

	public static MachineConfig Instance
	{
		get { return _instance; }
		set { _instance = value; }
	}

	#region Public

	public MachineConfig(string name)
	{
		_name = name;

		LoadRawData();

		Instance = this;
	}

	//payout
	public PayoutConfig GetCurPayoutConfig(CoreLuckyMode mode)
	{
		PayoutConfig result = mode == CoreLuckyMode.Normal ? _payoutConfig : _luckyPayoutConfig;
		return result;
	}
	public PayoutConfig GetCurPayoutConfig(CoreLuckySheetMode mode)
	{
		PayoutConfig result = mode == CoreLuckySheetMode.Normal ? _payoutConfig : _luckyPayoutConfig;
		return result;
	}

	//nearHit
	public NearHitConfig GetCurNearHitConfig(CoreLuckyMode mode)
	{
		NearHitConfig result = mode == CoreLuckyMode.Normal ? _nearHitConfig : _luckyNearHitConfig;
		return result;
	}
	public NearHitConfig GetCurNearHitConfig(CoreLuckySheetMode mode)
	{
		NearHitConfig result = mode == CoreLuckySheetMode.Normal ? _nearHitConfig : _luckyNearHitConfig;
		return result;
	}

	//payoutDist
	public PayoutDistConfig GetCurPayoutDistConfig(CoreLuckyMode mode)
	{
		PayoutDistConfig result = mode == CoreLuckyMode.Normal ? _payoutDistConfig : _luckyPayoutDistConfig;
		return result;
	}
	public PayoutDistConfig GetCurPayoutDistConfig(CoreLuckySheetMode mode)
	{
		PayoutDistConfig result = mode == CoreLuckySheetMode.Normal ? _payoutDistConfig : _luckyPayoutDistConfig;
		return result;
	}

	//nearHitDist
	public NearHitDistConfig GetCurNearHitDistConfig(CoreLuckyMode mode)
	{
		NearHitDistConfig result = mode == CoreLuckyMode.Normal ? _nearHitDistConfig : _luckyNearHitDistConfig;
		return result;
	}
	public NearHitDistConfig GetCurNearHitDistConfig(CoreLuckySheetMode mode)
	{
		NearHitDistConfig result = mode == CoreLuckySheetMode.Normal ? _nearHitDistConfig : _luckyNearHitDistConfig;
		return result;
	}

	//wheel
	public WheelConfig GetCurWheelConfig(CoreLuckyMode mode, string wheelName)
	{
		Dictionary<string, WheelConfig> d = GetCurWheelConfigDict(mode);
		return d[wheelName];
	}

	public Dictionary<string, WheelConfig> GetCurWheelConfigDict(CoreLuckyMode mode)
	{
		Dictionary<string, WheelConfig> result = mode == CoreLuckyMode.Normal ? _wheelConfigDict : _luckyWheelConfigDict;
		return result;
	}

	#endregion

	#region Public

	private void LoadRawData()
	{
		CoreAssetManager assetManager = CoreAssetManager.Instance;

		BasicSheet basicSheet = assetManager.LoadExcelAsset<BasicSheet, BasicData>(MachineConfigTable.SubDir, _name, BasicConfig.Name);
		_basicConfig = new BasicConfig(basicSheet, this);

		SymbolSheet symbolSheet = assetManager.LoadExcelAsset<SymbolSheet, SymbolData>(MachineConfigTable.SubDir, _name, SymbolConfig.Name);
		_symbolConfig = new SymbolConfig(symbolSheet, this);

		ReelSheet reelSheet = assetManager.LoadExcelAsset<ReelSheet, ReelData>(MachineConfigTable.SubDir, _name, ReelConfig.Name);
		_reelConfig = new ReelConfig(reelSheet, this);

		PayoutSheet payoutSheet = assetManager.LoadExcelAsset<PayoutSheet, PayoutData>(MachineConfigTable.SubDir, _name, PayoutConfig.Name);
		_payoutConfig = new PayoutConfig(payoutSheet, this);

		if(_basicConfig.IsMultiLine)
		{
			if(_basicConfig.IsMultiLineExhaustive)
			{
				//only for multi payline
				PayoutDistSheet payoutDistSheet = assetManager.LoadExcelAsset<PayoutDistSheet, PayoutDistData>(MachineConfigTable.SubDir, _name, PayoutDistConfig.Name);
				_payoutDistConfig = new PayoutDistConfig(payoutDistSheet, this);

				PayoutDistSheet luckyPayoutDistSheet = assetManager.LoadExcelAsset<PayoutDistSheet, PayoutDistData>(MachineConfigTable.SubDir, _name, PayoutDistConfig.LuckySheetName);
				_luckyPayoutDistConfig = new PayoutDistConfig(luckyPayoutDistSheet, this);

				NearHitDistSheet nearHitDistSheet = assetManager.LoadExcelAsset<NearHitDistSheet, NearHitDistData>(MachineConfigTable.SubDir, _name, NearHitDistConfig.Name);
				_nearHitDistConfig = new NearHitDistConfig(nearHitDistSheet, this);

				NearHitDistSheet luckyNearHitDistSheet = assetManager.LoadExcelAsset<NearHitDistSheet, NearHitDistData>(MachineConfigTable.SubDir, _name, NearHitDistConfig.LuckySheetName);
				_luckyNearHitDistConfig = new NearHitDistConfig(luckyNearHitDistSheet, this);

				RewardResultSheet rewardResultSheet = assetManager.LoadExcelAsset<RewardResultSheet, RewardResultData>(MachineConfigTable.SubDir, _name, RewardResultConfig.Name);
				_rewardResultConfig = new RewardResultConfig(rewardResultSheet, this);
			}

			PaylineSheet paylineSheet = assetManager.LoadExcelAsset<PaylineSheet, PaylineData>(MachineConfigTable.SubDir, _name, PaylineConfig.Name);
			_paylineConfig = new PaylineConfig(paylineSheet, this);
		}
		else
		{
			//only for single payline
			PayoutSheet luckyPayoutSheet = assetManager.LoadExcelAsset<PayoutSheet, PayoutData>(MachineConfigTable.SubDir, _name, PayoutConfig.LuckySheetName);
			_luckyPayoutConfig = new PayoutConfig(luckyPayoutSheet, this);

			NearHitSheet nearHitSheet = assetManager.LoadExcelAsset<NearHitSheet, NearHitData>(MachineConfigTable.SubDir, _name, NearHitConfig.Name);
			_nearHitConfig = new NearHitConfig(nearHitSheet, this);

			NearHitSheet luckyNearHitSheet = assetManager.LoadExcelAsset<NearHitSheet, NearHitData>(MachineConfigTable.SubDir, _name, NearHitConfig.LuckySheetName);
			_luckyNearHitConfig = new NearHitConfig(luckyNearHitSheet, this);
		}

		// only for wheel
		if (_basicConfig.WheelSheetNames.Length > 0)
		{
			_wheelConfigDict = new Dictionary<string, WheelConfig>();
			_luckyWheelConfigDict = new Dictionary<string, WheelConfig>();

			for(int i = 0; i < _basicConfig.WheelSheetNames.Length; i++)
			{
				string sheetName = _basicConfig.WheelSheetNames[i];
				
				WheelSheet wheelSheet = assetManager.LoadExcelAsset<WheelSheet, WheelData>(MachineConfigTable.SubDir, _name, sheetName);
				_wheelConfigDict[sheetName] = new WheelConfig(wheelSheet, this, sheetName);

				string luckySheetName = "Lucky" + sheetName;
				WheelSheet luckyWheelSheet = assetManager.LoadExcelAsset<WheelSheet, WheelData>(MachineConfigTable.SubDir, _name, luckySheetName);
				_luckyWheelConfigDict[sheetName] = new WheelConfig(luckyWheelSheet, this, luckySheetName);
			}
		}
	}

	#endregion
}


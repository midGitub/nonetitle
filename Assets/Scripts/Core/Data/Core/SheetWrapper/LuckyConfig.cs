// #define ENABLE_LONGLUCKY_LOG

using System.Collections;
using System.Collections.Generic;
using System;

public class LuckyConfig
{
	public static readonly string Name = "Lucky";

	private LuckySheet _sheet;
	private CoreConfig _coreConfig;

	public LuckySheet Sheet { get { return _sheet; } }

	private int _longLuckyThreshold;
	private int _longLuckyFactorThreshold;
	private int _dailyCreditsAddLongLucky;
	private int _hourlyCreditsAddLongLucky;
	private int[] _firstGameShortLuckyRandomArray;
	private int _shortLuckyMaxNoPay;
	private int _shortLuckyMinNoPay;
	private int _shortLuckyMaxAfterPay;
	private int _shortLuckyMinAfterPay;
	private float _hitRateIncreateAfterPay;
	private float _shortLuckyExponent;
	private int _shortLuckyStep;
	private int _faceBookLoginLongLuck;
	private int _fistStartGameBonusLongLucky;
	private int _faceBookLoginAddCoins;
	private int _firstShortLuckyMax;
	private float _rewardFactor;
	// 第N次付费后可以打开破产保护
	private int[] _payProtectionBankruptIndexes;
	// 购买次数达到indexes列表里最大次数后每隔N次购买后才触发付费保护
	private int _payProtectionBankruptMaxInterval;
	// 分阶段给lucky的credits条件判断系数
	private float _longLuckyAdd_CreditsFactor1;
	// 分阶段给lucky的lucky条件判断系数
	private float _longLuckyAdd_LuckyFactor1;
	// 分阶段给lucky的credits条件判断系数
	private float _longLuckyAdd_CreditsFactor2;
	// 分阶段给lucky的lucky条件判断系数
	private float _longLuckyAdd_LuckyFactor2;

	public int LongLuckyThreshold { get { return _longLuckyThreshold; } }
	public int LongLuckyFactorThreshold { get { return _longLuckyFactorThreshold; } }
	public int DailyCreditsAddLongLucky { get { return _dailyCreditsAddLongLucky; } }
	public int HourlyCreditsAddLongLucky { get { return _hourlyCreditsAddLongLucky; } }
	public int[] FirstGameShortLuckyRandomArray { get { return _firstGameShortLuckyRandomArray; } }
	public int ShortLuckyMax { 
		get {
			return UserBasicData.Instance.PayProtectionEnable ? _shortLuckyMaxAfterPay : _shortLuckyMaxNoPay;
		} 
	}
	public int ShortLuckyMin { 
		get { 
			return UserBasicData.Instance.PayProtectionEnable ? _shortLuckyMinAfterPay : _shortLuckyMinNoPay;
		} 
	}
	public int ShortLuckMaxNoPay{ get { return _shortLuckyMaxNoPay; } }
	public int ShortLuckyMinNoPay { get { return _shortLuckyMinNoPay; } }
	public int ShortLuckyMaxAfterPay { get { return _shortLuckyMaxAfterPay; } }
	public int ShortLuckyMinAfterPay { get { return _shortLuckyMinAfterPay; } }
	public float HitRateIncreaseAfterPay { get { return _hitRateIncreateAfterPay; } }
	public float ShortLuckyExponent { get { return _shortLuckyExponent; } }
	public int ShortLuckyStep { get { return _shortLuckyStep; } }
	public int FaceBookLoginLongLuck { get { return _faceBookLoginLongLuck;} }
	public int FistStartGameBonusLongLucky { get { return _fistStartGameBonusLongLucky; } }
	public int FaceBookLoginAddCoins { get { return _faceBookLoginAddCoins; } }
	public int FirstShortLuckyMax { get { return _firstShortLuckyMax; } }
	public float RewardFactor { get { return _rewardFactor; } }
	public int[] PayProtectionBankruptIndexes { get { return _payProtectionBankruptIndexes; } }
	public int PayProtectionBankruptMaxInterval { get { return _payProtectionBankruptMaxInterval; } }

	public LuckyConfig(LuckySheet sheet, CoreConfig coreConfig)
	{
		_sheet = sheet;
		_coreConfig = coreConfig;

		Init();

		InitFirstShortLuckyMax ();
	}

	private void Init()
	{
		_longLuckyThreshold = GetIntValueFromKey("LongLuckyThreshold");
		_longLuckyFactorThreshold = GetIntValueFromKey("LongLuckyFactorThreshold");
		_dailyCreditsAddLongLucky = GetIntValueFromKey("DailyCreditsAddLongLucky");
		_hourlyCreditsAddLongLucky = GetIntValueFromKey("HourlyCreditsAddLongLucky");
		_firstGameShortLuckyRandomArray = GetIntArrayValueFromKey ("FirstGameShortLuckyRandomArray");
		_shortLuckyMaxNoPay = GetIntValueFromKey("ShortLuckyMax");
		_shortLuckyMinNoPay = GetIntValueFromKey("ShortLuckyMin");
		_shortLuckyMaxAfterPay = GetIntValueFromKey ("ShortLuckyMaxAfterPay");
		_shortLuckyMinAfterPay = GetIntValueFromKey ("ShortLuckyMinAfterPay");
		_hitRateIncreateAfterPay = GetFloatValueFromKey ("HitRateIncreaseAfterPay");
		_shortLuckyExponent = GetFloatValueFromKey("ShortLuckyExponent");
		_shortLuckyStep = GetIntValueFromKey("ShortLuckyStep");
		_faceBookLoginLongLuck = GetIntValueFromKey("LoginFaceBookCreditsAddLongLucky");
		_fistStartGameBonusLongLucky = GetIntValueFromKey("FistStartGameBonusLongLucky");
		_faceBookLoginAddCoins = GetIntValueFromKey("LoginFaceBookCreditsAddCredits");
		_rewardFactor = GetFloatValueFromKey ("TournamentRewardFactor");
		_payProtectionBankruptIndexes = GetIntArrayValueFromKey ("PayProtectionBankruptIndexes");
		_payProtectionBankruptMaxInterval = GetIntValueFromKey ("PayProtectionBankruptMaxInterval");
		_longLuckyAdd_CreditsFactor1 = GetFloatValueFromKey("LongLuckyAdd_CreditsFactor1");
		_longLuckyAdd_LuckyFactor1 = GetFloatValueFromKey("LongLuckyAdd_LuckyFactor1");
		_longLuckyAdd_CreditsFactor2 = GetFloatValueFromKey("LongLuckyAdd_CreditsFactor2");
		_longLuckyAdd_LuckyFactor2 = GetFloatValueFromKey("LongLuckyAdd_LuckyFactor2");
	}

	private int GetIntValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		if(string.IsNullOrEmpty(str))
		{
			CoreDebugUtility.LogError("Get key error:" + key);
			CoreDebugUtility.Assert(false);
		}
		int result = 0;
		int.TryParse(str, out result);
		return result;
	}

	private int[] GetIntArrayValueFromKey(string key){
		string result = ValueFromKey (key);

		if (!string.IsNullOrEmpty (result)) {
			return StringUtility.ParseToArray<int> (result);
		} 

		return new int[0];
	}

	private float GetFloatValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		CoreDebugUtility.Assert(!string.IsNullOrEmpty(str));
		float result = 0.0f;
		float.TryParse(str, out result);
		return result;
	}

	private string ValueFromKey(string key)
	{
		string result = "";
		int index = ListUtility.Find(_sheet.dataArray, (LuckyData data) =>
		{
			return data.Key == key;
		});
		if(index >= 0)
			result = _sheet.dataArray[index].Val;
		return result;
	}

	private void InitFirstShortLuckyMax(){
		int Seed = new System.Random().Next();
		IRandomGenerator iRG = new LCG((uint)Seed, null);
		_firstShortLuckyMax = RandomUtility.RollSingleElement (iRG, _firstGameShortLuckyRandomArray);
	}

	// 是否启用付费玩家破产保护
	public bool IsBankruptCompensate(int times, int lastBankruptBuytime){
		if (_payProtectionBankruptIndexes.Length == 0)
			return false;

		if (times == lastBankruptBuytime)
			return false;

		CoreDebugUtility.Assert(lastBankruptBuytime <= times, "lastBankruptBuytime > times");

		int length = _payProtectionBankruptIndexes.Length;
		int maxTimes = _payProtectionBankruptIndexes[length - 1];

		// 判断上次获得破产保护时的购买次数和现在的购买次数不在同一个段位
		if (times >= maxTimes && lastBankruptBuytime >= maxTimes){
			// 超过表内最大的购买次数后，以每次的间隔作为判断依据
			return Math.Abs(times - lastBankruptBuytime) >= _payProtectionBankruptMaxInterval;
		}
		else{
			for(int i = length - 1; i >= 0; --i){
				int buytimes = _payProtectionBankruptIndexes[i];
				if (times >= buytimes && lastBankruptBuytime < buytimes){
					return true;
				}
			}
		}

		return false;
	}

	// 判断是否达成分段lucky的条件
	public int IsInLongLuckyPeriod(ulong curCredits, ulong curLonglucky, ulong refCredits, ulong refLongLucky, ulong itemCredits){
		float x1 = refCredits + _longLuckyAdd_CreditsFactor2 * (float)itemCredits;
		float y1 = refLongLucky + _longLuckyAdd_LuckyFactor2 * (float)itemCredits;
		float x2 = refCredits + _longLuckyAdd_CreditsFactor1 * (float)itemCredits;
		float y2 = refLongLucky + _longLuckyAdd_LuckyFactor1 * (float)itemCredits;

		#if ENABLE_LONGLUCKY_LOG
		CoreDebugUtility.Log("itemCredits = "+itemCredits);
		CoreDebugUtility.Log("phase 2 creditsThreshold = " + x2);
		CoreDebugUtility.Log("phase 2 luckyThreshold = " + y2);
		CoreDebugUtility.Log("phase 3 creditsThreshold = " + x1);
		CoreDebugUtility.Log("phase 3 luckyThreshold = " + y1);
		#endif

		if (curCredits < x1 && curLonglucky < y1){
			#if ENABLE_LONGLUCKY_LOG
			CoreDebugUtility.Log("is in longlucky period 3");
			#endif
			return 2; // 第三阶段
		}
		else if (curCredits < x2 && curLonglucky < y2){
			#if ENABLE_LONGLUCKY_LOG
			CoreDebugUtility.Log("is in longlucky period 2");
			#endif
			 return 1; // 第二阶段
		}
	
		#if ENABLE_LONGLUCKY_LOG
		CoreDebugUtility.Log("is in longlucky period none");
		#endif
		return 0;// 未达成分段条件
	}
}

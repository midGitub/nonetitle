using System.Collections;
using System.Collections.Generic;

public class CoreSpinSymbolRestriction
{
	//Important note: for now we only supports symbolName is Wild or BonusWild
	//If we wanna support Seven or Bar, more code needs to write
	public string[] _symbolNames;
	public CompareType _compareType;
	public int _count;

	public CoreSpinSymbolRestriction(string[] symbolNames, CompareType compareType, int count)
	{
		_symbolNames = symbolNames;
		_compareType = compareType;
		_count = count;
	}
}

public class CoreSpinInput
{
	#if false// zhousen 屏蔽
	private CoreFreeSpinData _freeSpinData;
	public CoreFreeSpinData FreeSpinData { get { return _freeSpinData; } set { _freeSpinData = value; } }
	#endif

	private ulong _betAmount;
	private bool _isRespin;
	private CoreSymbol[] _fixedSymbols;
	private CoreSpinSymbolRestriction _symbolRestriction; //support only one restriction for now, might be extended to List in the future.
	private bool _isPayProtectionTest;//自动测试时模拟付费保护机制

	public ulong BetAmount { get { return _betAmount; } set { _betAmount = value; } }
	public bool IsRespin { get { return _isRespin; } }
	public CoreSymbol[] FixedSymbols { get { return _fixedSymbols; } set { _fixedSymbols = value; } }
	public CoreSpinSymbolRestriction SymbolRestriction { get { return _symbolRestriction; } set { _symbolRestriction = value; } }
	public bool IsPayProtectionTest { get { return _isPayProtectionTest; } set { _isPayProtectionTest = value; } }

	// zhousen 新增加对应各种small game state用的存储数据
	private CoreSpinData[] _spinDataArray;
	private SpinDataType _spinDataType;

	public CoreSpinData CurrentSpinData{
		get {
			return _spinDataArray[(int)_spinDataType];
		}
	}

	public SpinDataType CurrentSpinType{
		get { 
			return _spinDataType;
		}
	}

	public bool HasSpinData{
		get { 
			if (_spinDataType != SpinDataType.None) {
				return _spinDataArray [(int)_spinDataType] != null;
			} else {
				return false;
			}
		}
	}

	public CoreFreeSpinData FreeSpinData {
		get { 
			return (CoreFreeSpinData)_spinDataArray [(int)SpinDataType.FreeSpin];
		}
		set { 
			_spinDataArray [(int)SpinDataType.FreeSpin] = value;
		}
	}

	public CoreFixWildSpinData FixWildSpinData {
		get { 
			return (CoreFixWildSpinData)_spinDataArray [(int)SpinDataType.FixWild];
		}
		set { 
			_spinDataArray [(int)SpinDataType.FixWild] = value;
		}
	}

	public CoreBonusSpinData BonusSpinData {
		get { 
			return (CoreBonusSpinData)_spinDataArray [(int)SpinDataType.Bonus];
		}
		set { 
			_spinDataArray [(int)SpinDataType.Bonus] = value;
		}
	}

	public CoreCollectSpinData CollectSpinData {
		get { 
			return (CoreCollectSpinData)_spinDataArray [(int)SpinDataType.Collect];
		}
		set { 
			_spinDataArray [(int)SpinDataType.Collect] = value;
		}
	}

	///

	public CoreSpinInput(ulong betAmount, int reelCount, bool isRespin, 
		SpinDataType type = SpinDataType.FreeSpin, CoreSpinSymbolRestriction symbolRestriction = null)
	{
		_betAmount = betAmount;
		_isRespin = isRespin;
		_fixedSymbols = new CoreSymbol[reelCount];
		_spinDataArray = new CoreSpinData[(int)SpinDataType.Max];
		_spinDataType = type;
		_symbolRestriction = symbolRestriction;
		_isPayProtectionTest = false;
	}
}

using System.Collections;
using System.Collections.Generic;

public class SingleReel
{
	private int _reelId;
	private MachineConfig _machineConfig; //ref

	private List<string> _symbolNameList = new List<string>();
	private List<string> _wildNameList = new List<string>(); //wilds
	private List<string> _nonWildNameList = new List<string>(); //(all - wilds - (slide symbols))
	private List<int> _wildNeighborIndexList = new List<int>(); //wilds and their neighbors
	private List<int> _nonWildNeighborIndexList = new List<int>(); //(all - wilds and their neighbors)

	private List<float> _normalProbList;
	private List<float> _luckyProbList;
	private RollHelper _normalRollHelper;
	private RollHelper _luckyRollHelper;

	//map symbolName to indexes which has the symbolName. It's an optimization for CoreGenerator fast access
	private Dictionary<string, List<int>> _symbolNameToStopIndexesDict = new Dictionary<string, List<int>>();

	public List<string> SymbolNameList { get { return _symbolNameList; } }
	public List<string> WildNameList { get { return _wildNameList; } }
	public List<string> NonWildNameList { get { return _nonWildNameList; } }
	public List<int> WildNeighborIndexList { get { return _wildNeighborIndexList; } }
	public List<int> NonWildNeighborIndexList { get { return _nonWildNeighborIndexList; } }

	public List<float> NormalProbList { get { return _normalProbList; } }
	public List<float> LuckyProbList { get { return _luckyProbList; } }
	public RollHelper NormalRollHelper { get { return _normalRollHelper; } }
	public RollHelper LuckyRollHelper { get { return _luckyRollHelper; } }

	public Dictionary<string, List<int>> SymbolNameToStopIndexesDict { get { return _symbolNameToStopIndexesDict; } }

	public int SymbolCount
	{
		get { return _symbolNameList.Count; }
	}

	#region

	public SingleReel(int reelId, MachineConfig machineConfig, ReelConfig reelConfig)
	{
		_reelId = reelId;
		_machineConfig = machineConfig;

		InitSymbolNameList(reelConfig);
		InitWildNameList();
		InitWildNeighborIndexList();
		InitSymbolNameToStopIndexesDict();

		if(_machineConfig.BasicConfig.IsMultiLineSymbolProb)
		{
			InitProbLists(reelConfig);
			InitRollHelpers();
		}
	}

	private void InitSymbolNameList(ReelConfig reelConfig)
	{
		ReelData[] dataArray = reelConfig.Sheet.dataArray;
		for(int i = 0; i < dataArray.Length; i++)
		{
			ReelData reelData = dataArray[i];

			string name = InitSymbolName(_reelId, reelData);
			if(string.IsNullOrEmpty(name))
				break;
			else
				_symbolNameList.Add(name);
		}
	}

	private string InitSymbolName(int reelId, ReelData reelData)
	{
		string result = "";
		switch(reelId)
		{
			case 1:
				result = reelData.Reel1;
				break;
			case 2:
				result = reelData.Reel2;
				break;
			case 3:
				result = reelData.Reel3;
				break;
			case 4:
				result = reelData.Reel4;
				break;
			case 5:
				result = reelData.Reel5;
				break;
			default:
				CoreDebugUtility.Assert(false, "reelId is out of range");
				break;
		}
		return result;
	}

	public void InitWildNameList()
	{
		SymbolConfig symbolConfig = _machineConfig.SymbolConfig;

		_wildNameList.Clear();
		_nonWildNameList.Clear();

		for(int i = 0; i < _symbolNameList.Count; i++)
		{
			string name = _symbolNameList[i];
			if(symbolConfig.IsMatchSymbolType(name, SymbolType.Wild) || symbolConfig.IsMatchSymbolType(name, SymbolType.Wild7))
			{
				_wildNameList.Add(name);
			}
			else
			{
				if(!ShouldSlideToNeighbor(i))
					_nonWildNameList.Add(name);
			}
		}
	}

	public void InitWildNeighborIndexList()
	{
		SymbolConfig symbolConfig = _machineConfig.SymbolConfig;

		_wildNeighborIndexList.Clear();
		_nonWildNeighborIndexList.Clear();

		for(int i = 0; i < _symbolNameList.Count; i++)
		{
			bool isWildNeighbor = false;
			int upIndex = GetNeighborStopIndex(i, SpinDirection.Up);
			int downIndex = GetNeighborStopIndex(i, SpinDirection.Down);
			List<int> checkList = new List<int>{i, upIndex, downIndex};

			for(int k = 0; k < checkList.Count; k++)
			{
				int checkIndex = checkList[k];
				string n = _symbolNameList[checkIndex];
				if (symbolConfig.IsMatchSymbolWildType(n))
				{
					isWildNeighbor = true;
					break;
				}
			}

			if(isWildNeighbor)
				_wildNeighborIndexList.Add(i);
			else
				_nonWildNeighborIndexList.Add(i);
		}
	}

	private void InitSymbolNameToStopIndexesDict()
	{
		for(int i = 0; i < _symbolNameList.Count; i++)
		{
			string s = _symbolNameList[i];
			if(!_symbolNameToStopIndexesDict.ContainsKey(s))
				_symbolNameToStopIndexesDict.Add(s, new List<int>());
			_symbolNameToStopIndexesDict[s].Add(i);
		}
	}

	private void InitProbLists(ReelConfig reelConfig)
	{
		_normalProbList = new List<float>();
		_luckyProbList = new List<float>();

		ReelData[] dataArray = reelConfig.Sheet.dataArray;
		for(int i = 0; i < dataArray.Length; i++)
		{
			ReelData reelData = dataArray[i];

			float prob = GetProb(_reelId, reelData);
			_normalProbList.Add(prob);
			float luckyProb = GetLuckyProb(_reelId, reelData);
			_luckyProbList.Add(luckyProb);
		}
	}

	float GetProb(int reelId, ReelData data)
	{
		float result = 0;
		switch(reelId)
		{
			case 1:
				result = data.Prob1;
				break;
			case 2:
				result = data.Prob2;
				break;
			case 3:
				result = data.Prob3;
				break;
			case 4:
				result = data.Prob4;
				break;
			case 5:
				result = data.Prob5;
				break;
			default:
				CoreDebugUtility.Assert(false, "reelId is out of range");
				break;
		}
		return result;
	}

	float GetLuckyProb(int reelId, ReelData data)
	{
		float result = 0;
		switch(reelId)
		{
			case 1:
				result = data.LuckyProb1;
				break;
			case 2:
				result = data.LuckyProb2;
				break;
			case 3:
				result = data.LuckyProb3;
				break;
			case 4:
				result = data.LuckyProb4;
				break;
			case 5:
				result = data.LuckyProb5;
				break;
			default:
				CoreDebugUtility.Assert(false, "reelId is out of range");
				break;
		}
		return result;
	}

	void InitRollHelpers()
	{
		_normalRollHelper = new RollHelper(_normalProbList);
		_normalRollHelper.NormalizeProbs();

		_luckyRollHelper = new RollHelper(_luckyProbList);
		_luckyRollHelper.NormalizeProbs();
	}

	#endregion

	public string GetSymbolName(int stopIndex)
	{
		return _symbolNameList[stopIndex];
	}

	public SymbolType GetSymbolType(int stopIndex)
	{
		string name = GetSymbolName(stopIndex);
		return _machineConfig.SymbolConfig.GetSymbolType(name);
	}

	public bool IsContainSymbol(string name)
	{
		return _symbolNameList.Contains(name);
	}

	public bool IsContainAnySymbol(string[] names){
		return ListUtility.IsContainAnyElements (_symbolNameList, names);
	}

	public bool IsContainSymbol(SymbolType type)
	{
		bool result = ListUtility.IsAnyElementSatisfied(_symbolNameList, (string name) => {
			SymbolType t = _machineConfig.SymbolConfig.GetSymbolType(name);
			return t == type;
		});
		return result;
	}

	public List<int> GetStopIndexesForSymbolName(string symbolName)
	{
		List<int> result = null;
		if(_symbolNameToStopIndexesDict.ContainsKey(symbolName))
			result = _symbolNameToStopIndexesDict[symbolName];
		return result;
	}

	public int GetNeighborStopIndex(int curIndex, SpinDirection dir)
	{
		int offset = 0;
		if(dir == SpinDirection.Up)
			offset = 1;
		else if(dir == SpinDirection.Down)
			offset = -1;
		else
			CoreDebugUtility.Assert(false);
		
		return GetNeighborStopIndex(curIndex, offset);
	}

	public int GetNeighborStopIndex(int curIndex, int offset)
	{
		int symbolCount = SymbolCount;
		curIndex += offset;

		//it assumes not offset too much outside the range
		if(curIndex < 0)
			curIndex += symbolCount;
		else if(curIndex >= symbolCount)
			curIndex -= symbolCount;

		return curIndex;
	}

	public List<int> GetVisibleStopIndexes(int curIndex)
	{
		int down = GetNeighborStopIndex(curIndex, SpinDirection.Down);
		int up = GetNeighborStopIndex(curIndex, SpinDirection.Up);
		List<int> result = new List<int>(){ down, curIndex, up };
		return result;
	}

	public bool ShouldSlideToNeighbor(int curIndex, SpinDirection dir)
	{
		//1 return if it's non-slide level
		if(!_machineConfig.BasicConfig.HasSlide)
			return false;

		//2 return if it's non-blank symbol
		string curName = GetSymbolName(curIndex);
		SymbolData curData = _machineConfig.SymbolConfig.GetSymbolData(curName);
		if(curData.SymbolType != SymbolType.Blank)
			return false;

		//3 then check its neighbor
		bool result = false;
		int neighborIndex = GetNeighborStopIndex(curIndex, dir);
		string neighborName = GetSymbolName(neighborIndex);
		SymbolData neighborData = _machineConfig.SymbolConfig.GetSymbolData(neighborName);
		if(neighborData.SlideType != SlideType.None)
		{
			if(dir == SpinDirection.Up)
				result = (neighborData.SlideType == SlideType.Down || neighborData.SlideType == SlideType.UpDown);
			else if(dir == SpinDirection.Down)
				result = (neighborData.SlideType == SlideType.Up || neighborData.SlideType == SlideType.UpDown);
			else
				CoreDebugUtility.Assert(false);
		}
		return result;
	}

	public bool ShouldSlideToNeighbor(int curIndex)
	{
		if(!_machineConfig.BasicConfig.HasSlide)
			return false;

		bool shouldUp = ShouldSlideToNeighbor(curIndex, SpinDirection.Up);
		bool shouldDown = ShouldSlideToNeighbor(curIndex, SpinDirection.Down);
		return shouldUp || shouldDown;
	}

	public List<float> GetCurProbList(CoreLuckyMode mode)
	{
		#if DEBUG
		CoreDebugUtility.Assert(_machineConfig.BasicConfig.IsMultiLineSymbolProb);
		#endif

		List<float> result = mode == CoreLuckyMode.Normal ? _normalProbList : _luckyProbList;
		return result;
	}

	public RollHelper GetCurRollHelper(CoreLuckyMode mode)
	{
		#if DEBUG
		CoreDebugUtility.Assert(_machineConfig.BasicConfig.IsMultiLineSymbolProb);
		#endif

		RollHelper result = mode == CoreLuckyMode.Normal ? _normalRollHelper : _luckyRollHelper;
		return result;
	}

	#if true
	// zhousen 获得指定能在一个界面呈现符合count数的symbol的stopIndex集合
	public List<int> GetSymbolNumStopIndexList(string name, int count){
		List<int> retList = new List<int> ();

		for (int i = 0; i < _symbolNameList.Count; ++i) {
			if (CheckSymbolCount(i, name, count)){
				retList.Add (i);
			}
		}

		return retList;
	}

	// 当前的stopIndex索引是否能构成count个名字为name的symbol
	private bool CheckSymbolCount(int stopIndex, string name, int count){
		int max = _symbolNameList.Count;
		CoreDebugUtility.Assert (stopIndex < max && stopIndex >= 0, "stopindex overflow");

		string currentName = _symbolNameList [stopIndex];
		int last = stopIndex - 1;
		int next = stopIndex + 1;
		string lastName = _symbolNameList[GetStopIndex (last)];
		string nextName = _symbolNameList[GetStopIndex (next)];
		int sum = 0;

		if (currentName.Equals (name))
			sum++;
		if (lastName.Equals (name))
			sum++;
		if (nextName.Equals (name))
			sum++;

		return count == sum;
	}

	// stopindex边界检查
	private int GetStopIndex(int index){
		int max = _symbolNameList.Count;
		if (index >= max) {
			return 0;
		} else if (index < 0) {
			return max - 1;
		}
		return index;
	}

	public Dictionary<int, List<int>> ConstructUnorderSymbolCountDict(string symbol){
		Dictionary<int, List<int>> dict = new Dictionary<int, List<int>> ();

		for (int m = 0; m <= CoreDefine.UnorderSymbolCount; ++m) {
			List<int> stopIndexes = GetSymbolNumStopIndexList (symbol, m);
			if (stopIndexes.Count > 0) {
				dict.Add (m, stopIndexes);
			}
		}

		return dict;
	}

	#endif
}

public class ReelConfig
{
	public static readonly string Name = "Reel";

	private ReelSheet _sheet;
	private List<SingleReel> _singleReelList = new List<SingleReel>();
	private MachineConfig _machineConfig; //ref

	public ReelSheet Sheet { get { return _sheet; } }
	//public List<SingleReel> SingleReelList { get { return _singleReelList; } }

	public ReelConfig(ReelSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;
		_machineConfig = machineConfig;

		InitSingleReelList();
	}

	private void InitSingleReelList()
	{
		for(int reelId = 1; reelId <= _machineConfig.BasicConfig.ReelCount; reelId++)
		{
			SingleReel single = new SingleReel(reelId, _machineConfig, this);
			_singleReelList.Add(single);
		}
	}

	public SingleReel GetSingleReel(int index)
	{
		CoreDebugUtility.Assert(index >= 0 && index < _singleReelList.Count, "Input index out of range");
		return _singleReelList[index];
	}

	public string GetSymbolName(int reelIndex, int stopIndex)
	{
		SingleReel reel = GetSingleReel(reelIndex);
		return reel.GetSymbolName(stopIndex);
	}

	public string[] GetSymbolNames(IList<int> stopIndexes)
	{
		string[] symbolNames = new string[stopIndexes.Count];
		for(int i = 0; i < stopIndexes.Count; i++)
		{
			if(stopIndexes[i] != CoreDefine.InvalidIndex)
				symbolNames[i] = GetSymbolName(i, stopIndexes[i]);
		}
		return symbolNames;
	}

	//Caution: stopIndexes will be changed. So nothing to return.
	public void OffsetStopIndexes(IList<int> stopIndexes, IList<int> offsets)
	{
		for(int i = 0; i < stopIndexes.Count; i++)
		{
			SingleReel reel = GetSingleReel(i);
			stopIndexes[i] = reel.GetNeighborStopIndex(stopIndexes[i], offsets[i]);
		}
	}

	public List<CoreSymbol> GetNonPayoutLineSymbolsFromStopIndex(int reelIndex, int stopIndex)
	{
		List<CoreSymbol> result = new List<CoreSymbol> ();
		SingleReel reel = GetSingleReel(reelIndex);
		int neighborIndex1 = reel.GetNeighborStopIndex(stopIndex, 1);
		int neighborIndex2 = reel.GetNeighborStopIndex(stopIndex, -1);
		CoreSymbol s1 = _machineConfig.SymbolConfig.CreateCoreSymbol(reelIndex, neighborIndex1);
		CoreSymbol s2 = _machineConfig.SymbolConfig.CreateCoreSymbol(reelIndex, neighborIndex2);
		result.Add(s1);
		result.Add(s2);

		return result;
	}

	public List<List<int>> GetAllVisibleStopIndexes(List<int> stopIndexes)
	{
		List<List<int>> result = new List<List<int>>();
		for(int i = 0; i < stopIndexes.Count; i++)
		{
			int curIndex = stopIndexes[i];
			SingleReel reel = GetSingleReel(i);
			List<int> indexes = reel.GetVisibleStopIndexes(curIndex);
			result.Add(indexes);
		}
		return result;
	}
}

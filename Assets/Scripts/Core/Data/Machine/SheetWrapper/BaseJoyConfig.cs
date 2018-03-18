using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ReelsWildConfig
{
	private List<float[]> _probsList = new List<float[]>();
	private List<float> _probSumList = new List<float>();

	public List<float[]> ProbsList { get { return _probsList; } }
	public List<float> ProbSumList { get { return _probSumList; } }

	private MachineConfig _machineConfig = null;

	public ReelsWildConfig(IJoyData data, int reelCount, MachineConfig config)
	{
		_machineConfig = config;

		_probsList.Add(data.Reel1Wild);
		float sum1 = ListUtility.FoldList(data.Reel1Wild, MathUtility.Add);
		_probSumList.Add(sum1);

		_probsList.Add(data.Reel2Wild);
		float sum2 = ListUtility.FoldList(data.Reel2Wild, MathUtility.Add);
		_probSumList.Add(sum2);

		_probsList.Add(data.Reel3Wild);
		float sum3 = ListUtility.FoldList(data.Reel3Wild, MathUtility.Add);
		_probSumList.Add(sum3);

		if(reelCount >= 4)
		{
			_probsList.Add(data.Reel4Wild);
			float sum4 = ListUtility.FoldList(data.Reel4Wild, MathUtility.Add);
			_probSumList.Add(sum4);
		}

		#if DEBUG
		VerifyWildCount();
		#endif
	}

	#if DEBUG
	private void VerifyWildCount(){
		if (!_machineConfig.BasicConfig.IsMultiLine){
			ReelConfig reelConfig = _machineConfig.ReelConfig;
			for(int i = 0; i < _machineConfig.BasicConfig.ReelCount; ++i){
				SingleReel reel = reelConfig.GetSingleReel(i);
				float[] wilds = _probsList[i];

				//when no wilds on reel, don't check it.
				//Ex. M4 has no wild on reel 1,2,3
				if(reel.WildNameList.Count != 0)
					CoreDebugUtility.Assert(reel.WildNameList.Count == wilds.Length, "wild count is not equal");
			}
		}
	}
	#endif
}

public class SlideConfig
{
	private List<float> _probList = new List<float>();

	public List<float> ProbList { get { return _probList; } }

	public SlideConfig(IJoyData data)
	{
		_probList.Add(data.Slide1);
		_probList.Add(data.Slide2);
		_probList.Add(data.Slide3);
	}
}

//Common base for PayoutConfig and NearHitConfig
public class BaseJoyConfig
{
	protected MachineConfig _machineConfig; //ref
	protected float[] _overallHitArray;
	protected float _totalProb;
	protected List<ReelsWildConfig> _reelsWildConfigList = new List<ReelsWildConfig>();
	protected List<SlideConfig> _slideConfigList = new List<SlideConfig>();
	protected IJoyData[] _joyDataArray;

	public float[] OverallHitArray { get { return _overallHitArray; } }
	public float TotalProb { get { return _totalProb; } }
	public IJoyData[] JoyDataArray { get { return _joyDataArray; } }

	protected void Init(MachineConfig machineConfig, IJoyData []joyDataArray)
	{
		_machineConfig = machineConfig;
		_joyDataArray = joyDataArray;

		InitOverallHitArray();
		InitTotalHitProb();
		InitReelsWildConfigList();
		InitSlideConfigList();
	}

	private void InitOverallHitArray()
	{
		_overallHitArray = new float[_joyDataArray.Length];
		for(int i = 0; i < _joyDataArray.Length; i++)
		{
			IJoyData data = _joyDataArray[i];
			_overallHitArray[i] = data.OverallHit;
		}
	}

	private void InitTotalHitProb()
	{
		_totalProb = 0.0f;
		for(int i = 0; i < _joyDataArray.Length; i++)
		{
			IJoyData data = _joyDataArray[i];
			_totalProb += data.OverallHit;
		}
	}

	private void InitReelsWildConfigList()
	{
		for(int i = 0; i < _joyDataArray.Length; i++)
		{
			IJoyData data = _joyDataArray[i];
			ReelsWildConfig config = new ReelsWildConfig(data, _machineConfig.BasicConfig.ReelCount, _machineConfig);
			_reelsWildConfigList.Add(config);
		}
	}

	private void InitSlideConfigList()
	{
		for(int i = 0; i < _joyDataArray.Length; i++)
		{
			IJoyData data = _joyDataArray[i];
			SlideConfig config = new SlideConfig(data);
			_slideConfigList.Add(config);
		}
	}

	public ReelsWildConfig GetReelsWildConfig(IJoyData data)
	{
		int index = FindJoyDataIndex(data);
		CoreDebugUtility.Assert(index >= 0, "Found index out of range");
		return _reelsWildConfigList[index];
	}

	public SlideConfig GetSlideConfig(IJoyData data)
	{
		int index = FindJoyDataIndex(data);
		CoreDebugUtility.Assert(index >= 0, "Found index out of range");
		return _slideConfigList[index];
	}

	private int FindJoyDataIndex(IJoyData data)
	{
		int result = System.Array.FindIndex(_joyDataArray, (IJoyData d) => {
			return d.Id == data.Id;
		});
		return result;
	}

	public bool IsPureSymbols(IJoyData data, SymbolType type)
	{
		return ListUtility.IsAllElementsSatisfied(data.Symbols, (string name) => {
			return _machineConfig.SymbolConfig.IsMatchSymbolType(name, type);
		});
	}

	public bool IsPureSymbols(IJoyData data, SymbolType[] types)
	{
		return ListUtility.IsAllElementsSatisfied(data.Symbols, (string name) => {
			return ListUtility.IsAnyElementSatisfied(types, (SymbolType type) => {
				return _machineConfig.SymbolConfig.IsMatchSymbolType(name, type);
			});
		});
	}

	private List<IJoyData> GetRelatedItems(string symbolName, IJoyData exceptData)
	{
		List<IJoyData> result = new List<IJoyData>();
		for(int i = 0; i < _joyDataArray.Length; i++)
		{
			IJoyData data = _joyDataArray[i];
			if(data != exceptData && data.Symbols.Contains(symbolName))
				result.Add(data);
		}
		return result;
	}

//	public List<string> GetRelatedSymbols(string symbolName, IJoyData exceptData)
//	{
//		List<IJoyData> relatedPayouts = GetRelatedItems(symbolName, exceptData);
//		List<string> result = new List<string>();
//		for(int i = 0; i < relatedPayouts.Count; i++)
//		{
//			IJoyData data = relatedPayouts[i];
//			for(int k = 0; k < data.Symbols.Length; k++)
//			{
//				string s = data.Symbols[k];
//				if(s != symbolName && !result.Contains(s))
//					result.Add(s);
//			}
//		}
//		return result;
//	}
}

using System.Collections;
using System.Collections.Generic;

public class PaylineConfig
{
	public static readonly string Name = "Payline";

	private PaylineSheet _sheet;
	private MachineConfig _machineConfig; //ref

	private int _paylineCount;
	private List<int[]> _alllineList = new List<int[]>();
	private List<int[]> _paylineList = new List<int[]>();
	private List<int[]> _offlineList = new List<int[]>();

	public PaylineSheet Sheet { get { return _sheet; } }

	public int PaylineCount { get { return _paylineCount; } }
	public List<int[]> AlllineList { get { return _alllineList; } }
	public List<int[]> PaylineList { get { return _paylineList; } }
	public List<int[]> OfflineList { get { return _offlineList; } }

	#region Init

	public PaylineConfig(PaylineSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;
		_machineConfig = machineConfig;

		InitPaylines();
		InitAllAndOfflines();
	}

	private void InitPaylines()
	{
		_paylineCount = _sheet.dataArray.Length;

		for(int i = 0; i < _paylineCount; i++)
		{
			int[] arr = new int[_machineConfig.BasicConfig.ReelCount];
			for(int k = 0; k < arr.Length; k++)
				arr[k] = GetSheetData(i, k);
			
			_paylineList.Add(arr);
		}
	}

	private void InitAllAndOfflines()
	{
		if(_machineConfig.BasicConfig.IsMultiLineExhaustive)
		{
			int reelCount = _machineConfig.BasicConfig.ReelCount;
			int[] points = new int[]{ -1, 0, 1 };
			int totalCount = reelCount * reelCount * reelCount;
			for(int i = 0; i < totalCount; i++)
			{
				int k0 = i % reelCount;
				int r = i / reelCount;
				int k1 = r % reelCount;
				int k2 = r / reelCount;

				int[] line = new int[]{ points[k0], points[k1], points[k2] };
				_alllineList.Add(line);

				if(!IsPayline(line))
					_offlineList.Add(line);
			}
		}
		else if(_machineConfig.BasicConfig.IsMultiLineSymbolProb)
		{
			//for SymbolProb, no offlineList concept, and alllineList is just paylineList
			_alllineList = new List<int[]>(_paylineList);
		}
		else
		{
			CoreDebugUtility.Assert(false);
		}


	}

	#endregion

	#region Private

	int GetSheetData(int dataArrayIndex, int reelIndex)
	{
		int result = 0;
		if(reelIndex == 0)
			result = _sheet.dataArray[dataArrayIndex].Reel1;
		else if(reelIndex == 1)
			result = _sheet.dataArray[dataArrayIndex].Reel2;
		else if(reelIndex == 2)
			result = _sheet.dataArray[dataArrayIndex].Reel3;
		else if(reelIndex == 3)
			result = _sheet.dataArray[dataArrayIndex].Reel4;
		else if(reelIndex == 4)
			result = _sheet.dataArray[dataArrayIndex].Reel5;
		else
			CoreDebugUtility.Assert(false);
		
		return result;
	}

	#endregion

	#region Public

	public bool IsPayline(int[] line)
	{
		bool result = false;
		for(int i = 0; i < _paylineList.Count; i++)
		{
			int[] payline = _paylineList[i];
			if(ListUtility.IsEqualLists(payline, line))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool IsOffline(int[] line)
	{
		#if DEBUG
		CoreDebugUtility.Assert(_machineConfig.BasicConfig.IsMultiLineExhaustive, "only call this function when exhaustive");
		#endif

		bool result = false;
		for(int i = 0; i < _offlineList.Count; i++)
		{
			int[] offline = _offlineList[i];
			if(ListUtility.IsEqualLists(offline, line))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int GetPaylineIndex(int[] line)
	{
		int result = -1;
		for(int i = 0; i < _paylineList.Count; i++)
		{
			int[] payline = _paylineList[i];
			if(ListUtility.IsEqualLists(payline, line))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	#endregion
}

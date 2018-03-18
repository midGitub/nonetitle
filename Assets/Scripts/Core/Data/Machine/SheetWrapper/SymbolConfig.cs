using System.Collections;
using System.Collections.Generic;

public class SymbolConfig
{
	public static readonly string Name = "Symbol";

	private SymbolSheet _sheet;
	private MachineConfig _machineConfig; //ref
	private int _jackpotCountMax;// 最大的jackpotCount数

	public SymbolSheet Sheet { get { return _sheet; } }
	public int JackpotCountMax{ get { return _jackpotCountMax; } }

	public SymbolConfig(SymbolSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;
		_machineConfig = machineConfig;
		FetchJackpotCountMax();
	}

	private void FetchJackpotCountMax(){
		_jackpotCountMax = 0;
		for(int i = 0; i < _sheet.DataArray.Length; ++i){
			SymbolData data = _sheet.DataArray[i];
			if (data.JackpotCount > _jackpotCountMax){
				_jackpotCountMax = data.JackpotCount;
			}
		}
	}

	public SymbolData GetSymbolData(string name)
	{
		SymbolData result = null;
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			SymbolData data = _sheet.dataArray[i];
			if(data.Name == name)
			{
				result = data;
				break;
			}
		}
		if(result == null)
			CoreDebugUtility.LogError("Fail when call GetSymbolDataFromName");
		return result;
	}

	public SymbolType GetSymbolType(string name)
	{
		SymbolData symbolData = GetSymbolData(name);
		#if DEBUG
		if (symbolData == null) {
			CoreDebugUtility.Log ("GetSymbolType name = " + name);
		}
		#endif
		return symbolData.SymbolType;
	}

	public List<string> GetSymbolNames(SymbolType type)
	{
		List<string> result = new List<string>();
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			SymbolData data = _sheet.dataArray[i];
			if(data.SymbolType == type)
				result.Add(data.Name);
		}
		return result;
	}

	public List<string> GetSlideSymbolNames()
	{
		List<string> result = new List<string>();
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			SymbolData data = _sheet.dataArray[i];
			if(data.SlideType != SlideType.None)
				result.Add(data.Name);
		}
		return result;
	}

	public CoreSymbol CreateCoreSymbol(int reelIndex, int stopIndex)
	{
		string name = _machineConfig.ReelConfig.GetSymbolName(reelIndex, stopIndex);
		CoreSymbol symbol = new CoreSymbol(reelIndex + 1, stopIndex + 1, name, this);
		return symbol;
	}

	public int GetMultiplier(string symbolName)
	{
		int result = 1;
		SymbolType type = GetSymbolType(symbolName);
		if(IsMatchSymbolWildType(type))
		{
			SymbolData data = GetSymbolData(symbolName);
			result = data.Multiplier;
		}
		return result;
	}

	public string[] GetSymbolCanApplyCollect(string name)
	{
		SymbolData result = null;
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			SymbolData data = _sheet.dataArray[i];
			if(data.Name == name)
			{
				result = data;
				break;
			}
		}
		if(result == null)
			CoreDebugUtility.LogError("Fail when call GetSymbolCanApplyCollect");
		
		return result.CanApplyCollect;
	}

	public bool CheckSymbolCanApplyCollect(string name, string collect){
		string[] collectArray = GetSymbolCanApplyCollect (name);
		if (collectArray != null) {
			return ListUtility.IsAnyElementSatisfied (collectArray, (string c)=>{
				return collect == c;
			});
		}
		return false;
	}

	#region Type predicate

	public bool IsMatchSymbolType(string name, SymbolType destType)
	{
		SymbolType t = GetSymbolType(name);
		return IsMatchSymbolType(t, destType);
	}

	public bool IsMatchSymbolType(SymbolType srcType, SymbolType destType)
	{
		return CoreUtility.IsMatchSymbolType(srcType, destType);
	}

	public bool IsMatchSymbolWildType(SymbolType srcType){
		return CoreUtility.IsMatchSymbolType (srcType, SymbolType.Wild) ||
			CoreUtility.IsMatchSymbolType (srcType, SymbolType.Wild7);
	}

	public bool IsMatchSymbolWildType(string name){
		SymbolType t = GetSymbolType(name);
		return IsMatchSymbolWildType(t);
	}

	public bool IsMatchAnySymbolWildTypes(string[] names){
		return ListUtility.IsAnyElementSatisfied (names, IsMatchSymbolWildType);
	}

	public bool IsSevenBarSymbolType(string name)
	{
		SymbolType type = GetSymbolType(name);
		return ListUtility.IsContainElement(CoreDefine.SevenBarTypes, type);
	}

	public bool IsSevenSymbolType(string name){
		SymbolType type = GetSymbolType (name);
		return type == SymbolType.Seven;
	}

	public bool CanWildChange(string name)
	{
		return IsSevenBarSymbolType(name);
	}

	public bool CanWild7Change(string name){
		return IsSevenSymbolType (name);
	}

	#endregion
}

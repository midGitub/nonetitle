using System.Collections;
using System.Collections.Generic;

public class CoreReel
{
	private int _id;
	private SingleReel _reelConfig;
	private List<CoreSymbol> _symbolList = new List<CoreSymbol>();

	private MachineConfig _machineConfig; //ref

	public int Id { get { return _id; } }
	public SingleReel ReelConfig { get { return _reelConfig; } }
	public List<CoreSymbol> SymbolList { get { return _symbolList; } }

	public CoreReel(int id, SingleReel reelConfig, MachineConfig machineConfig)
	{
		_id = id;
		_reelConfig = reelConfig;
		_machineConfig = machineConfig;

		InitSymbols();
	}

	private void InitSymbols()
	{
		List<string> names = _reelConfig.SymbolNameList;
		for(int i = 0; i < names.Count; i++)
		{
			string name = names[i];
			SymbolData symbolData = _machineConfig.SymbolConfig.GetSymbolData(name);
			CoreSymbol symbol = new CoreSymbol(_id, i + 1, symbolData);
			_symbolList.Add(symbol);
		}
	}

	public int SymbolCount()
	{
		return _symbolList.Count;
	}
}

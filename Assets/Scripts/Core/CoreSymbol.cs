using System.Collections;

[System.Serializable]
public class CoreSymbol
{
	private int _reelId; //which reel it's on
	private int _stopId; //stop pos in the reel
	private SymbolData _symbolData;

	public int ReelId { get { return _reelId; } }
	public int ReelIndex { get { return _reelId - 1; } }
	public int StopId { get { return _stopId; } }
	public int StopIndex { get { return _stopId - 1; } }
	public SymbolData SymbolData { get { return _symbolData; } }

	public CoreSymbol(int reelId, int stopId, SymbolData symbolData)
	{
		_reelId = reelId;
		_stopId = stopId;
		_symbolData = symbolData;
	}

	public CoreSymbol(int reelId, int stopId, string name, SymbolConfig config)
	{
		_reelId = reelId;
		_stopId = stopId;
		_symbolData = config.GetSymbolData(name);
	}
}

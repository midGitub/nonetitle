using System.Collections;
using System.Collections.Generic;

public class CoreCollectData {

	// zhousen 是否有收集物依附
	private bool _hasCollectSymbol;
	// 收集物名称
	private string _collectSymbol;

	public int ReelIndex { get; set; }
	public int StopIndex { get; set; }

	public bool HasCollectSymbol { get { return _hasCollectSymbol; }  }
	public string CollectSymbol { get { return _collectSymbol; }  }


	public CoreCollectData(int reelIndex, int stopIndex){
		ReelIndex = reelIndex;
		StopIndex = stopIndex;
	}

	public void AttachCollectSymbol(string symbol){
		_hasCollectSymbol = true;
		_collectSymbol = symbol;
	}

	public void DetachCollectSymbol(){
		_hasCollectSymbol = false;
		_collectSymbol = "";
	}
}

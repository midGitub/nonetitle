using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankruptCompensateConfig : SimpleSingleton<BankruptCompensateConfig>
{
	public static readonly string Name = "BankruptCompensate";

	private BankruptCompensateSheet _sheet;
	private Dictionary<string, int[]> _dict = new Dictionary<string, int[]>();

	public BankruptCompensateConfig(){
		LoadData ();
	}

	private void LoadData(){
		_sheet = GameConfig.Instance.LoadExcelAsset<BankruptCompensateSheet>(Name);
		InitDict (_sheet);
	}

	private void InitDict(BankruptCompensateSheet sheet){
		_dict.Clear ();
		ListUtility.ForEach(_sheet.DataArray, (BankruptCompensateData data)=>{
			_dict.Add(data.Key, data.Data);
		});
	}

	public static void Reload()
	{
		Debug.Log("Reload BankruptCompensateConfig");
		BankruptCompensateConfig.Instance.LoadData();
	}

	public int GetCompensateCredits(float totalPayAmounts, int buyTimes, int[] referenceIndexes){
		// 根据总体付费金额和付费次数来得到credits
		int[] creditsArray = new int[0];

		// 搜索符合条件的credits数组
		foreach(var pair in _dict){
			string key = pair.Key;
			string[] prices = key.Split (new char[]{ '-' });
			Debug.Assert (prices.Length == 2, "getcompensate prices length != 2");
			float min, max;
			bool noError = float.TryParse (prices [0], out min);
			Debug.Assert(noError, "bankrupt compensate min parse failed : "+prices[0]);
			noError = float.TryParse(prices [1], out max);
			Debug.Assert(noError, "bankrupt compensate max parse failed : "+prices[1]);
			if (totalPayAmounts >= min && totalPayAmounts <= max) {
				creditsArray = pair.Value;
				break;
			}
		}

		// 从credits数组中寻找符合条件的
		if (creditsArray.Length > 0){
			int referenceLength = referenceIndexes.Length;
			int creditsIndex = -1;
			for (int i = referenceLength - 1; i >= 0; --i) {
				if (buyTimes >= referenceIndexes [i]) {
					creditsIndex = i;
					break;
				}
			}

			if (creditsIndex >= 0) {
				return creditsArray [creditsIndex];
			}
		}

		return 0;
	}

	public void Test(){
		#if DEBUG
		int max = 5;
		int[] testRef = new int[]{ 1, 4, 7, 10, 13 };
		float[] totalAmounts = new float[]{ 5.99f, 29.0f, 55.0f, 13.0f, 100.0f };
		int[] buyTimes = new int[]{ 3, 7, 15, 4, 9 };
		int[] creditsHope = new int[]{ 1000, 11000, 26000, 4000, 25000};
		for (int i = 0; i < max; ++i) {
			Debug.Assert (GetCompensateCredits(totalAmounts[i], buyTimes[i], testRef) == creditsHope[i], "test" + i);
		}
		#endif
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DailyType
{
	D1 = 0,
	D2,
	D3,
	D4,
	D5
}

public class DailyBonusConfig : SimpleSingleton<DailyBonusConfig>
{
	public static readonly string Name = "DailyBonus";

	public DailyBonusSheet Sheet { get; private set; }

	public List<DailyBonusData> DailyBonusList { get; private set; }

	/// <summary>
	/// 每天几率的总和
	/// </summary>
	public Dictionary<DailyType, int> SumProbabilityDictionay = new Dictionary<DailyType, int>();

	public DailyBonusConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		Sheet = GameConfig.Instance.LoadExcelAsset<DailyBonusSheet>(Name);
		DailyBonusList = Sheet.dataArray.ToList();
	}

	public static void Reload()
	{
		Debug.Log("Reload DailyBonusConfig");
		DailyBonusConfig.Instance.LoadData();
	}

	/// <summary>
	/// Gets the <see cref="T:DailyBonusConfig"/> with the specified id.
	/// 通过ID找到需要的Daily列
	/// </summary>
	/// <param name="id">Identifier.</param>
	public DailyBonusData this[int id]
	{
		get
		{
			return DailyBonusList.FirstOrDefault((a) => { return a.BonusID == id; });
		}
	}

	/// <summary>
	/// Gets the bonus probability.
	/// 根据ID得到某一天的中奖率
	/// </summary>
	/// <returns>The bonus probability.</returns>
	/// <param name="id">Identifier.</param>
	/// <param name="dt">Dt.</param>
	public int GetBonusProbability(int id, DailyType dt)
	{

		int dayP = GetDBDTypeProb(this[id], dt);

		return dayP;
	}

	public int GetSumProbability(DailyType dt)
	{
		if (!SumProbabilityDictionay.ContainsKey(dt))
		{
			SumProbabilityDictionay.Add(dt, SumProbability(dt));
		}

		return SumProbabilityDictionay[dt];
	}

	private int SumProbability(DailyType dt)
	{
		int sum = 0;
		foreach (var item in DailyBonusList)
		{
			sum += GetDBDTypeProb(item, dt);
		}

		return sum;
	}

	private int GetDBDTypeProb(DailyBonusData db, DailyType dt)
	{
		int dayP = 0;
		switch (dt)
		{
			case DailyType.D1:
				dayP = db.Prob1;
				break;
			case DailyType.D2:
				dayP = db.Prob2;
				break;
			case DailyType.D3:
				dayP = db.Prob3;
				break;
			case DailyType.D4:
				dayP = db.Prob4;
				break;
			case DailyType.D5:
				dayP = db.Prob5;
				break;
			default:
				Debug.Assert(false);
				break;
		}
		return dayP;
	}

}

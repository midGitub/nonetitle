using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinsPoolItem : MonoBehaviour 
{
	private Dictionary<TournamentMatchSection, string> SectionTextDic = new Dictionary<TournamentMatchSection, string>();
	public Text SectionText;
	public Text AllCoins;

	// Use this for initialization
	private void Awake()
	{
		SectionTextDic.Add(TournamentMatchSection.Small, "0 - 500 BET");
		SectionTextDic.Add(TournamentMatchSection.Normal,"1,000-5,000 BET");
		SectionTextDic.Add(TournamentMatchSection.Big,"10,000+ BET");
	}

	public void SetValue(ulong allcoins,TournamentMatchSection ts)
	{
		SectionText.text = SectionTextDic[ts];
		AllCoins.text = StringUtility.FormatNumberString(allcoins,true,true);
	}

}

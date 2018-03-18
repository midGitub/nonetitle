using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System;

public class JackpotData  {
	private static readonly string _currentBonusTag = "CurrentBonus";
	private static readonly string _nextBonusTag = "NextBonus";

	public int CurrentBonus { get; set; }
	public int NextBonus { get; set; }

	public JackpotData(){
		CurrentBonus = 0;
		NextBonus = 0;
	}

	public JackpotData(int current, int next){
		CurrentBonus = current;
		NextBonus = next;
	}

	public static string Serialize(JackpotData data){
		if (data == null) {
			return  "null";
		}

		Dictionary<string, object> dict = new Dictionary<string, object> ();

		dict [_currentBonusTag] = data.CurrentBonus;
		dict [_nextBonusTag] = data.NextBonus;

		string result = Json.Serialize (dict);

		return result;
	}

	public static JackpotData Deserialize(string s){
		JackpotData data = new JackpotData();
		Dictionary<string, object> dict = Json.Deserialize(s) as Dictionary<string, object>;
		if (dict == null) {
			return data;
		}

		if (dict.ContainsKey(_currentBonusTag)){
			data.CurrentBonus = Convert.ToInt32(dict[_currentBonusTag]);
		}

		if (dict.ContainsKey(_nextBonusTag)){
			data.NextBonus = Convert.ToInt32(dict[_nextBonusTag]);
		}

		return data;
	}
}

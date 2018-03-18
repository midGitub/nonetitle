using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System;

public enum GiftType{
	None,
	Credit,
	Lucky,
	Diamond,
	Max,
}

public class GiftDataBase{
	protected GiftType _type;// 礼物品类
	protected int _num;// 礼物数量

	public GiftType Type{
		get{ 
			return _type;
		}
	}

	public int Num{ 
		get { 
			return _num;
		}
		set { 
			_num = value;
		}
	}

	public GiftDataBase(){
		_type = GiftType.None;
		_num = 0;
	}

	public GiftDataBase(int num){
		_type = GiftType.None;
		_num = num;
	}
}

public class CreditGift : GiftDataBase  {

	public CreditGift() : base(){
		_type = GiftType.Credit;
	}

	public CreditGift(int num) : base(num){
		_type = GiftType.Credit;
	}
}

public class DaimondGift : GiftDataBase {

	public DaimondGift() : base(){
		_type = GiftType.Diamond;
	}

	public DaimondGift(int num) : base(num){
		_type = GiftType.Diamond;
	}
}

public class LuckyGift : GiftDataBase {

	public LuckyGift() : base(){
		_type = GiftType.Lucky;
	}

	public LuckyGift(int num) : base(num){
		_type = GiftType.Lucky;
	}
}

public class FriendGift{
	private Dictionary<GiftType, GiftDataBase> _giftDict = new Dictionary<GiftType, GiftDataBase>();
	public Dictionary<GiftType, GiftDataBase> GiftDict{
		get { 
			return _giftDict;
		}
		set { 
			_giftDict = value;
		}
	}

	public FriendGift(){
		_giftDict.Add(GiftType.Credit, FriendGift.CreateGift(GiftType.Credit, FriendSettingConfig.Instance.GiftCoins));
	}

	public void SetGift(GiftType type, GiftDataBase data){
		if (_giftDict != null) {
			_giftDict [type] = data;
		}
	}

	public GiftDataBase GetGift(GiftType type){
		if (_giftDict != null && _giftDict.ContainsKey (type)) {
			return _giftDict [type];
		}

		return null;
	}

	public void RemoveGift(GiftType type){
		if (_giftDict != null && _giftDict.ContainsKey (type)) {
			_giftDict.Remove (type);
		}
	}

	public static string Serialize(FriendGift gift){
		if (gift == null)
			return "";
		
		Dictionary<string, object> dict = new Dictionary<string, object> ();

		foreach (var pair in gift.GiftDict) {
			dict.Add (pair.Key.ToString(), pair.Value.Num);
		}

		return Json.Serialize (dict);
	}

	public static FriendGift Deserialize(string s){
		FriendGift gift = null;
		Dictionary<string , object> dict = Json.Deserialize (s) as Dictionary<string, object>;
		Dictionary<GiftType , GiftDataBase> giftdict = new Dictionary<GiftType, GiftDataBase> ();
		foreach (var pair in dict) {
			GiftType type = GetGiftType (pair.Key);
			GiftDataBase data = CreateGift (type, Convert.ToInt32 (pair.Value));
			if (type != GiftType.None) {
				giftdict.Add (type, data);
			}
		}

		if (giftdict.Count > 0) {
			gift = new FriendGift ();
			gift.GiftDict = giftdict;
		}

		return gift;
	}

	public static GiftType GetGiftType(string s){
		if (s.Equals (GiftType.Credit.ToString ())) {
			return GiftType.Credit;
		}
		else if (s.Equals (GiftType.Lucky.ToString ())) {
			return GiftType.Lucky;
		}
		else if (s.Equals (GiftType.Diamond.ToString ())) {
			return GiftType.Diamond;
		}
		return GiftType.None;
	}

	public static GiftDataBase CreateGift(GiftType type, int num){
		if (type == GiftType.Credit) {
			return new CreditGift (num);
		} else if (type == GiftType.Lucky) {
			return new LuckyGift (num);
		} else if (type == GiftType.Diamond) {
			return new DaimondGift (num);
		}

		LogUtility.Log ("invalid gift type = " + type.ToString());
		return null;
	} 
}
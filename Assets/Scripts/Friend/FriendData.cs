using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MiniJSON;


public class FriendData
{
	private static readonly string _nameTag = "name";
	private static readonly string _idTag = "udid";
	private static readonly string _iconTag = "icon";
	private static readonly string _levelTag = "level";
	private static readonly string _giftTag = "gift";
	private static readonly string _lastSendTimeTag = "lastSendTime";

	public string Name{ get; set; }
	public string ID{ get; set; }
	public string ICON { get; set; }
	public int Level { get; set; }
	public FriendGift Gift { get; set; }
	public DateTime LastSendTime{ get; set; }

	public FriendData (){
		Name = "";
		ID = "";
		ICON = "";
		Level = 0;
		Gift = new FriendGift();
		LastSendTime = DateTime.MinValue;
	}

	public FriendData(string name, string id, string icon_path) {
		Name = name;
		ID = id;
		ICON = icon_path;
		Level = 0;
		Gift = new FriendGift();
		LastSendTime = DateTime.MinValue;
	}

	public FriendData(string name, string id, string icon_path, int level, FriendGift gift, DateTime time ) {
		Name = name;
		ID = id;
		ICON = icon_path;
		Level = level;
		Gift = gift;
		LastSendTime = time;
	}

	public static string Serialize(FriendData data){
		if (data == null) {
			return "";
		}
		Dictionary<string, object> dict = new Dictionary<string, object> ();
		dict.Add (_nameTag, data.Name);
		dict.Add (_idTag, data.ID);
		dict.Add (_iconTag, data.ICON);
		dict.Add (_levelTag, data.Level);
		dict.Add (_giftTag, FriendGift.Serialize (data.Gift));
		dict.Add (_lastSendTimeTag, data.LastSendTime);

		return Json.Serialize (dict);
	}

	public static FriendData Deserialize(string s){
		FriendData result = null;
		Dictionary<string, object> dict = Json.Deserialize(s) as Dictionary<string, object>;
		if (dict.ContainsKey (_nameTag) && dict.ContainsKey (_idTag) 
			&& dict.ContainsKey(_iconTag) && dict.ContainsKey (_levelTag)
			&& dict.ContainsKey (_giftTag) && dict.ContainsKey (_lastSendTimeTag)) {

			string name = Convert.ToString(dict [_nameTag]);
			string id = Convert.ToString(dict [_idTag]);
			string icon = Convert.ToString (dict [_iconTag]);
			int level = Convert.ToInt32 (dict [_levelTag]);
			FriendGift gift = FriendGift.Deserialize(Convert.ToString (dict [_giftTag]));
			DateTime lastSendTime = Convert.ToDateTime (dict [_lastSendTimeTag]);

			result = new FriendData (name, id, icon, level, gift, lastSendTime);
		}

		return result;
	}

	public override string ToString ()
	{
		return string.Format ("[FriendData: Name={0}, ID={1}, ICON={2}, Level={3}, Gift={4}, LastSendTime={5}]", Name, ID, ICON, Level, Gift, LastSendTime);
	}
}

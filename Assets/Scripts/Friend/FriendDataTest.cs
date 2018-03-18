using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FriendDataTest{
	public static List<FriendData> CollectTestFriends = new List<FriendData>(){
		new FriendData("name1", "1", "", 100,null,  DateTime.MinValue),
		new FriendData("name2", "2","",  200,null,  DateTime.MinValue),
		new FriendData("name3", "3", "", 300,null,  DateTime.MinValue),
		new FriendData("name4", "4", "", 400,null,  DateTime.MinValue),
		new FriendData("name5", "5","",  500,null,  DateTime.MinValue),
		new FriendData("name6", "6", "", 600,null,  DateTime.MinValue),
		new FriendData("name7", "7", "", 790,null,  DateTime.MinValue),
	};
	public static List<FriendData> SendTestFriends = new List<FriendData>(){
		new FriendData("name11", "11", "", 100,null,  DateTime.MinValue),
		new FriendData("name12", "12","",  200,null,  DateTime.MinValue),
		new FriendData("name13", "13", "", 300,null,  DateTime.MinValue),
		new FriendData("name14", "14", "", 400,null,  DateTime.MinValue),
		new FriendData("name15", "15","",  500,null,  DateTime.MinValue),
		new FriendData("name16", "16", "", 600,null,  DateTime.MinValue),
		new FriendData("name17", "17", "", 790,null,  DateTime.MinValue),
	};
	public static List<FriendData> InviteTestFriends = new List<FriendData>(){
		new FriendData("name21", "21", "", 100,null,  DateTime.MinValue),
		new FriendData("name22", "22","",  200,null,  DateTime.MinValue),
		new FriendData("name23", "23", "", 300,null,  DateTime.MinValue),
		new FriendData("name24", "24", "", 400,null,  DateTime.MinValue),
		new FriendData("name25", "25","",  500,null,  DateTime.MinValue),
		new FriendData("name26", "26", "", 600,null,  DateTime.MinValue),
		new FriendData("name27", "27", "", 790,null,  DateTime.MinValue),
	};
}
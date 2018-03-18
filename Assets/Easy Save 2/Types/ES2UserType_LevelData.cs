using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class ES2UserType_LevelData : ES2Type
{
	public override void Write(object data, ES2Writer writer)
	{
		LevelData ld = (LevelData)data;
		writer.Write((float)ld.Level);
		writer.Write((float)ld.LevelPoint);
	}

	public override object Read(ES2Reader reader)
	{
		LevelData ld = new LevelData { Level = reader.Read<float>(), LevelPoint = reader.Read<float>() };
		return ld;
	}

	public ES2UserType_LevelData() : base(typeof(LevelData)) { }
}

public class LevelData
{
	/// <summary>
	/// 当前的等级
	/// </summary>
	public ObscuredFloat Level = 0;

	/// <summary>
	/// 当前的点数
	/// </summary>
	public ObscuredFloat LevelPoint = 0;

	public static LevelData CreateData(int level, int point){
		LevelData data = new LevelData ();
		data.Level = level;
		data.LevelPoint = point;
		return data;
	}

	public static int Compare(LevelData a, LevelData b)
	{
		int result = 0;
		if(a.Level < b.Level || (a.Level == b.Level && a.LevelPoint < b.LevelPoint))
		{
			result = -1;
		}
		else if(a.Level > b.Level || (a.Level == b.Level && a.LevelPoint > b.LevelPoint))
		{
			result = 1;
		}
		else
		{
			Debug.Assert(a.Level == b.Level && a.LevelPoint == b.LevelPoint);
			result = 0;
		}
		return result;
	}
}

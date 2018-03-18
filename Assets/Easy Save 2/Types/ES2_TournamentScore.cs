using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ES2_TournamentScore : ES2Type
{

	public override void Write(object obj, ES2Writer writer)
	{
		TournamentScoreInfor infor = (TournamentScoreInfor)obj;
		writer.Write(infor.WriteTime);
		writer.Write(infor.LastTime);
		writer.Write(infor.AllScore);
	}

	public override object Read(ES2Reader reader)
	{
		TournamentScoreInfor infor = new TournamentScoreInfor
		{
			WriteTime = reader.Read<DateTime>(),
			LastTime = reader.Read<float>(),
			AllScore = reader.Read<ulong>()
		};

		return infor;
	}

	public ES2_TournamentScore() : base(typeof(TournamentScoreInfor)) { }
}

public class TournamentScoreInfor
{
	/// <summary>
	/// 写入的时间
	/// </summary>
	public DateTime WriteTime = DateTime.MinValue;

	/// <summary>
	/// 这次锦标赛剩下的时间
	/// </summary>
	public float LastTime = 0;

	/// <summary>
	/// 总分数
	/// </summary>
	public ulong AllScore = 0;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ES2_JackpotData : ES2Type {

	public ES2_JackpotData():base(typeof(JackpotData)){}

	public override void Write(object obj, ES2Writer writer)
	{
		JackpotData data = (JackpotData)obj;
		writer.Write (data.CurrentBonus);
		writer.Write (data.NextBonus);
	}

	public override object Read(ES2Reader reader)
	{
		JackpotData data = new JackpotData();
		data.CurrentBonus = reader.Read<int> ();
		data.NextBonus = reader.Read<int> ();

		return data;
	}
}

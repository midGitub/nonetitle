using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ES2_DateTime : ES2Type{
	
	public ES2_DateTime():base(typeof(System.DateTime)){}

	public override void Write(object obj, ES2Writer writer)
	{
		System.DateTime data = (System.DateTime)obj;

		#if true
		writer.Write(data.ToString("yyyy-MM-dd HH:mm:ss"));
		#else
		writer.Write((System.Int32)data.Year);
		writer.Write((System.Int32)data.Month);
		writer.Write((System.Int32)data.Day); 
		writer.Write((System.Int32)data.Hour);
		writer.Write((System.Int32)data.Minute);
		writer.Write((System.Int32)data.Second);
		#endif
	}

	public override object Read(ES2Reader reader)
	{
		#if true
		string date = reader.Read<System.String>();
		DateTime realDate = Convert.ToDateTime(date);
		return realDate;
		#else
		int year = reader.Read<System.Int32> ();
		int month = reader.Read<System.Int32> ();
		int day = reader.Read<System.Int32> ();
		int hour = reader.Read<System.Int32> ();
		int minute = reader.Read<System.Int32> ();
		int second = reader.Read<System.Int32> ();

		return new System.DateTime(year, month, day, hour, minute, second);
		#endif
	}
}
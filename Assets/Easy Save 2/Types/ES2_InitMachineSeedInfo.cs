using UnityEngine;
using System.Collections;

public class ES2_InitMachineSeedInfo : ES2Type
{
	public ES2_InitMachineSeedInfo():base(typeof(InitMachineSeedInfo)){}

	public override void Write(object obj, ES2Writer writer)
	{
		InitMachineSeedInfo data = (InitMachineSeedInfo)obj;
		writer.Write (data._seed);
		writer.Write (data._isFromServer);
	}

	public override object Read(ES2Reader reader)
	{
		InitMachineSeedInfo data = new InitMachineSeedInfo(0, false);
		data._seed = reader.Read<uint> ();
		data._isFromServer = reader.Read<bool> ();

		return data;
	}
}


using System.Collections;
using System.Collections.Generic;
using System;

public partial class UserDeviceLocalData : UserDataBase
{
	static public string Name = "UserDeviceLocalData";

	static private UserDeviceLocalData _instance = null;
	static public UserDeviceLocalData Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new UserDeviceLocalData();
				_instance.Init();
			}
			return _instance;
		}
	}

	static private string _lastExitTimeTag = "lastExitTime";
	static private string _jackpotPointTag = "jackpotPoint";

	private DateTime _lastExitTime = System.DateTime.MinValue;
	private float _jackpotPoint = 0.0f;

	public DateTime LastExitTime
	{
		get { return _lastExitTime; }
		set { _lastExitTime = value; }
	}

	public float JackpotPoint{
		get { return _jackpotPoint; }
		set { _jackpotPoint = value; }
	}

	#region Init

	protected UserDeviceLocalData()
	{
	}

	private void Init()
	{
	}

	protected override string GetFileName()
	{
		return Name;
	}

	#endregion

	#region Read Write

	public override void Save()
	{
		base.Save();

		CoreDebugUtility.Log("### UserDeviceLocalData.Save() ###");
	}

	private void ReadCore(ES2Reader reader)
	{
		_lastExitTime = IsTagExist(_lastExitTimeTag) ?
			reader.Read<System.DateTime>(_lastExitTimeTag) : System.DateTime.MinValue;

		_jackpotPoint = IsTagExist (_jackpotPointTag) ?
			reader.Read<float> (_jackpotPointTag) : 0.0f;
	}

	private void WriteCore(ES2Writer writer)
	{
		writer.Write(_lastExitTime, _lastExitTimeTag);
		writer.Write (_jackpotPoint, _jackpotPointTag);
	}

	#endregion
}

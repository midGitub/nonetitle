using System.Collections;
using System.Collections.Generic;

//This class stores temporary user datas which don't need to be serialized
public partial class UserTempData
{
	static private UserTempData _instance = null;
	static public UserTempData Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new UserTempData();
				_instance.Init();
			}
			return _instance;
		}
	}

	private ulong _lastBetAmount;
	private bool _isFirstShortLucky = true;
	public ulong LastBetAmount { get { return _lastBetAmount; } set { _lastBetAmount = value; } }
	public bool IsFirstShortLucky { get { return _isFirstShortLucky; } set { _isFirstShortLucky = value; } }

	protected UserTempData()
	{
	}

	private void Init()
	{
	}
}

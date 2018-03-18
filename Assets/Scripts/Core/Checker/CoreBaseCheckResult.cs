using System.Collections;
using System.Collections.Generic;

public class CoreBaseCheckResult
{
	protected SpinResultType _type = SpinResultType.None;

	public SpinResultType Type { get { return _type; } set { _type = value; } }

	public CoreBaseCheckResult()
	{
		_type = SpinResultType.None;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class CSVData
{
	public CSVData()
	{
	}

	public virtual bool Parse(string[] items)
	{
		return false;
	}
}

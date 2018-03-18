using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;

public class MemoryManager : Singleton<MemoryManager>
{
	public void MemoryWarning()
	{
		//todo
		Debug.Log("MemoryWarning called");
	}
}


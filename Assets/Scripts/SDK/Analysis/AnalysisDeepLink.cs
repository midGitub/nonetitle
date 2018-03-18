using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalysisDeepLink : MonoBehaviour
{
	public void DeepLink(string url)
	{
		AnalysisManager.Instance.DeepLink(url);
	}
}

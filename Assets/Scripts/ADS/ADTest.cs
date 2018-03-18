using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class ADTest : Singleton<ADTest>
{

	//void OnEnable()
	//{
	//	FyberCallback.AnalysisSucceeded += OnAnalysisSucceeded;
	//	FyberCallback.AnalysisFailed += OnAnalysisFailed;
	//}

	//void OnDisable()
	//{
	//	FyberCallback.AnalysisSucceeded -= OnAnalysisSucceeded;
	//	FyberCallback.AnalysisFailed -= OnAnalysisFailed;
	//}

	////[...]

	//public void OnAnalysisSucceeded(IntegrationReport integrationReport)
	//{
	//	// handle integration report
	//	LogUtility.Log("广告分析成功");
	//	List<MediationBundleInfo> list = integrationReport.StartedBundles;
	//	foreach(var item in list)
	//	{
	//		Debug.Log("开启的");
	//		Debug.Log(item.Name);
	//		Debug.Log(item.BundleError);
	//	}
	//	list = integrationReport.UnstartedBundles;
	//	foreach(var item in list)
	//	{
	//		Debug.Log("未开启的");
	//		Debug.Log(item.Name);
	//		Debug.Log(item.BundleError);
	//	}
	//}

	//public void OnAnalysisFailed(AnalysisError? analysisError)
	//{
	//	// handle the error
	//	LogUtility.Log("广告分析失败", Color.red);
	//}

	//public void StartTest()
	//{
	//	IntegrationAnalyzer.ShowTestSuite();
	//}

	//public void InGameTest()
	//{
	//	IntegrationAnalyzer.Analyze();
	//}
}

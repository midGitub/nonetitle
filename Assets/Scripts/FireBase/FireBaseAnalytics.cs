using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;

public class FireBaseAnalytics :SimpleSingleton <FireBaseAnalytics>
{
	private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
	public void Init() 
	{
		dependencyStatus = FirebaseApp.CheckDependencies();
		if (dependencyStatus != DependencyStatus.Available) 
		{
			FirebaseApp.FixDependenciesAsync().ContinueWith(task => {
				dependencyStatus = FirebaseApp.CheckDependencies();
				if (dependencyStatus == DependencyStatus.Available) 
				{
					InitializeFirebase();
				}
				else 
				{
					Debug.LogError(
						"Could not resolve all Firebase dependencies: " + dependencyStatus);
				}
			});
		}
		else 
		{
			InitializeFirebase();
		}
	}

	void InitializeFirebase() 
	{
		FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
		if(!UserBasicData.Instance.UDID.IsNullOrEmpty())
			FirebaseAnalytics.SetUserId(UserBasicData.Instance.UDID);
	}

	public void TrackEvent(Dictionary<string, object> parm)
	{
		if (dependencyStatus == DependencyStatus.Available) 
		{
//			Debug.Log ("FireBaseAnalytics.sentproperty");
			foreach (KeyValuePair<string,object> pair in parm) 
			{
				
				if ((!pair.Key.IsNullOrEmpty ()) && pair.Value != null && (!pair.Value.ToString().IsNullOrEmpty())) 
				{
//					Debug.Log ("FireBaseAnalytics.sentproperty" + pair.Key + ":" + pair.Value.ToString ());
					Firebase.Analytics.FirebaseAnalytics.SetUserProperty (pair.Key, pair.Value.ToString ());
				}
					
			}
		} 
		else
		{
			Debug.Log ("FireBaseAnalytics.sentproperty do not success because direbase init failed or have not init yet");
		}
	}
}

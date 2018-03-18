using System;
using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using Firebase.Analytics;
public class FireBaseCloudMessage : SimpleSingleton<FireBaseCloudMessage>
{
	private DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;		
	public void Init() 
	{
		dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
		if (dependencyStatus != Firebase.DependencyStatus.Available) 
		{
			Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(task => 
				{
					dependencyStatus = Firebase.FirebaseApp.CheckDependencies();
					if (dependencyStatus == Firebase.DependencyStatus.Available)
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
//		Debug.Log ("Message Init");
		FirebaseMessaging.MessageReceived += OnMessageReceived;
		FirebaseMessaging.TokenReceived += OnTokenReceived;
		#if DEBUG
		FirebaseMessaging.Subscribe("Debug");
		#endif
	}

	private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) 
	{
//		Debug.Log ("Message Received");
		if (e.Message.NotificationOpened) 
		{
//			Debug.Log ("message.NotificationOpened!");
			if (e.Message.Notification != null) 
			{
				string[] stringtosend = e.Message.Notification.Body.ToString ().Split (' ');
				string result = null;
				for (int i = 0; i < 3 && i < stringtosend.Length; i++) 
				{
					result = result + stringtosend [i] + " ";
				}
				AnalysisManager.Instance.RemotePushReceived("Remote",result);
			}
		}
	}

	private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) 
	{
		//AnalysisManager.Instance.SendDeviceToken(token.Token);
		Debug.Log ("Token Received : " + token.Token);
	}
		
	private void OnDestroy() 
	{
		FirebaseMessaging.MessageReceived -= OnMessageReceived;
		FirebaseMessaging.TokenReceived -= OnTokenReceived;
	}
}


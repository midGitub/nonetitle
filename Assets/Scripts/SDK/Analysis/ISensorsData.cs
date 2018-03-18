using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ISensorsData
{
	void InitSensorsData(string serverurl, string configurl, bool isdebug);
	void SetUserId(string udid);
	void TrackEvent(string eventName, Dictionary<string, object> parm);
	void TrackEvent(string eventName);
	void SetGameInfo(string channel, string codename);
	void SetProfile(string name, string value);
	void SetDicProfile(Dictionary<string, object> parm);
	void TrackInstallation();
}


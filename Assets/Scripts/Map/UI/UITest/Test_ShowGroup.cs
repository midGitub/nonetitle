using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_ShowGroup : MonoBehaviour {
	public Text GroupText;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		GroupMember member = new GroupMember ();
		GroupText.text = "recency: " + member.LastPaytime.ToString() + " frequency: " + member.PayCount.ToString() + "\n"
		+ " monetary: " + member.PayAmount.ToString() + "maxPaid: " + member.HistoryMaxPay.ToString() + " group: " + UserGroup.GetUserGroupID().ToString() + "\n"
		+ " Login: " + "session: " + member.SessionAverage.ToString() + "\n"
		+ " Spin: " + member.SpinAverage.ToString() + "active: " + GroupConfig.Instance.GetActiveID();;



	}
}

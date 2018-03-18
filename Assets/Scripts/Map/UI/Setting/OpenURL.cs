using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour 
{
	public string URL = "http://citrusjoy.com/pp.html";

	public void Open()
	{
        Application.OpenURL(PackageConfigManager.Instance.CurPackageConfig.PrivacyPolicyUrl);
	}
}

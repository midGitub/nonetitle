using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMultiLineBack : MonoBehaviour
{
	private PuzzleMachine _machine;
	private MachineConfig _machineConfig;

	private int _paylineCount;
	private List<Image[]> _paylineImages = new List<Image[]>();

	#region Init

	public void Init(PuzzleMachine machine)
	{
		_machine = machine;
		_machineConfig = _machine.MachineConfig;
		_paylineCount = _machineConfig.PaylineConfig.PaylineCount;

		InitPaylineImages();
		HideAllPayline();
	}

	void InitPaylineImages()
	{
		for(int i = 1; i <= _paylineCount; i++)
		{
			string s = "Payline" + i.ToString();
			Transform t = gameObject.transform.FindChild(s);
			Image[] image = t.gameObject.GetComponentsInChildren<Image>(true);
			_paylineImages.Add(image);
		}
	}

	#endregion

	public void HideAllPayline()
	{
		ListUtility.ForEach(_paylineImages, (Image[] image) => {
			ListUtility.ForEach(image, (Image i)=>{
				i.enabled = false;
			});
		});
	}

	public void ShowPayline(MultiLineMatchInfo info)
	{
		int paylineIndex = _machine.MachineConfig.PaylineConfig.GetPaylineIndex(info.Payline);
		if(paylineIndex >= 0)
		{
			ShowSinglePayline(paylineIndex);
		}
		else
		{
			Debug.Assert(false);
		}
	}

	private void ShowSinglePayline(int index)
	{
		ListUtility.ForEach(_paylineImages[index], (Image i)=>{
			i.enabled = true;
		});
	}
}

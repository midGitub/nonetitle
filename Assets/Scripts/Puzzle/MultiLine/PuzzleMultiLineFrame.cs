using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMultiLineFrame : MonoBehaviour
{
	private PuzzleMachine _machine;
	private MachineConfig _machineConfig;

	private int _paylineCount;
	private List<Image[]> _paylineImages = new List<Image[]>();
	private List<Image> _paylineNumberImages = new List<Image>();

	#region Init

	public void Init(PuzzleMachine machine)
	{
		_machine = machine;
		_machineConfig = _machine.MachineConfig;
		_paylineCount = _machineConfig.PaylineConfig.PaylineCount;

		if(ShouldShowPaylines())
		{
			InitPaylineImages();
			InitPaylineNumberImages();
		}
	}

	bool ShouldShowPaylines()
	{
		return !_machineConfig.BasicConfig.IsFiveReel;
	}

	void InitPaylineImages()
	{
		_paylineImages.Add(new Image[] {_machine._reelFrame._payLine });
		for(int i = 2; i <= _paylineCount; i++)
		{
			string s = "Payline" + i.ToString();
			Transform t = gameObject.transform.FindDeepChild(s);
			Image[] images = t.gameObject.GetComponentsInChildren<Image>(true);
			_paylineImages.Add(images);
		}

		ListUtility.ForEach(_paylineImages, (Image[] image) => {
			ListUtility.ForEach(image, (Image i) => {
				i.sprite = _machine._reelFrame._payLine.sprite;
				i.color = new Color(i.color.r, i.color.g, i.color.b, 0.5f);
			});
		});

		//todo: replace image in different machine
	}

	void InitPaylineNumberImages()
	{
		string numberPrefix = _machineConfig.BasicConfig.MultiLinePayLineNumber;

		for(int i = 1; i <= _paylineCount; i++)
		{
			string s = "PaylineNumber" + i.ToString();
			Transform t = gameObject.transform.FindChild(s);
			Image image = t.gameObject.GetComponent<Image>();
			_paylineNumberImages.Add(image);

			//replace image in different machine
			if(!string.IsNullOrEmpty(numberPrefix))
			{
				string name = numberPrefix + i.ToString();
				Sprite sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(name, _machine.MachineName);
				Debug.Assert(sprite != null);
				image.sprite = sprite;
				image.SetNativeSize();
			}
		}
	}

	#endregion
}



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MapMachineDownloader : MonoBehaviour
{
	public Image _downloadIcon;
	public Image _mask;
	public Image _circleBack;
	public Image _circleProgress;
	public Text _loadingText;

	MapMachineController _controller;
	private string _machineName;
	private bool _isDownloading;

	public bool IsDownloading { get { return _isDownloading; } }

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
	
	}

	public void Init(string machineName, MapMachineController controller)
	{
		_machineName = machineName;
		_controller = controller;

		_downloadIcon.enabled = false;
		_mask.enabled = false;
		_circleBack.enabled = false;
		_circleProgress.enabled = false;
		_loadingText.enabled = false;

		RefreshDownloadIcon();

		if(ShouldDownload())
			MachineAssetDownloaderManager.Instance.InitDownloader(_machineName);
	}

	public void OnDestroy()
	{
		if(MachineAssetDownloaderManager.Instance != null && !string.IsNullOrEmpty(_machineName))
			MachineAssetDownloaderManager.Instance.UnregisterDownloadCallbacks(_machineName);
	}

	public void StartDownload(MachineAssetDownloadCompleteDelegate onSuccess, MachineAssetDownloadCompleteDelegate onFail)
	{
		StartDownloadEffect();

		_isDownloading = true;

		MachineAssetDownloaderManager.Instance.StartDownloadMachineAsset(_machineName,
			DownloadUpdateCallback,
			(string machineName) => {
				EndDownloadCallback();
				if(onSuccess != null)
					onSuccess(_machineName);
			},
			(string machineName) => {
				EndDownloadCallback();
				if(onFail != null)
					onFail(_machineName);
			}
		);
	}

	public bool ShouldDownload()
	{
		bool result = false;
		if(MachineAssetManager.Instance.IsRemoteAssetMachine(_machineName))
		{
			result = !MachineAssetManager.Instance.IsMachineDownloadedAndMatched(_machineName);
		}
		return result;
	}

	private void StartDownloadEffect()
	{
		_mask.enabled = true;
		_circleBack.enabled = true;
		_circleProgress.enabled = true;
		_circleProgress.fillAmount = 0.0f;
		_loadingText.enabled = true;
	}

	private void EndDownloadEffect()
	{
		_mask.enabled = false;
		_circleBack.enabled = false;
		_circleProgress.enabled = false;
		_circleProgress.fillAmount = 0.0f;
		_loadingText.enabled = false;
	}

	public void RefreshDownloadIcon()
	{
		_downloadIcon.enabled = ShouldDownload() && _controller.IsUnlock;
	}

	#region Download callback

	private void DownloadUpdateCallback(string machineName, float percent)
	{
		_circleProgress.fillAmount = percent;
	}

	private void EndDownloadCallback()
	{
		_isDownloading = false;

		EndDownloadEffect();

		RefreshDownloadIcon();
	}

	#endregion
}


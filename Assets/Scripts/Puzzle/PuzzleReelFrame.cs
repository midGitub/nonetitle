using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleReelFrame : MonoBehaviour
{
	private static readonly string _imageDir = "Images/Machines/";

	private static readonly string _effectPrefabPath = "Effect/Prefab/";

	public List<PuzzleReel> _reelList;
	public Image _reelBackLeft;
	public Image _reelBackRight;
	public Image _reelBack4;
	public Image _payLine;
	public Image _highLight;
	public Image[] _reelShadowUp;
	public Image[] _reelShadowUpSimple;
	public Image[] _mirrorReelShadowUpSimple;
	public Image[] _reelShadowDown;
	public Image _singleReelShadowUp;
	public Image _singleReelShadowUpSimple;
	public Image _singleReelShadowDownSimple;
	public Image _singleReelShadowDown;
	public Image _reelBackLeftFrame;
	public Image _reelBackRightFrame;
	public GameObject _multiLineBackParent;
	public GameObject _collectSlider;// 收集槽
	public GameObject _jackpotScore;// jackpot分数板
	public GameObject _widgetEffect;// 小物件特效节点，收集物等用
	public GameObject _backgroundDecorate;// 背景装饰

	public float _originalReelBackHeight;
	public float _currentReelBackHeight;
	public float _reelBackHeightFactor;

	// 收集物滑动条
	public PuzzleSlider _puzzleSlider;

	private MachineTheme _machineTheme;
	private PuzzleMachine _puzzleMachine;
	private MachineConfig _machineConfig;

	// jackpot tips
	private JackpotTipBehaviour _jackpotTip;
	// jackpot check 
	private JackpotCheckBehaviour _jackpotCheck;


	public JackpotTipBehaviour JackpotTip{
		get { return _jackpotTip; }
	}

	public JackpotCheckBehaviour JackpotCheck{
		get { return _jackpotCheck; }
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Init(PuzzleMachine machine)
	{
		_puzzleMachine = machine;
		_originalReelBackHeight = _reelBackLeft.sprite.rect.height;
		_machineTheme = CoreDefine.MachineThemeDict[machine.MachineName];
		_machineConfig = machine.MachineConfig;

		InitReelBack (machine);
		InitPayline (machine);
		InitHighLight (machine);
		InitReelShadow (machine);
		InitSpecialMachineUI (machine);
		InitReelOffsets (machine);
		InitBackgroundDecorate (machine);
	}

	private void InitDecorateSprite(PuzzleMachine machine){
		string path = _imageDir + machine.CoreMachine.MachineConfig.BasicConfig.BackgroundDecorate;
		Sprite spr = AssetManager.Instance.LoadMachineAsset<Sprite> (path, machine.MachineName);

		Image img = _backgroundDecorate.GetComponent<Image> ();
		if (spr != null){
			img.enabled = true;
			img.sprite = spr;
			img.SetNativeSize ();
		}

		UpdateDecoratePos (machine);
	}

	private void InitDecoratePrefab(PuzzleMachine machine){
		UGUIUtility.CreateMachineAsset(machine.CoreMachine.MachineConfig.BasicConfig.BackgroundDecoratePrefab, 
			machine.MachineName, _backgroundDecorate);
		UpdateDecoratePos (machine);
	}

	private void UpdateDecoratePos(PuzzleMachine machine){
		if (machine.CoreMachine.MachineConfig.BasicConfig.BackgroudDecorateOffsets.Length > 0) {
			float[] offsets = machine.CoreMachine.MachineConfig.BasicConfig.BackgroudDecorateOffsets;
			Vector3 vec = _backgroundDecorate.transform.localPosition + new Vector3 (offsets [0], offsets [1], 0.0f);
			_backgroundDecorate.transform.localPosition = vec;
		}
	}

	private void InitBackgroundDecorate(PuzzleMachine machine){
		if (_backgroundDecorate != null) {
			_backgroundDecorate.SetActive (true);

			if (!machine.CoreMachine.MachineConfig.BasicConfig.BackgroundDecorate.IsNullOrEmpty ()) {
				InitDecorateSprite (machine);
			} else if (!machine.CoreMachine.MachineConfig.BasicConfig.BackgroundDecoratePrefab.IsNullOrEmpty ()){
				InitDecoratePrefab (machine);
			} else {
				_backgroundDecorate.SetActive (false);
			}
		}
	}

	private void InitReelOffsets(PuzzleMachine machine){
		UpdateReelEffectOffset (machine, PuzzleDefine.ReelEffectType.NormalWin);
		UpdateReelEffectOffset (machine, PuzzleDefine.ReelEffectType.BigWin);
		UpdateReelEffectOffset (machine, PuzzleDefine.ReelEffectType.Special);
	}

	private void UpdateSliceRecttransform(Image image, Sprite refSpr){
		RectTransform rect = image.rectTransform;
		Vector2 vec = rect.sizeDelta;
		rect.sizeDelta = new Vector2(vec.x, refSpr.rect.height);
	}

	private void InitReelBack(PuzzleMachine machine){
		string reelBackPath = _imageDir + machine.CoreMachine.MachineConfig.BasicConfig.ReelSkin;
		Sprite reelSprite = AssetManager.Instance.LoadMachineAsset<Sprite>(reelBackPath, machine.MachineName);
		_reelBackLeft.sprite = reelSprite;
		_reelBackRight.sprite = reelSprite;

		if (_puzzleMachine.MachineConfig.BasicConfig.IsReelBackSlice){
			_reelBackLeft.type = Image.Type.Sliced;
			_reelBackRight.type = Image.Type.Sliced;
			UpdateSliceRecttransform(_reelBackLeft, reelSprite);
			UpdateSliceRecttransform(_reelBackRight, reelSprite);
		}else{
			_reelBackLeft.SetNativeSize();
			_reelBackRight.SetNativeSize();
		}

		if(_puzzleMachine.MachineConfig.BasicConfig.IsFourReel)
		{
			//Note: just add a suffix instead of a new configure. This follows "convention over configuration"
			string path4 = _imageDir + machine.CoreMachine.MachineConfig.BasicConfig.ReelSkin + "4";
			Sprite sprite4 = AssetManager.Instance.LoadMachineAsset<Sprite>(path4, machine.MachineName);
			_reelBack4.sprite = sprite4;
			_reelBack4.SetNativeSize();
		}

		_currentReelBackHeight = reelSprite.rect.height;
		_reelBackHeightFactor = _currentReelBackHeight / _originalReelBackHeight;

		reelBackPath = machine.CoreMachine.MachineConfig.BasicConfig.ReelSkinFrame;
		if (!reelBackPath.IsNullOrEmpty ()) {
			reelBackPath = _imageDir + reelBackPath;
			Sprite spr = AssetManager.Instance.LoadMachineAsset<Sprite> (reelBackPath, machine.MachineName);
			_reelBackLeftFrame.sprite = spr;
			_reelBackRightFrame.sprite = spr;

			if (_puzzleMachine.MachineConfig.BasicConfig.IsReelBackSlice){
				_reelBackLeftFrame.type = Image.Type.Sliced;
				_reelBackRightFrame.type = Image.Type.Sliced;
				UpdateSliceRecttransform(_reelBackLeftFrame, spr);
				UpdateSliceRecttransform(_reelBackRightFrame, spr);
			}else {
				_reelBackLeftFrame.SetNativeSize ();
				_reelBackRightFrame.SetNativeSize ();
			}
			_reelBackLeftFrame.gameObject.SetActive (true);
			_reelBackRightFrame.gameObject.SetActive (true);
		} else {
			_reelBackLeftFrame.gameObject.SetActive (false);
			_reelBackRightFrame.gameObject.SetActive (false);
		}
	}

	private void InitPayline(PuzzleMachine machine)
	{
		// don't show payline for 5 reels machine
		if(_machineConfig.BasicConfig.IsFiveReel)
		{
			_payLine.gameObject.SetActive(false);
		}
		else
		{
			if (_machineConfig.BasicConfig.PayLine.IsNullOrEmpty ()) {
				_payLine.gameObject.SetActive (false);
			} else {
				string payLinePath = _imageDir + _machineConfig.BasicConfig.PayLine;
				_payLine.sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(payLinePath, machine.MachineName);

				float[] rectPosArray = _machineConfig.BasicConfig.PayLineRectPos;
				if (rectPosArray.Length > 0) {
					RectTransform rect = _payLine.GetComponent<RectTransform> ();
					UpdatePayLineRectPos (rect, rectPosArray);
				}
			}
		}
	}

	private void InitHighLight(PuzzleMachine machine){
		if (!string.IsNullOrEmpty (machine.CoreMachine.MachineConfig.BasicConfig.HighLight)) {
			string highLightPath = _imageDir + machine.CoreMachine.MachineConfig.BasicConfig.HighLight;
			_highLight.gameObject.SetActive(true);
			Sprite spr = AssetManager.Instance.LoadMachineAsset<Sprite> (highLightPath, machine.MachineName);
			if (spr == null) {
				LogUtility.Log ("InitHighLight spr is null path = " + highLightPath, Color.red);
				_highLight.gameObject.SetActive (false);
			} else {
				_highLight.sprite = spr;
			}
		} else {
			_highLight.gameObject.SetActive (false);
		}

		if (machine.CoreMachine.MachineConfig.BasicConfig.HighLightLocalPos.Length > 0) {
			float[] pos = machine.CoreMachine.MachineConfig.BasicConfig.HighLightLocalPos;
			_highLight.transform.localPosition = new Vector3 (pos[0], pos[1], pos[2]);
		}
	}

	private Image[] GetReelShadowImage(int reelCount, Image[] imgs){
		Image[] shadowImgs;
		if (_puzzleMachine.MachineConfig.BasicConfig.IsFourReel)
			shadowImgs = new Image[]{imgs[0], imgs[1], imgs[2]};
		else
			shadowImgs = imgs;
		return shadowImgs;
	}

	private void InitReelShadow(PuzzleMachine machine){
		BasicConfig config = machine.MachineConfig.BasicConfig;

		bool isShow = !config.SingleReelShadowUp.IsNullOrEmpty();
		InitReelShadow(machine, isShow, _singleReelShadowUp, config.SingleReelShadowUp, config.ReelShadowUpProperty, false);

		isShow = !config.ReelShadowUp.IsNullOrEmpty();
		Image[] shadowUpImgs = GetReelShadowImage(config.ReelCount, _reelShadowUp);
		InitReelShadow(machine, isShow, shadowUpImgs, config.ReelShadowUp, config.ReelShadowUpProperty, false);

		isShow = !config.SingleReelShadowDown.IsNullOrEmpty();
		InitReelShadow(machine, isShow, _singleReelShadowDown, config.SingleReelShadowDown, config.ReelShadowDownProperty, false);

		isShow = !config.ReelShadowDown.IsNullOrEmpty();
		Image[] shadowDownImgs = GetReelShadowImage(config.ReelCount, _reelShadowDown);
		InitReelShadow(machine, isShow, shadowDownImgs, config.ReelShadowDown, config.ReelShadowDownProperty, false);

		isShow = !config.ReelShadowUpSimple.IsNullOrEmpty();
		InitReelShadow(machine, isShow, _reelShadowUpSimple, config.ReelShadowUpSimple, config.ReelShadowUpProperty, true);

		isShow = !config.SingleReelShadowUpSimple.IsNullOrEmpty();
		InitReelShadow(machine, isShow, _singleReelShadowUpSimple, config.SingleReelShadowUpSimple, config.ReelShadowUpProperty, true);

		isShow = !config.SingleReelShadowDownSimple.IsNullOrEmpty();
		InitReelShadow(machine, isShow, _singleReelShadowDownSimple, config.SingleReelShadowDownSimple, config.ReelShadowDownProperty, true);

		isShow = !config.MirrorReelShadowUpSimple.IsNullOrEmpty();
		InitReelShadow(machine, isShow, _mirrorReelShadowUpSimple, config.MirrorReelShadowUpSimple, config.ReelShadowUpProperty, true);
		UpdateSortOrder(machine, _mirrorReelShadowUpSimple, config.MirrorReelShadowUpSortOrder);

		if (machine.MachineConfig.BasicConfig.NoReelShadow){
			ListUtility.ForEach(shadowUpImgs, (Image img)=>{
				img.enabled = false;
			});
			ListUtility.ForEach(shadowDownImgs, (Image img)=>{
				img.enabled = false;
			});
		}
	}

	private void InitReelShadow(PuzzleMachine machine, bool isShow, Image image, string path, string property, bool isSimple){
		if (image != null){
			image.enabled = isShow;
			if (isShow){
				analyzeReelShadow(machine, image, path, property);
				if (isSimple){
					image.SetNativeSize();
				}
			}
		}
	}

	private void UpdateSortOrder(PuzzleMachine machine, Image[] imgs, int sortOrder){
		if (sortOrder > 0){
			Transform parent = imgs[0].transform.parent;
			Canvas canvas = parent.gameObject.AddComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = sortOrder;
		}
	}

	private void InitReelShadow(PuzzleMachine machine, bool isShow, Image[] images, string path, string property, bool isSimple){
		ListUtility.ForEach(images, (Image img)=>{
			InitReelShadow(machine, isShow, img, path, property, isSimple);
		});
	}

	private void analyzeReelShadow(PuzzleMachine machine, Image image, string sprName, string property){
		if (sprName == "") {
			LogUtility.Log ("Reel shadow sprname is null,  " + machine.CoreMachine.MachineConfig.Name, Color.red);
			return;
		}

		Sprite reelShadow = AssetManager.Instance.LoadMachineAsset<Sprite> (_imageDir + sprName, machine.MachineName);
		//LogUtility.Log ("Reel shadow sprname is "+ sprName, Color.green);
		if (reelShadow != null) {
			image.sprite = reelShadow;
		}

		if (property == "") {
			LogUtility.Log ("Reel shadow property is null,  " + machine.CoreMachine.MachineConfig.Name, Color.red);
			return;
		}

		//LogUtility.Log (" reel shadow property is " + property, Color.green);

		string[] parameters = property.Split(';');
		List<float> paramList = new List<float> ();
		paramList = ListUtility.MapList<string, float> (parameters, (x)=>{
			return float.Parse(x);
		});

		shadowPropertyAnalyze(image, paramList);
	}

	private void shadowPropertyAnalyze(Image img, List<float> paramList){
		
		#if true
		if (paramList.Count >4){
			// left, posy, posz, right, height
			RectTransform trans = img.rectTransform;

			Vector3 vec = trans.anchoredPosition3D;

//			Vector2 offsetMin = trans.offsetMin;
//			trans.offsetMin = new Vector2 (paramList[0 + offset], offsetMin.y);
//			Vector2 offsetMax = trans.offsetMax;
//			trans.offsetMax = new Vector2 (-paramList[3 + offset], offsetMax.y);

			Vector2 sizeDelta = trans.sizeDelta;
			// 物体的宽和高
			trans.sizeDelta = new Vector2(paramList.Count > 5 ? paramList[5] * sizeDelta.x : sizeDelta.x, paramList[4]);

			Vector2 offsetMin = trans.offsetMin;
			Vector2 offsetMax = trans.offsetMax;
			if (paramList.Count > 6){
				// 物体y位置
				trans.anchoredPosition3D = new Vector3 (vec.x, paramList[6], vec.z);
			}
			if (paramList.Count > 7){
				
			}
		}
		#endif

		Color color = new Color (paramList[0], paramList[1], paramList[2], paramList[3]);
		img.color = color;
	}

	private void UpdateReelEffectOffset(PuzzleMachine machine, PuzzleDefine.ReelEffectType type){
		float[] offsets = new float[0];

		if (type == PuzzleDefine.ReelEffectType.NormalWin) {
			offsets = machine.CoreMachine.MachineConfig.BasicConfig.ReelNormalWinEffectOffsets;
		} else if (type == PuzzleDefine.ReelEffectType.BigWin) {
			offsets = machine.CoreMachine.MachineConfig.BasicConfig.ReelBigWinEffectOffsets;
		} else if (type == PuzzleDefine.ReelEffectType.Special) {
			offsets = machine.CoreMachine.MachineConfig.BasicConfig.ReelSpecialEffectOffsets;
		}

		if (offsets.Length == 0)
			return;

		for (int i = 0; i < _reelList.Count; ++i) {
			PuzzleReel reel = _reelList [i];
			Vector2 offsetPos = new Vector2 (offsets[2 * i], offsets[2 * i + 1]);
			reel.UpdateEffectParentOffset (offsetPos, type);
		}
	}

	private void UpdatePayLineRectPos(RectTransform rect, float[] posArray){
		Vector3 vec = rect.anchoredPosition3D;
		rect.anchoredPosition3D = new Vector3 (vec.x, posArray[1], vec.z);

		Vector2 offsetMin = rect.offsetMin;
		rect.offsetMin = new Vector2 (posArray[0], offsetMin.y);
		Vector2 offsetMax = rect.offsetMax;
		rect.offsetMax = new Vector2 (-posArray[2], offsetMax.y);

		Vector2 sizeDelta = rect.sizeDelta;
		rect.sizeDelta = new Vector2(sizeDelta.x, posArray[3]);
	}

	private void UpdateReelFramePos(PuzzleMachine machine){
		if (machine.MachineConfig.BasicConfig.ReelFrameOffsets.Length > 0){
			float[] offsets = machine.MachineConfig.BasicConfig.ReelFrameOffsets;
			Debug.Assert(offsets.Length > 2);
			Vector3 pos = gameObject.transform.localPosition + new Vector3 (offsets[0], offsets[1], offsets[2]);
			gameObject.transform.localPosition = pos;
		}
	}

	private void InitSpecialMachineUI(PuzzleMachine machine){
		_puzzleSlider = new PuzzleSlider(machine, _collectSlider);
		InitJackpotPool(machine);
		UpdateReelFramePos(machine);
		InitReelFrameTopBoard(machine);
	}

	#region slider
	public void SetCollectSliderValue(float value){
		if (_puzzleSlider != null){
			_puzzleSlider.SetCollectSliderValue(value);
		}
	}

	public void UpdateCollectSliderValue(){
		if (_puzzleSlider != null){
			_puzzleSlider.UpdateCollectSliderValue();
		}
	}

	public GameObject GetLocationObjFromCurrentValue(int offsetIndex){
		if (_puzzleSlider != null){
			return _puzzleSlider.GetLocationObjFromCurrentValue(offsetIndex);
		}
		return null;
	}

	public void ShowSliderEffect(CollectEffectType type, bool isShow){
		if (_puzzleSlider != null){
			_puzzleSlider.ShowSliderEffect(type, isShow);
		}
	}
	#endregion

	#region jackpot
	// jackpot
	private void UpdateJackpotPoolPos(PuzzleMachine machine, GameObject jackpotPool){
		if (machine.MachineConfig.BasicConfig.JackpotPoolOffsets.Length > 0){
			float[] offsets = machine.MachineConfig.BasicConfig.JackpotPoolOffsets;
			Debug.Assert(offsets.Length > 2);
			jackpotPool.transform.localPosition = new Vector3(offsets[0], offsets[1], offsets[2]);
		}
	}

	private void InitJackpotPool(PuzzleMachine machine){
		string jackpotScore = machine.CoreMachine.MachineConfig.BasicConfig.JackpotScore;
		if (!string.IsNullOrEmpty(jackpotScore)) {
			GameObject jackpotPool = UGUIUtility.CreateMachineAsset(jackpotScore, machine.MachineName, _jackpotScore);
			UpdateJackpotPoolPos(machine, jackpotPool);
			_jackpotCheck = jackpotPool.GetComponent<JackpotCheckBehaviour> ();
			InitJackpotSelect ();
			InitJackpotTips ();
		}
	}

	private void InitJackpotSelect(){
		BasicConfig config = _puzzleMachine.CoreMachine.MachineConfig.BasicConfig;
		ulong curBetAmount = _puzzleMachine.GameData.BetAmount;
		ulong minJackpotBet = config.JackpotMinBet;
		bool isSatisfiedJackpot = curBetAmount >= minJackpotBet;
		if (_jackpotCheck != null) {
			_jackpotCheck.EnableJackpot (isSatisfiedJackpot);
		}

		string path = config.JackpotSelect;
		if (path.IsNullOrEmpty ())
			return;
		
		if (isSatisfiedJackpot) 
			return;

		GameObject obj = AssetManager.Instance.LoadAsset<GameObject> (path);
		if (obj != null) {
			GameObject jackpotObj = UGUIUtility.CreateObj (obj, null);
			JackpotSelectBehaviour jackpotBehaviour = jackpotObj.GetComponent<JackpotSelectBehaviour> ();
			jackpotBehaviour.Init(_puzzleMachine.CoreMachine, _puzzleMachine);
		}
	}

	private void InitJackpotTips(){
		string path = _puzzleMachine.CoreMachine.MachineConfig.BasicConfig.JackpotTips;
		if (!path.IsNullOrEmpty ()) {
			GameObject obj = AssetManager.Instance.LoadAsset<GameObject> (path);
			if (obj != null) {
				GameObject jackpotObj = UGUIUtility.CreateObj (obj, _jackpotScore);
				_jackpotTip = jackpotObj.GetComponent<JackpotTipBehaviour> ();
				_jackpotTip.Init (_puzzleMachine.CoreMachine);
			}
		}
	}

	void InitReelFrameTopBoard(PuzzleMachine machine)
	{
		string path = machine.CoreMachine.MachineConfig.BasicConfig.ReelFrameTopBoard;
		if(!path.IsNullOrEmpty())
		{
			GameObject obj = UGUIUtility.CreateMachineAsset(path, machine.MachineName, _jackpotScore);
		}
	}

	#endregion
}

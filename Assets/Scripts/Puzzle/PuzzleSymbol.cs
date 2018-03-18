//#define DEBUG_SHOW_SYMBOL_WHEN_WIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using CitrusFramework;

public enum SymbolAnimStateType{
	Hint,
	Win,
	StartEvent,
}

public class PuzzleSymbol : MonoBehaviour
{
	private static readonly string _imageDir = "Images/Symbols/"; //todo
	private static readonly string _effectLightDir = "Effect/Prefab/"; //todo
	private static readonly string _effect3DDir = "Effect/Prefab/"; //todo
	private static readonly string _hypeDarkBackPath = "Game/SymbolHypeDarkBack"; //todo
	private static readonly string _blurMatRealTimeDir = "Shader/SpinBlurRealTime";

	private static readonly string _effectPrefabPath = "Effect/Prefab/";
	private static readonly string _effectAnimPath = "Effect/Anim/";

	// 替换symbol用的资源类
	internal class SwitchSymbolResourceInfo{
		private Sprite _sprite;
		private Sprite _blurSprite;
		private GameObject _winEffect;
		private string _machineName;
		private PuzzleSymbol _symbol;

		public Sprite ImageSprite{ get { return _sprite; } }
		public Sprite BlurSprite { get { return _blurSprite; } }
		public GameObject WinEffect { get { return _winEffect; } }

		public SwitchSymbolResourceInfo(PuzzleSymbol symbol, Sprite spr, Sprite blurSpr, GameObject effect){
			_sprite = spr;
			_blurSprite = blurSpr;
			_winEffect = effect;
			_symbol = symbol;
			_machineName = _symbol._reel.Machine.MachineName;
		}

		public SwitchSymbolResourceInfo(PuzzleSymbol symbol, SymbolData data){
			_symbol = symbol;
			_machineName = _symbol._reel.Machine.MachineName;

			string imgPath = data.ArtAsset;
			string effectPath = !data.WinEffect.IsNullOrEmpty() ? data.WinEffect : data.WinEffect3D;

			//img
			string filePath = _imageDir + imgPath;
			_sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(filePath, _machineName);

			// init blur img
			_blurSprite = _symbol._reel.Machine.ResourceManager.SymbolBlurSpriteDict[data.Name];

			// effect
			if(!string.IsNullOrEmpty(effectPath))
			{
				string path = _effect3DDir + effectPath;
				GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machineName);
				if(obj == null)
					Debug.LogError("SwitchSymbolResourceInfo Fail to load effect:" + path);
				_winEffect = Instantiate(obj, _symbol.transform);
				_winEffect.transform.localPosition = Vector3.zero;
				_winEffect.transform.localScale = Vector3.one;
				_winEffect.SetActive(false);
			}
		}

		public static SwitchSymbolResourceInfo CreateResourceInfo(PuzzleSymbol symbol, Sprite spr, Sprite blurSpr, GameObject effect){
			return new SwitchSymbolResourceInfo(symbol, spr, blurSpr, effect);
		}

		public static SwitchSymbolResourceInfo CreateResourceInfo(PuzzleSymbol symbol, SymbolData data){
			return new SwitchSymbolResourceInfo(symbol, data);
		}
	}

	public Image _image;
	public Image _blurImage;
	public Image _hypeBlurImage;
	public Animator _animator;

	public CoreSymbol _coreSymbol;
	// zhousen 新增的collect data
	private CoreCollectData _collectData;
	private CollectController _collectController;

	private PuzzleReel _reel;
	private GameObject _winEffectLight;
	private GameObject _winEffect3D;
	private GameObject _hypeDarkBack;
	private GameObject _hypeSymbolEffect;
	private Material _blurMatRealTime;
	private bool _isBlank;
	private Dictionary<string, SwitchSymbolResourceInfo> _switchSymbolDict = new Dictionary<string, SwitchSymbolResourceInfo>();// SYMBOL资源容奇， KEY是元素NAME，VALUE是资源结构体

	public CoreSymbol CoreSymbol { get { return _coreSymbol; } }

	public CoreCollectData CollectData{
		get { return _collectData; }
		set { _collectData = value; }
	}

	public CollectController CollectController {
		get { return _collectController; }
		set { _collectController = value; }
	}

	public delegate void SymbolHandler ();
	// 加入列表时的处理回调
	public SymbolHandler AddHandler = null;
	// 退出列表时的处理回调
	public SymbolHandler PopHandler = null;

	#region #region strong special machine

	// 强主题元素特殊动画
	private GameObject _strongSpecialEffect = null;
	// 强主题元素动画状态机
	private Animator _strongSpecialEffectAnimator = null;
	// 动画状态名和动画时长
	private string _hintStateName = "";
	private float _hintStateDelay = 0.0f;
	private AudioType _hintAudio = AudioType.None;
	private string _winStateName = "";
	private float _winStateDelay = 0.0f;
	private AudioType _winAudio = AudioType.None;
	private string _startEventStateName = "";
	private float _startEventStateDelay = 0.0f;
	private AudioType _startEventAudio = AudioType.None;
	// 是否有特殊强元素动画
	private bool _hasStrongSpecialEffect = false;

	public bool HasStrongSpecialEffect{
		get { return _hasStrongSpecialEffect; }
	}

	#endregion


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	#region Init

	public void Init(CoreSymbol coreSymbol, PuzzleReel reel)
	{
		_coreSymbol = coreSymbol;
		_reel = reel;
		_isBlank = _coreSymbol.SymbolData.SymbolType == SymbolType.Blank;

		InitImage();
		InitBlurImage();
		InitHypeBlurImage();
		InitWinEffect();
		InitHypeDarkEffect();
		InitBlurMatRealTime();
		InitAnimator ();
		InitMirror ();
		InitStrongSpecialEffect ();
		InitSpecialBehaviour ();
		InitSwitchSymbolDict();

		EndBlurEffect();
	}

	private void InitAnimator(){
		string[] animations = _reel.Machine.CoreMachine.MachineConfig.BasicConfig.RewindAnimations;
		IRandomGenerator roller = _reel.Machine.CoreMachine.Roller;

		if (animations != null) {
			// 读取所有的animation controller
			List<UnityEngine.Object> controllerList = new List<UnityEngine.Object>();
			ListUtility.ForEach (animations, (string animName) => {
				UnityEngine.Object control = AssetManager.Instance.LoadMachineAsset<UnityEngine.Object>(_effectAnimPath + animName, _reel.Machine.MachineName);
				if (control != null){
					controllerList.Add(control);
				}
			});

			if (controllerList.Count > 0)
				_animator.runtimeAnimatorController = RandomUtility.RollSingleElement(roller, controllerList) as RuntimeAnimatorController;
			
			_animator.enabled = false;
		}
	}

	// 镜像symbol
	private void InitMirror(){
		SymbolData data = _coreSymbol.SymbolData;
		if (data.CanMirror) {
			int index = ListUtility.Find(data.MirrorReelIndexes, (int i)=>{
				return i == _reel.ReelId;
			});
			if (index >= 0) {
				transform.localRotation = Quaternion.Euler (new Vector3(0.0f, 180.0f, 0.0f));
			}
		}
	}

	// 初始化强主题动画
	private void InitStrongSpecialEffect(){
		SymbolData data = _coreSymbol.SymbolData;
		_hasStrongSpecialEffect = data.HasStrongSpecialEffect;

		if (!_hasStrongSpecialEffect)
			return;

		if (!string.IsNullOrEmpty (data.StrongSpecialEffect)) {
			string path = _effectPrefabPath + data.StrongSpecialEffect;
			GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _reel.Machine.MachineName);
			_strongSpecialEffect = UGUIUtility.CreateObj (obj, gameObject);
			_strongSpecialEffectAnimator = _strongSpecialEffect.GetComponentInChildren<Animator> ();
			_hintStateName = data.StrongSpecialHintEffect;
			_winStateName = data.StrongSpecialWinEffect;
			_startEventStateName = data.StrongSpecialStartEffect;
			_hintStateDelay = data.StrongSpecialHintDelay;
			_winStateDelay = data.StrongSpecialWinDelay;
			_startEventStateDelay = data.StrongSpecialStartDelay;
			if (!data.StrongSpecialHintSound.IsNullOrEmpty ())
				_hintAudio = TypeUtility.GetEnumFromString<AudioType> (data.StrongSpecialHintSound);
			if (!data.StrongSpecialWinSound.IsNullOrEmpty ())
				_winAudio = TypeUtility.GetEnumFromString<AudioType> (data.StrongSpecialWinSound);
			if (!data.StrongSpecialStartSound.IsNullOrEmpty ())
				_startEventAudio = TypeUtility.GetEnumFromString<AudioType> (data.StrongSpecialStartSound);
		}

		HideStrongSpecialEffect ();
	}

	private void InitSpecialBehaviour(){
		ButterflyController butterflyCtrl = gameObject.GetComponentInChildren<ButterflyController> (true);
		if (butterflyCtrl != null)
			butterflyCtrl.Init (_reel.Machine , _coreSymbol);
	}

	private void InitImage()
	{
		string filePath = _imageDir + _coreSymbol.SymbolData.ArtAsset;
		_image.sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(filePath, _reel.Machine.MachineName);
		_image.SetNativeSize();

		//clone material
		#if false // zhousen 换一种symbol变暗的方式
		_image.material = new Material(_image.material);
		#else
		_image.material = null;
		#endif

		#if DEBUG
		float scaleFactor = _reel.Machine.PuzzleConfig.DebugSymbolScaleFactor;
		if(scaleFactor != 1.0f)
		{
			_image.gameObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1.0f);
		}
		#endif

		if(_isBlank)
			_image.enabled = false;
	}

	private void InitSingleBlurImageState(Image image, Dictionary<string, Sprite> dict)
	{
		if (_reel.Machine.MachineConfig.BasicConfig.UserBlurImage){
			string symbolName = _coreSymbol.SymbolData.ArtAsset;
			string filePath = _imageDir + symbolName + "_Blur";
			Sprite spr = AssetManager.Instance.LoadMachineAsset<Sprite>(filePath, _reel.Machine.MachineName);
			Debug.Assert(spr != null, symbolName);
			image.sprite = spr;
			image.SetNativeSize();

			// 竖直高度变成2倍
			Vector2 size = image.rectTransform.sizeDelta;
			image.rectTransform.sizeDelta = new Vector2(size.x * 2.0f, size.y * 2.0f);
		} else {
			string symbolName = _coreSymbol.SymbolData.Name;
			Debug.Assert(dict.ContainsKey(symbolName));
			image.sprite = dict[symbolName];
			image.SetNativeSize();
		}
		image.material = null;
		image.enabled = false;
	}

	private void InitBlurImage()
	{
		if(_isBlank)
		{
			//blank symbol don't need blur
			_blurImage.enabled = false;
		}
		else
		{
			InitSingleBlurImageState(_blurImage, _reel.Machine.ResourceManager.SymbolBlurSpriteDict);
		}
	}

	private void InitHypeBlurImage()
	{
		if(_reel.IsHypeReel())
		{
			if(_isBlank)
			{
				//blank symbol don't need blur
				_hypeBlurImage.enabled = false;
			}
			else
			{
				InitSingleBlurImageState(_hypeBlurImage, _reel.Machine.ResourceManager.SymbolHypeBlurSpriteDict);
			}
		}
	}

	private void InitWinEffect()
	{
		string effect3DName = _coreSymbol.SymbolData.WinEffect3D;
		if(!string.IsNullOrEmpty(effect3DName))
		{
			string path = _effect3DDir + effect3DName;
			GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _reel.Machine.MachineName);
			if(obj == null)
				Debug.LogError("Fail to load effect:" + path);
			_winEffect3D = Instantiate(obj, this.transform);
			_winEffect3D.transform.localPosition = Vector3.zero;
			_winEffect3D.transform.localScale = Vector3.one;
		}

		string effectLightName = _coreSymbol.SymbolData.WinEffect;
		if(!string.IsNullOrEmpty(effectLightName))
		{
			string path = _effectLightDir + effectLightName;
			GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _reel.Machine.MachineName);
			if(obj == null)
				Debug.LogError("Fail to load asset:" + path);
			
			_winEffectLight = Instantiate(obj, this.transform);
			_winEffectLight.transform.localPosition = Vector3.zero;
			_winEffectLight.transform.localScale = Vector3.one;
		}

		HideWinEffect();
	}

	private void InitHypeDarkEffect()
	{
		if(_coreSymbol.SymbolData.SymbolType != SymbolType.Blank)
		{
//			GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(_hypeDarkBackPath, _reel.Machine.MachineName);
//			_hypeDarkBack = Instantiate(obj, this.transform);
//			_hypeDarkBack.transform.localPosition = Vector3.zero;
//			_hypeDarkBack.transform.localScale = Vector3.one;
//			_hypeDarkBack.transform.SetAsFirstSibling();

//			string path = _effectLightDir + "FX_HypeSymbolLight";
//			obj = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _reel.Machine.MachineName);
//			_hypeSymbolEffect = Instantiate(obj, this.transform);
//			_hypeSymbolEffect.transform.localPosition = Vector3.zero;
//			_hypeSymbolEffect.transform.localScale = Vector3.one;

			HideHypeDarkEffect();
		}
	}

	private void InitBlurMatRealTime()
	{
		if(_reel.Machine.MachineConfig.BasicConfig.IsTriggerType(TriggerType.Collect))
		{
			Material prefab = AssetManager.Instance.LoadAsset<Material>(_blurMatRealTimeDir);
			_blurMatRealTime = new Material(prefab);
		}
	}

	void SetImageEnabled(bool flag)
	{
		if(!_isBlank)
			_image.enabled = flag;
	}

	#endregion

	#region Public

	public void ChangeAnimation(string clip){
		if (_animator != null){
			// LogUtility.Log("ChangeAnim = " + clip, Color.yellow);
			_animator.Play(clip);
		}
	}

	public void ShowWinEffect(WinType winType, bool shouldRespin)
	{
		if(_winEffect3D != null)
		{
			#if DEBUG && DEBUG_SHOW_SYMBOL_WHEN_WIN
			#else
			SetImageEnabled(false);
			#endif
			//deactive first to force it play from the first frame
			_winEffect3D.SetActive(false);
			_winEffect3D.SetActive(true);
		}
		if(_winEffectLight != null)
		{
			#if DEBUG && DEBUG_SHOW_SYMBOL_WHEN_WIN
			#else
			SetImageEnabled(false);
			#endif
			_winEffectLight.SetActive(false);
			_winEffectLight.SetActive(true);
		}

		ShowStrongSpecialWinEffect ();
		if (shouldRespin) {
			UnityTimer.Start (this, _winStateDelay, () => {
				ShowStrongSpecialStartEventEffect();
				UnityTimer.Start(this, _startEventStateDelay, ()=>{
					ShowStrongSpecialHintEffect();
				});
			});
		}
	}

	public void HideWinEffect()
	{
		HideWin3DEffect();
		HideWinLightEffect();

		bool respin = _reel.Machine.CoreMachine.ShouldRespin ();
		if (!respin)
			HideStrongSpecialEffect ();
	}

	public void HideWinLightEffect()
	{
		if(_winEffectLight != null)
		{
			SetImageEnabled(true);
			_winEffectLight.SetActive(false);
		}
	}

	public void HideWin3DEffect()
	{
		if(_winEffect3D != null)
		{
			SetImageEnabled(true);
			_winEffect3D.SetActive(false);
		}
	}

	public void ShowHypeDarkEffect()
	{
		if(_hypeDarkBack != null)
			_hypeDarkBack.SetActive(true);
		
		if(_hypeSymbolEffect != null)
			_hypeSymbolEffect.SetActive(true);
	}

	public void HideHypeDarkEffect()
	{
		if(_hypeDarkBack != null)
			_hypeDarkBack.SetActive(false);

		if(_hypeSymbolEffect != null)
			_hypeSymbolEffect.SetActive(false);
	}

	public void StartBlurEffect()
	{
		SetImageEnabled(false);
		if(!_isBlank)
		{
			if(_reel.IsHyping())
				_hypeBlurImage.enabled = true;
			else
				_blurImage.enabled = true;
		}

		StartCollectBlur ();
	}

	public void EndBlurEffect()
	{
		SetImageEnabled(true);
		if(!_isBlank)
		{
			_blurImage.enabled = false;
			_hypeBlurImage.enabled = false;
		}

		EndCollectBlur ();
	}

	public void StartWinNotMatchEffect()
	{
		if (_coreSymbol.SymbolData.SymbolType != SymbolType.Blank) {
//			_image.material.SetFloat ("_ColorFactor", 0.4f);
			BasicConfig config = _reel.Machine.MachineConfig.BasicConfig;
			if (config.DarkSymbolColors.Length > 0){
				float[] colors = config.DarkSymbolColors;
				_image.color = new Color(colors[0], colors[1], colors[2], colors[3]);
			}else{
				_image.color = Color.grey;
			}
//			LogUtility.Log ("StartWinNotMatchEffect symbol is "+_coreSymbol.SymbolData.Name+" reel is "+_coreSymbol.ReelIndex, Color.yellow);
		}
	}

	public void EndWinNotMatchEffect()
	{
		if (_coreSymbol.SymbolData.SymbolType != SymbolType.Blank) {
//			_image.material.SetFloat ("_ColorFactor", 1.0f);
			_image.color = Color.white;
//			LogUtility.Log ("EndWinNotMatchEffect symbol is "+_coreSymbol.SymbolData.Name+" reel is "+_coreSymbol.ReelIndex, Color.yellow);
		}
	}

	// zhousen 生成收集物
	public void GenerateCollect(){
		SymbolConfig config = _reel.Machine.MachineConfig.SymbolConfig;
		SymbolData data = config.GetSymbolData (_collectData.CollectSymbol);
//		LogUtility.Log ("Generate collect symbol name is " + _collectData.CollectSymbol);

		GameObject obj = new GameObject (data.Name + "_collect");

		obj.transform.SetParent (transform);
		obj.transform.localPosition = new Vector3 (50.0f, -50.0f, 0.0f);
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;

		Sprite spr = AssetManager.Instance.LoadMachineAsset<Sprite> (_imageDir + data.ArtAsset, _reel.Machine.MachineName);
		if (spr != null) {
			Image img = obj.AddComponent<Image> ();
			img.sprite = spr;
			//			img.SetNativeSize ();
			CollectController control = obj.AddComponent<CollectController> ();
			control.TrailEffect = CollectWidgetAttach(data.CollectTrailEffect, obj);
			control.AnimatorObject = CollectWidgetAttach(data.CollectAnimator, obj);
		}
	}

	private GameObject CollectWidgetAttach(string assetName, GameObject parent){
		GameObject obj = null;
		if (!assetName.IsNullOrEmpty()){
			string effectPath = _effectPrefabPath + assetName;
			GameObject effect = UGUIUtility.CreateMachineAsset(effectPath, _reel.Machine.MachineName, parent);
			if (effect != null){
				effect.SetActive(false);
				obj = effect;
			}
		}
		return obj;
	}

	private void StartCollectBlur(){
		if (_reel.Machine.MachineConfig.BasicConfig.IsTriggerType (TriggerType.Collect)) {
			CollectController ctrl = gameObject.GetComponentInChildren<CollectController> ();	

			if (ctrl != null) {
				Image img = ctrl.gameObject.GetComponent<Image> ();
				img.material = _blurMatRealTime;
			}
		}
	}

	private void EndCollectBlur(){
		if (_reel.Machine.MachineConfig.BasicConfig.IsTriggerType (TriggerType.Collect)) {
			CollectController ctrl = gameObject.GetComponentInChildren<CollectController> ();	

			if (ctrl != null) {
				Image img = ctrl.gameObject.GetComponent<Image> ();
				img.material = null;
			}
		}
	}

	public void AddAreaSymbolHandler(){
		if (AddHandler != null) {
			AddHandler ();
			AddHandler = null;
		}
	}

	public void PopAreaSymbolHandler(){
		if (PopHandler != null) {
			PopHandler ();
			PopHandler = null;
		}
	}

	// rewind动画处理
	public void StartRewindAnimation(){
		_animator.enabled = true;
		_animator.Play ("Rewind");

		AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo (0);
		UnityTimer.Start (this, info.length + 0.1f, () => {
			_animator.enabled = false;
		});
	}

	#endregion

	#region strong special machine

	public void ShowStrongSpecialEffect(bool show, string clip = ""){
		if (_strongSpecialEffect != null) {
			_strongSpecialEffect.SetActive (show);
			if (!string.IsNullOrEmpty(clip) && _strongSpecialEffectAnimator != null){
				_strongSpecialEffectAnimator.Play(clip);
			}
			SetImageEnabled(!show);
		}
	}	

	public void ShowStrongSpecialHintEffect(){
		if (_hasStrongSpecialEffect) {
			ShowStrongSpecialEffect (true, _hintStateName);
			PlayStrongSpecialSound (_hintAudio);
		} else {
			ShowWinEffect (WinType.Normal, false);
//			LogUtility.Log ("ShowStrongSpecialHintEffect "+_coreSymbol.SymbolData.Name, Color.yellow);
		}
	}

	public void HideStrongSpecialHintEffect(){
		if (_hasStrongSpecialEffect) {
//			AnimatorStateInfo info = _strongSpecialEffectAnimator.GetCurrentAnimatorStateInfo (0);
//			bool isHint = info.IsName (_hintStateName);
//			if (isHint) {
//				HideStrongSpecialEffect ();
//			}
		}else {
			HideWin3DEffect ();
		}
	}

	public void ShowStrongSpecialWinEffect(){
		if (_hasStrongSpecialEffect) {
			ShowStrongSpecialEffect (true, _winStateName);
			PlayStrongSpecialSound (_winAudio);
		}
	}

	public void ShowStrongSpecialStartEventEffect(){
		if (_hasStrongSpecialEffect) {
			ShowStrongSpecialEffect (true, _startEventStateName);
			PlayStrongSpecialSound (_startEventAudio);
		}
	}

	public void HideStrongSpecialEffect(){
		if (_hasStrongSpecialEffect) {
			ShowStrongSpecialEffect (false);
		}
	}

	public void PlayStrongSpecialSound(AudioType type){
		if (type == AudioType.None)
			return;

		if (!AudioManager.Instance.IsPlayingSound(type))
			AudioManager.Instance.PlaySound (type);
	}

	public float GetHideStrongSpecialEffectDelayTime(SymbolAnimStateType type = SymbolAnimStateType.Hint){
		if (_hasStrongSpecialEffect) {
			if (type == SymbolAnimStateType.Hint)
				return _hintStateDelay;
			else if (type == SymbolAnimStateType.Win)
				return _winStateDelay;
			else if (type == SymbolAnimStateType.StartEvent)
				return _startEventStateDelay;
			else
				return 0.0f;
		} else {
			return 0.5f;
		}
	}

	#endregion

	#region switch symbol

	// TODO:待实现
	private void InitSwitchSymbolDict(){
		SymbolData data = _coreSymbol.SymbolData;
		if (data.SwitchSymbolNames.Length > 0 && !data.SwitchSymbolNames[0].IsNullOrEmpty()){
			// 先加自己本身
			SwitchSymbolResourceInfo info = SwitchSymbolResourceInfo.CreateResourceInfo(this, _image.sprite, _blurImage.sprite, 
					_winEffect3D != null ? _winEffect3D : _winEffectLight);
			_switchSymbolDict.Add(data.Name, info);

			// 再循环遍历switch symbol names
			ListUtility.ForEach(data.SwitchSymbolNames, (string str)=>{
				data = GetSymbolData(str);
				info = SwitchSymbolResourceInfo.CreateResourceInfo(this, data);
				if (!_switchSymbolDict.ContainsKey(data.Name)){
					_switchSymbolDict.Add(data.Name, info);
				}
			});
		}
	}

	private SymbolData GetSymbolData(string name){
		return _reel.Machine.MachineConfig.SymbolConfig.GetSymbolData(name);
	}

	public void SwitchSymbol(CoreSymbol symbol){
		SymbolData data = symbol.SymbolData;
		if (_switchSymbolDict.ContainsKey(data.Name)){
			SwitchSymbolResourceInfo info = _switchSymbolDict[data.Name];
			// 图片替换
			_image.sprite = info.ImageSprite;
			_image.SetNativeSize();
			_blurImage.sprite = info.BlurSprite;
			_blurImage.SetNativeSize();
			// 特效停止播放 
			HideWinEffect();
			// 特效替换
			if (_winEffect3D != null){
				_winEffect3D = info.WinEffect;
				_animator = _winEffect3D.GetComponent<Animator>();
			}else if (_winEffectLight != null){
				_winEffectLight = info.WinEffect;
				_animator = _winEffectLight.GetComponent<Animator>();
			}
		}else{
			LogUtility.Log("Get Switch Symbol = " + data.Name);
		}
	}

	#endregion
}

using System.Collections;
using System.Collections.Generic;
using System;

public class BasicConfig
{
	public static readonly string Name = "Basic";

	private BasicSheet _sheet;
	public BasicSheet Sheet { get { return _sheet; } }

	private MachineConfig _machineConfig; //ref

	// *********************
	// Core general config
	// *********************

	private int _reelCount;
	private bool _isMultiLine;
	private MultiLineMethod _multiLineMethod;
	private float[] _nearHitProbs;

	private int _forceWinPayoutId;
	private int _forceWinThresholdForNormal;
	private int _forceWinThresholdForLucky;
	private ulong[] _periodRatioBets;// 影响ratio的下注区间
	private float _multilineHypeThreshold;// 多线大肆宣扬的阈值

	// 
	// Getters
	//
	public int ReelCount { get { return _reelCount; } }
	public bool IsMultiLine { get { return _isMultiLine; } }

	public MultiLineMethod MultiLineMethod { get { return _multiLineMethod; } }
	public bool IsMultiLineExhaustive { get { return _multiLineMethod == MultiLineMethod.Exhaustive; } }
	public bool IsMultiLineSymbolProb { get { return _multiLineMethod == MultiLineMethod.SymbolProb; } }

	public float[] NearHitProbs { get { return _nearHitProbs; } }

	public int ForceWinPayoutId { get { return _forceWinPayoutId; } }
	public int ForceWinThresholdForNormal { get { return _forceWinThresholdForNormal; } }
	public int ForceWinThresholdForLucky { get { return _forceWinThresholdForLucky; } }
	public ulong[] PeriodRatioBets { get { return _periodRatioBets; } }
	public float MultilineHypeThreshold { get { return _multilineHypeThreshold; } }

	public bool IsPeriodRatioBet {
		get {
			return _periodRatioBets.Length > 0;
		}
	}

	// ***********************
	// Core SmallGame config
	// ***********************

	private bool _isBonusValidOnNonPayline;
	private bool _hasSlide;
	private string[] _smallGameTypes;
	private TriggerType _triggerType;

	// Collect
	private string[] _collectSymbols;
	private int _collectNum;
	private float _normalCollectRate;
	private float _luckyCollectRate;

	// FixWild
	private FixWildType _fixWildType;
	private float _fixhitNonWin;

	// FreeSpin
	private FreeSpinType _freeSpinType;
	private bool _isFreeSpinFixBonus;
	private int _freeSpinTriggerCount;
	private string[] _freeSpinTriggerSymbolNames;
	private int _freeSpinStopCountOfBonusWild;
	private string[] _freeSpinStopSymbolNames;
	private float[] _freeSpinHitsNonWin;
	// freespin概率
	private float[] _freeSpinHits;
	private float[] _luckyFreeSpinHits;
	// 固定次数玩法用
	private int[] _freeSpinFixCounts;
	// 固定次数玩法中强制中奖次数
	private int _freeSpinForceWinTime;

	// Wheel
	private string[] _wheelSheetNames;

	// TapBox
	private string _tapBoxPuzzleType;
	private float[] _tapBoxWinProbsOfNormal;
	private float[] _tapBoxWinProbsOfLucky;
	private float[] _tapBoxWinRatios;
	private float[] _tapBoxRatioProbsOfNormal;
	private float[] _tapBoxRatioProbsOfLucky;

	// SymbolSwitch
	private int _symbolSwitchTriggerCount;
	private string[] _symbolSwitchTriggerNames;

	// 
	// Getters
	//
	public bool IsBonusValidOnNonPayline { get { return _isBonusValidOnNonPayline; } }
	public bool HasSlide { get { return _hasSlide; } }
	public string[] SmallGameTypes { get { return _smallGameTypes; } }
	public TriggerType TriggerType { get { return _triggerType; } }

	// Collect
	public string[] CollectSymbol { get { return _collectSymbols; } }
	public int CollectNum { get { return _collectNum; } }
	public float NormalCollectRate { get { return _normalCollectRate; } }
	public float LuckyCollectRate { get { return _luckyCollectRate; } }

	// FixWild
	public FixWildType FixWildType { get { return _fixWildType; } }
	public float FixHitNonWin { get { return _fixhitNonWin; } }

	// FreeSpin
	public FreeSpinType FreeSpinType{ get { return _freeSpinType; } }
	public bool IsFreeSpinFixBonus{ get { return _isFreeSpinFixBonus; } }
	public int FreeSpinTriggerCount { get { return _freeSpinTriggerCount; } }
	public string[] FreeSpinTriggerSymbolNames { get { return _freeSpinTriggerSymbolNames; } }
	public int FreeSpinStopCountOfBonusWild { get { return _freeSpinStopCountOfBonusWild; } }
	public string[] FreeSpinStopSymbolNames { get { return _freeSpinStopSymbolNames; } }
	public float[] FreeSpinHitsNonWin { get { return _freeSpinHitsNonWin; } }
	public float[] FreeSpinHits { get { return _freeSpinHits; } }
	public float[] LuckyFreeSpinHits { get { return _luckyFreeSpinHits; } }
	public int[] FreeSpinFixCounts { get { return _freeSpinFixCounts; } }
	public int FreeSpinForceWinTime { get { return _freeSpinForceWinTime; } }

	// Wheel
	public string[] WheelSheetNames { get { return _wheelSheetNames; } }

	// TapBox
	public bool IsPuzzleTapBox { get { return _tapBoxPuzzleType == "PuzzleTapBox"; } }
	public bool IsPuzzleTapChip { get { return _tapBoxPuzzleType == "PuzzleTapChip"; } }
	public float[] TapBoxWinProbsOfNormal { get { return _tapBoxWinProbsOfNormal; } }
	public float[] TapBoxWinProbsOfLucky { get { return _tapBoxWinProbsOfLucky; } }
	public float[] TapBoxWinRatios { get { return _tapBoxWinRatios; } }
	public float[] TapBoxRatioProbsOfNormal { get { return _tapBoxRatioProbsOfNormal; } }
	public float[] TapBoxRatioProbsOfLucky { get { return _tapBoxRatioProbsOfLucky; } }

	// SymbolSwitch
	public int SymbolSwitchTriggerCount { get { return _symbolSwitchTriggerCount; } }
	public string[] SymbolSwitchTriggerNames { get { return _symbolSwitchTriggerNames; } }

	// **********************
	// Puzzle general config
	// **********************

	private int[] _reelStartPos;
	private string _multiLinePayLineNumber;
	private string _multiFrameAsset;
	private string _multiLineBackAsset;

	private float _symbolUnitDistanceFactor;
	private string _reelSkin;
	private string _reelSkinFrame;
	private float _spinSpeed;
	private float[] _reelSpinTimes;// 对应puzzle config上的reel spin time

	private string _backgroundPrefab;
	private string _backgroundEffectPrefab;
	private string _backgroundEffectIdlePrefab;
	private string _backgroundEffectSpinPrefab;
	private string _backgroundEffectFreespinPrefab;
	private string _payLine;
	private float[] _payLineRectPos;
	private float[] _reelSpecialEffectOffsets;
	private float[] _reelNormalWinEffectOffsets;
	private float[] _reelBigWinEffectOffsets;
	private string _highLight;
	private float[] _highLightLocalPos;
	private bool _noReelShadow;
	private string _mirrorReelShadowUpSimple;
	private string _reelShadowUp;
	private string _reelShadowUpSimple;
	private string _reelShadowUpProperty;
	private string _reelShadowDown;
	private string _reelShadowDownProperty;
	private string _singleReelShadowUp;
	private string _singleReelShadowUpSimple;
	private string _singleReelShadowDown;
	private string _singleReelShadowDownSimple;
	private bool _hasIdleReelSideEffect;
	private bool _hasSpinReelSideEffect;
	private bool _hasRespinReelSideEffect;
	private bool _hasNormalWinReelSideEffect;
	private bool _hasBigWinReelSurroundingsEffect;
	private bool _useBlurImage;// 使用美术做的模糊图片

	private int _spinReelSoundNum;
	private AudioType[] _spinReelSoundNames;
	private AudioType[] _machineBGM;
	private AudioType[] _specialBGM;

	// machine ui
	private string _backgroundDecorate;// 背景层装饰
	private float[] _backgroundDecorateOffsets;// 装饰偏移
	private string _backgroundDecoratePrefab;// 背景层装饰prefab
	private int _mirrorReelShadowUpSortOrder;// 镜像遮罩的sort order
	private bool _showBackgroundEffectWhenFix;// 锁住时释放全屏特效开关
	private float _freespinHintDelay;// freespin时候提示效果播放的延迟时间
	private BackgroundType _freespinBackgroundEffectType;// freespin时候背景特效 
	private bool _freespinEffectNoDelay;
	private bool _reelBackSlice;// 是否以slice模式使用卷轴背景
	private string _betWinFrameImage;// BET和WIN板的贴图
	private bool _isBetWinFrameSimpleMode; // 上面的分数栏是否是simple模式
	private bool _isCanvasAlphaEnable;// 是否需要卷轴半透明化
	private float _canvasAlphaValue;// 半透明化的值

	// machine offset
	private float[] _reelFrameOffsets;// 面板偏移
	private float[] _jackpotPoolOffsets;// jackpot计分板偏移
	private string _reelFrameTopBoard; // ex. M44 use it

	// 特殊强元素特效
	private string _specialSymbolLightEffect;
	// 超级元素特效
	private string _superSymbolLightEffect;
	// 在支付线上才播放强元素特效
	private bool _isPaylineStrongSpecialEffect;
	// postSpinWaitTimeWhenRespin
	private float _postSpinWaitTimeWhenRespin;
	// 只有中间卷轴才有highwin、lowWin特效
	private bool _isOnlyMiddleReelHighLowWinEffect;
	// 元素模糊度
	private float _normalBlurFactor;
	private float _hypeBlurFactor;
	// 进入freespin是第一次旋转的delay
	private bool _enableTriggerSmallGameDelay;
	// 进入smallgame时候延迟
	private float _triggerSmallGameDelay;
	// 启用special reel light delay
	private bool _enableTriggerSmallGameEffectsDelay;
	// 触发播放时的延迟
	private float _triggerSmallGameEffectsDelay;
	// 触发播放特效时候延迟的时间间隔
	private float _triggerSmallGameEffectsDelayInterval;
	// 特殊卷轴效果区分
	private bool _enableSpecialSymbolEffectIndex;
	// 触发特效持续时间
	private float _triggerSmallGameEffectsDuration;
	// 在触发trigger特效之前播放一个win特效的延迟
	private float _triggerSmallGameWinEffectDelay;
	// 非中奖元素变暗时的颜色乘算
	private float[] _darkSymbolColors;
	private AudioType[] _hypeAudios;
	// respin时候播放音效
	private AudioType _respinAudio;

	//
	// Getters
	//
	public int[] ReelStartPos { get { return _reelStartPos; } }
	public string MultiLinePayLineNumber { get { return _multiLinePayLineNumber; } }
	public string MultiFrameAsset { get { return _multiFrameAsset; } }
	public string MultiLineBackAsset { get { return _multiLineBackAsset; } }

	public float SymbolUnitDistanceFactor { get { return _symbolUnitDistanceFactor; } }
	public string ReelSkin { get { return _reelSkin; } }
	public string ReelSkinFrame { get { return _reelSkinFrame; } }
	public float SpinSpeed { get { return _spinSpeed; } }
	public float[] ReelSpinTimes { get { return _reelSpinTimes; } }

	public string BackgroundPrefab { get{ return _backgroundPrefab; } }
	public string BackgroundEffectPrefab { get { return _backgroundEffectPrefab; } }
	public string BackgroundEffectIdlePrefab { get { return _backgroundEffectIdlePrefab; } }
	public string BackgroundEffectSpinPrefab { get { return _backgroundEffectSpinPrefab; } }
	public string BackgroundEffectFreespinPrefab { get { return _backgroundEffectFreespinPrefab; } }
	public string PayLine { get { return _payLine; } }
	public float[] PayLineRectPos { get { return _payLineRectPos; } }
	public float[] ReelSpecialEffectOffsets { get { return _reelSpecialEffectOffsets; } }
	public float[] ReelNormalWinEffectOffsets { get { return _reelNormalWinEffectOffsets; } }
	public float[] ReelBigWinEffectOffsets { get { return _reelBigWinEffectOffsets; } }
	public string HighLight { get{ return _highLight; } }
	public float[] HighLightLocalPos { get { return _highLightLocalPos; } }
	public bool NoReelShadow { get { return _noReelShadow; } }
	public string MirrorReelShadowUpSimple { get { return _mirrorReelShadowUpSimple; } }
	public string ReelShadowUp{ get{ return _reelShadowUp; } }
	public string ReelShadowUpSimple { get { return _reelShadowUpSimple; } }
	public string ReelShadowUpProperty{ get{ return _reelShadowUpProperty; } }
	public string ReelShadowDown{ get{ return _reelShadowDown; } }
	public string ReelShadowDownProperty{ get{ return _reelShadowDownProperty; } }
	public string SingleReelShadowUp { get { return _singleReelShadowUp; } }
	public string SingleReelShadowDown { get { return _singleReelShadowDown; } }
	public string SingleReelShadowUpSimple { get { return _singleReelShadowUpSimple; } }
	public string SingleReelShadowDownSimple { get { return _singleReelShadowDownSimple; } }
	public bool HasIdleReelSideEffect { get { return _hasIdleReelSideEffect; } }
	public bool HasSpinReelSideEffect { get { return _hasSpinReelSideEffect; } }
	public bool HasRespinReelSideEffect { get { return _hasRespinReelSideEffect; } }
	public bool HasNormalWinReelSideEffect { get { return _hasNormalWinReelSideEffect; } }
	public bool HasBigWinReelSurroundingsEffect { get { return _hasBigWinReelSurroundingsEffect; } }
	public bool UserBlurImage { get { return _useBlurImage; } }

	public int SpinReelSoundNumber { get { return _spinReelSoundNum; } }
	public AudioType[] SpinReelSoundNames { get { return _spinReelSoundNames; } }
	public AudioType[] MachineBGM { get { return _machineBGM; } }
	public AudioType[] SpecialBGM { get { return _specialBGM; } }

	// machine ui
	public string BackgroundDecorate { get { return _backgroundDecorate; } }
	public float[] BackgroudDecorateOffsets { get { return _backgroundDecorateOffsets; } }
	public string BackgroundDecoratePrefab { get { return _backgroundDecoratePrefab; } }
	public int MirrorReelShadowUpSortOrder { get { return _mirrorReelShadowUpSortOrder; } }
	public bool ShowBackgroundEffectWhenFix { get { return _showBackgroundEffectWhenFix; } }
	public float FreespinHintDelay { get { return _freespinHintDelay; } }
	public BackgroundType FreespinBackgroundEffectType { get { return _freespinBackgroundEffectType; } }
	public bool FreespinEffectNoDelay { get { return _freespinEffectNoDelay; } }
	public bool IsReelBackSlice { get { return _reelBackSlice; } }
	public string BetWinFrameImage { get { return _betWinFrameImage; } }
	public bool IsBetWinFrameSimpleMode { get { return _isBetWinFrameSimpleMode; } }
	public bool IsCanvasAlphaEnable { get { return _isCanvasAlphaEnable; } }
	public float CanvasAlphaValue { get { return _canvasAlphaValue; } }

	// machine offset
	public float[] ReelFrameOffsets { get { return _reelFrameOffsets; } }
	public float[] JackpotPoolOffsets { get { return _jackpotPoolOffsets; } }
	public string ReelFrameTopBoard { get { return _reelFrameTopBoard; } }

	public string SpecialSymbolLightEffect { get { return _specialSymbolLightEffect; } }
	public string SuperSymbolLightEffect { get { return _superSymbolLightEffect; } }
	public bool IsPaylineStrongSpecialEffect { get { return _isPaylineStrongSpecialEffect; } }
	public float PostSpinWaitTimeWhenRespin { get { return _postSpinWaitTimeWhenRespin; } }
	public bool IsOnlyMiddleReelHighLowWinEffect { get { return _isOnlyMiddleReelHighLowWinEffect; } }
	public float NormalBlurFactor { get { return _normalBlurFactor; } }
	public float HypeBlurFactor { get { return _hypeBlurFactor; } }
	public bool EnableTriggerSmallGameDelay { get { return _enableTriggerSmallGameDelay; } }
	public float TriggerSmallGameDelay { get { return _triggerSmallGameDelay; } }
	public bool EnableTriggerSmallGameEffectsDelay { get { return _enableTriggerSmallGameEffectsDelay; } }
	public float TriggerSmallGameEffectsDelay { get { return _triggerSmallGameEffectsDelay; } }
	public float TriggerSmallGameEffectsDelayInterval { get { return _triggerSmallGameEffectsDelayInterval; } }
	public bool EnableSpecialSymbolEffectIndex { get { return _enableSpecialSymbolEffectIndex; } }
	public float TriggerSmallGameEffectsDuration { get { return _triggerSmallGameEffectsDuration; } }
	public float TriggerSmallGameWinEffectDelay { get { return _triggerSmallGameWinEffectDelay; } }
	public float[] DarkSymbolColors { get { return _darkSymbolColors; } }
	public AudioType[] HypeAudios { get { return _hypeAudios; } }
	public AudioType RespinAudio { get { return _respinAudio; } }


	// ************************
	// Puzzle SmallGame config
	// ************************

	private string _smallGamePrefab;
	private string _smallGameEffect;

	//Frozen
	private string _frozenFadeInEffect;
	private string _frozenFadeOutEffect;

	private string _butterflyHitEffect;
	private string _butterflyHitTwinkleEffect;

	private string _jackpotScore;

	// Collect
	private string _collectSlider;
	private string _collectBottom;
	// 收集时特效
	private string _collectEffect;
	// 收集完成时特效
	private string _collectCompleteEffect;
	// 收集特效提示
	private string _collectHintEffect;
	// 收集特效(一直存在)
	private string _collectAlwaysEffect;

	// FreeSpin
	private string _respinPreEffect;
	private string _freeSpinSymbolFlyEffect;

	// Wheel
	private Dictionary<int, string> _wheelPrefabNameDict;

	// Rewind
	private string[] _rewindAnimations;

	// 龙机台中间卷轴效果
	private string _dragonMachineReelEffect;

	// Jackpot
	private bool _isJackpot;
	// jackpot 最小下注
	private ulong _leastBet;
	// jackpot 能启动jackpot的最小下注
	private ulong _jackpotMinBet;
	// jackpot选择界面
	private string _jackpotSelect;
	// jackpottips
	private string _jackpotTips;
	// jackpot payout index
	private int[] _jackpotPayoutIndexes;

	// logical switch flag
	private CloseBackgroundEffectType _closeBackgroundEffectType;
	private ShowBackgroundEffectType _showBackgroundEffectType;
	private CollectionProcessType _collectionProcessType;

	// special flag
	private bool _isSpecialMachine;
	private bool _canPlaySpecialSymbolEffect;
	private bool _respinReelSpecialEffect;
	private bool _hitTwinkle;
	private bool _backgroundEffectMultiType;// 背景特效多样化
	private bool _backgroundEffectStartOn;// 背景特效进机台就播放
	private bool _hideJackpotScoreWhenSmallGame;// 小游戏时隐藏jackpot计分板

	// 特效
	private string _smallgameSpecialEffect;
	private float _smallgameSpecialEffectDelayClose = 0.0f;
	private string _specialReelBackEffect;
	private string _specialReelBackAnim;
	private float _specialReelBackStartDelay = 0.0f;
	private string _specialReelBackIdleAnimTrigger;
	private float _collectFlyDuration = 0.0f;
	private float _collectAnimatorDuration = 0.0f;
	private string _collectSymbolEffect;
	private float[] _butterFlyStartPos;// 蝴蝶特效闪烁点位置
	private float _butterFlyHitTwinkleEnterDelay;// 蝴蝶飞入时延迟
	private float _butterFlyHitTwinkleEnterAndLeaveDelay;// 包含蝴蝶飞下去的延迟
	
	private float _switchSymbolEffectStartDelay;// 切换symbol模式下特效开始延迟
	private float _switchSymbolEffectEndDelay;// 切换symbol模式下关闭效果延迟
	private float[] _switchSymbolDelays;// 切换symbol的延迟，从左往右，对应每个symbol

	private float _backgroundEffectStartDelay;// 全屏特效播放delay
	private string _changeAnim;// 切换symbol动画

	// 音效
	private AudioType _butterflyHitAudio;
	private AudioType _butterflyUpAudio;
	private AudioType _butterflyDownAudio;
	private AudioType _frozenAudio;
	private AudioType _breakFrozenAudio;
	private AudioType _collectFlyAudio;
	private AudioType _specialAudio;
	private AudioType _freespinHintAudio;
	private AudioType _freespinBGM;
	private AudioType _switchSymbolSound1;// 切换symbol时的音效
	private float _switchSymbolSound1Delay;
	private AudioType _switchSymbolSound2;
	private float _switchSymbolSound2Delay;
	private AudioType _switchSymbolSound3;
	private float _switchSymbolSound3Delay;

	//
	// Getters
	//
	public string SmallGamePrefab { get { return _smallGamePrefab; } }
	public string SmallGameEffect { get { return _smallGameEffect; } }

	public string FrozenFadeInEffect{ get { return _frozenFadeInEffect; } }
	public string FrozenFadeOutEffect{ get { return _frozenFadeOutEffect; } }

	public string ButterflyHitEffect{ get { return _butterflyHitEffect; } }
	public string ButterflyHitTwinkleEffect { get { return _butterflyHitTwinkleEffect; } }

	public string JackpotScore { get { return _jackpotScore; } }

	// FreeSpin
	public string RespinPreEffect { get { return _respinPreEffect; } }
	public string FreeSpinSymbolFlyEffect { get { return _freeSpinSymbolFlyEffect; } }

	// Collect
	public string CollectSlider{ get { return _collectSlider; } }
	public string CollectBottom { get { return _collectBottom; } }
	public string CollectEffect { get { return _collectEffect; } }
	public string CollectCompleteEffect { get { return _collectCompleteEffect; } }
	public string CollectHintEffect { get { return _collectHintEffect; } }
	public string CollectAlwaysEffect { get { return _collectAlwaysEffect; } }

	// Wheel
	public Dictionary<int, string> WheelPrefabNameDict { get { return _wheelPrefabNameDict; } }

	public string[] RewindAnimations { get { return _rewindAnimations; } }
	public string DragonMachineReelEffect { get { return _dragonMachineReelEffect; } }

	// Jackpot
	public bool IsJackpot { get { return _isJackpot; } }
	public ulong LeastBet { get { return _leastBet; } }
	public ulong JackpotMinBet { get { return _jackpotMinBet; } }
	public string JackpotSelect { get { return _jackpotSelect; } }
	public string JackpotTips { get { return _jackpotTips; } }
	public int[] JackpotPayoutIndexes { get { return _jackpotPayoutIndexes; } }

	// logic switch flag
	public CloseBackgroundEffectType CloseBgEffectType { get { return _closeBackgroundEffectType; } }
	public ShowBackgroundEffectType ShowBgEffectType { get { return _showBackgroundEffectType; } }
	public CollectionProcessType CollectionProcessType { get { return _collectionProcessType; } }

	// special flag
	public bool IsSpecialMachine { get { return _isSpecialMachine; } }
	public bool CanPlaySpecialSymbolEffect { get { return _canPlaySpecialSymbolEffect; } }
	public bool RespinReelSpecialEffect { get { return _respinReelSpecialEffect; } }
	public bool HitTwinkle { get { return _hitTwinkle; } }
	public bool BackgroundEffectMultiType { get { return _backgroundEffectMultiType; } }
	public bool BackgroundEffectStartOn { get { return _backgroundEffectStartOn; } }
	public bool HideJackpotScoreWhenSmallGame { get { return _hideJackpotScoreWhenSmallGame; } }

	// 特效
	public string SmallGameSpecialEffect { get { return _smallgameSpecialEffect; } }
	public float SmallGameSpecialEffectDelayClose { get { return _smallgameSpecialEffectDelayClose; } }
	public string SpecialReelBackEffect { get { return _specialReelBackEffect; } }
	public string SpecialReelBackAnim { get { return _specialReelBackAnim; } }
	public float SpecialReelBackStartDelay { get { return _specialReelBackStartDelay; } }
	public string SpecialReelBackIdleAnimTrigger { get { return _specialReelBackIdleAnimTrigger; } }
	public float CollectFlyDuration { get { return _collectFlyDuration; } }
	public float CollectAnimatorDuration { get { return _collectAnimatorDuration; } }
	public float CollectTotalDuration { get { return _collectFlyDuration + _collectAnimatorDuration; } }
	public string CollectSymbolEffect { get { return _collectSymbolEffect; } }
	public float[] ButterFlyStartPos { get { return _butterFlyStartPos; } }
	public float ButterFlyHitTwinkleEnterDelay { get { return _butterFlyHitTwinkleEnterDelay; } }
	public float ButterFlyHitTwinkleEnterAndLeaveDelay { get { return _butterFlyHitTwinkleEnterAndLeaveDelay; } }
	public float SwitchSymbolEffectStartDelay { get { return _switchSymbolEffectStartDelay; } }
	public float SwitchSymbolEffectEndDelay { get { return _switchSymbolEffectEndDelay; } }
	public float[] SwitchSymbolDelays { get { return _switchSymbolDelays; } }
	public float BackgroundEffectStartDelay { get { return _backgroundEffectStartDelay; } }
	public string ChangeAnim { get { return _changeAnim; } }

	// 音效
	public AudioType ButterflyHitAudio { get { return _butterflyHitAudio; } }
	public AudioType ButterflyUpAudio { get { return _butterflyUpAudio; } }
	public AudioType ButterflyDownAudio { get { return _butterflyDownAudio; } }
	public AudioType FrozenAudio { get { return _frozenAudio; } }
	public AudioType BreakFrozenAudio { get { return _breakFrozenAudio; } }
	public AudioType CollectFlyAudio { get { return _collectFlyAudio; } }
	public AudioType SpecialAudio { get { return _specialAudio; } }
	public AudioType FreespinHintAudio { get { return _freespinHintAudio; } }
	public AudioType FreespinBGM { get { return _freespinBGM; } }
	public AudioType SwitchSymbolSound1 { get { return _switchSymbolSound1; } }
	public float SwitchSymbolSound1Delay { get { return _switchSymbolSound1Delay; } }
	public AudioType SwitchSymbolSound2 { get { return _switchSymbolSound2; } }
	public float SwitchSymbolSound2Delay { get { return _switchSymbolSound2Delay; } }
	public AudioType SwitchSymbolSound3 { get { return _switchSymbolSound3; } }
	public float SwitchSymbolSound3Delay { get { return _switchSymbolSound3Delay; } }

	public bool HasFreeSpin{
		get { return _smallGameTypes.Contains("FreeSpin"); }
	}
	public bool HasRewind{
		get { return _smallGameTypes.Contains("Rewind"); }
	}
	public bool HasFixWild{
		get{ return _smallGameTypes.Contains("FixWild"); }
	}
	public bool HasWheel{
		get { return _smallGameTypes.Contains("Wheel"); }
	}
	public bool HasJackpot{
		get { return _smallGameTypes.Contains("Jackpot") || !string.IsNullOrEmpty(_jackpotScore); }
	}
	public bool HasSwitchSymbol{
		get { return _smallGameTypes.Contains("SwitchSymbol"); }
	}

	// Tap games
	public bool HasTapBox{
		get { return _smallGameTypes.Contains("TapBox"); }
	}

	public bool IsTriggerNone{
		get { return _triggerType == TriggerType.None; }
	}
	public bool IsTriggerCollect{
		get{ return _triggerType == TriggerType.Collect; }
	}
	public bool IsTriggerPayout{
		get{ return _triggerType == TriggerType.Payout; }
	}

	public bool IsThreeReel
	{
		get { return _reelCount == CoreDefine.Reel3; }
	}

	public bool IsFourReel
	{
		get { return _reelCount == CoreDefine.Reel4; }
	}

	public bool IsFiveReel
	{
		get { return _reelCount == CoreDefine.Reel5; }
	}

	public int BasicReelCount
	{
		get {
			if(_reelCount == CoreDefine.Reel4)
				return CoreDefine.Reel3;
			else
				return _reelCount;
		}
	}

	//only for MultiLine
	public bool ShouldNormalizeBetAmount
	{
		get {
			return IsFiveReel;
		}
	}

	//hype
	int[] _hypeReelIds;
	int[] _hypeReelIndexes;
	public int[] HypeReelIds { get { return _hypeReelIds; } }
	public int[] HypeReelIndexes { get { return _hypeReelIndexes; } }
	bool _hasHype;
	public bool HasHype { get { return _hasHype; } }

	public bool IsTriggerType(TriggerType type){
		return _triggerType == type;
	}

	public BasicConfig(BasicSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;
		_machineConfig = machineConfig;

		InitCoreGeneralConfig();
		InitCoreSmallGameConfig();
		InitPuzzleGeneralConfig();
		InitPuzzleSmallGameConfig();
	}

	void InitCoreGeneralConfig()
	{
		string str;

		//reelCount
		str = ValueFromKey("ReelCount");
		CoreDebugUtility.Assert(!str.IsNullOrEmpty());
		_reelCount = int.Parse(str);

		//isMultiLine
		str = ValueFromKey("IsMultiLine");
		if(!str.IsNullOrEmpty())
			_isMultiLine = int.Parse(str) != 0;
		else
			_isMultiLine = false;

		InitHype();

		str = ValueFromKey("MultiLineMethod");
		if(!str.IsNullOrEmpty())
			_multiLineMethod = TypeUtility.GetEnumFromString<MultiLineMethod>(str);
		else
			_multiLineMethod = MultiLineMethod.Exhaustive;

		str = ValueFromKey ("NearHitProbs");
		if(!str.IsNullOrEmpty())
			_nearHitProbs = StringUtility.ParseToArray<float>(str);
		else
			_nearHitProbs = new float[0];

		_forceWinPayoutId = GetKeyValue<int>("ForceWinPayoutId");
		_forceWinThresholdForNormal = GetKeyValue<int>("ForceWinThresholdForNormal");
		_forceWinThresholdForLucky = GetKeyValue<int>("ForceWinThresholdForLucky");
		_periodRatioBets = GetKeyValues<ulong>("PeriodRatioBets");
		_multilineHypeThreshold = GetKeyValue<float>("MultiLineHypeThreshold");
	}

	void InitHype()
	{
		if(_reelCount == CoreDefine.Reel3 || _reelCount == CoreDefine.Reel4)
		{
			if(_isMultiLine)
			{
				_hypeReelIds = new int[] { };
				_hasHype = false;
			}
			else
			{
				if (_reelCount == CoreDefine.Reel3){
					_hypeReelIds = new int[] { CoreDefine.Reel3 };
				}else {
					_hypeReelIds = new int[] { CoreDefine.Reel3, CoreDefine.Reel4 };
				}
				_hasHype = true;
			}
		}
		else if(_reelCount == CoreDefine.Reel5)
		{
			_hypeReelIds = new int[] { CoreDefine.Reel4, CoreDefine.Reel5 };
			_hasHype = true;
		}
		else
		{
			CoreDebugUtility.Assert(false);
		}

		_hypeReelIndexes = new int[_hypeReelIds.Length];
		for(int i = 0; i < _hypeReelIndexes.Length; i++)
			_hypeReelIndexes[i] = _hypeReelIds[i] - 1;
	}

	void InitCoreSmallGameConfig()
	{
		string str;
		bool success = false;

		str = ValueFromKey("IsBonusValidOnNonPayline");
		if(!str.IsNullOrEmpty())
			_isBonusValidOnNonPayline = int.Parse(str) != 0;
		else
			_isBonusValidOnNonPayline = false;
		
		str = ValueFromKey("HasSlide");
		if(!str.IsNullOrEmpty())
		{
			int hasSlideInt = int.Parse(str);
			_hasSlide = hasSlideInt != 0;
		}
		else
		{
			_hasSlide = false;
		}

		str = ValueFromKey("SmallGameTypes");
		if(!str.IsNullOrEmpty())
			_smallGameTypes = StringUtility.ParseToArray<string>(str);
		else
			_smallGameTypes = new string[0];

		str = ValueFromKey ("TriggerType");
		if(!str.IsNullOrEmpty())
			_triggerType = TypeUtility.GetEnumFromString<TriggerType>(str);

		// FixWild
		str = ValueFromKey("FixWildType");
		if (!str.IsNullOrEmpty ())
			_fixWildType = TypeUtility.GetEnumFromString<FixWildType> (str);
		else
			_fixWildType = FixWildType.None;
		
		str = ValueFromKey ("FixHitNonWin");
		if(!str.IsNullOrEmpty())
			_fixhitNonWin = StringUtility.ParseTo<float>(str, out success);
		else
			_fixhitNonWin = 0.0f;

		str = ValueFromKey ("CollectSymbols");
		_collectSymbols = StringUtility.ParseToArray<string> (str);

		str = ValueFromKey ("CollectCount");
		if(!str.IsNullOrEmpty())
			_collectNum = StringUtility.ParseTo<int>(str, out success);
		else
			_collectNum = 0;

		str = ValueFromKey ("NormalCollectRate");
		if(!str.IsNullOrEmpty())
			_normalCollectRate = StringUtility.ParseTo<float>(str, out success);
		else
			_normalCollectRate = 0.0f;

		str = ValueFromKey ("LuckyCollectRate");
		if(!str.IsNullOrEmpty())
			_luckyCollectRate = StringUtility.ParseTo<float>(str, out success);
		else
			_luckyCollectRate = 0.0f;

		//FreeSpin
		str = ValueFromKey ("FreeSpinType");
		if(!str.IsNullOrEmpty())
			_freeSpinType = TypeUtility.GetEnumFromString<FreeSpinType>(str);

		//isFreeSpinFixBonus
		str = ValueFromKey("IsFreeSpinFixBonus");
		if(!str.IsNullOrEmpty())
			_isFreeSpinFixBonus = str != "0";

		//freeSpinTriggerCount
		str = ValueFromKey("FreeSpinTriggerCount");
		if(!str.IsNullOrEmpty())
			_freeSpinTriggerCount = int.Parse(str);
		else
			_freeSpinTriggerCount = 0;

		//freeSpinTriggerSymbolNames
		str = ValueFromKey("FreeSpinTriggerSymbolNames");
		if(!str.IsNullOrEmpty())
			_freeSpinTriggerSymbolNames = StringUtility.ParseToArray<string> (str);

		//freeSpinStopCountOfBonusWild
		str = ValueFromKey("FreeSpinStopCountOfBonusWild");
		if(!str.IsNullOrEmpty())
			_freeSpinStopCountOfBonusWild = int.Parse(str);
		else
			_freeSpinStopCountOfBonusWild = 0;

		//freespinstopSymbolNames
		str = ValueFromKey("FreeSpinStopSymbolNames");
		if (!str.IsNullOrEmpty ()) {
			_freeSpinStopSymbolNames = StringUtility.ParseToArray<string> (str);
		}

		//freeSpinHitsNonWin
		str = ValueFromKey("FreeSpinHitsNonWin");
		if(!str.IsNullOrEmpty())
			_freeSpinHitsNonWin = StringUtility.ParseToArray<float>(str);
		else
			_freeSpinHitsNonWin = new float[0];

		str = ValueFromKey ("FreeSpinHits");
		if(!str.IsNullOrEmpty())
			_freeSpinHits = StringUtility.ParseToArray<float> (str);
		else
			_freeSpinHits = new float[0];

		str = ValueFromKey ("LuckyFreeSpinHits");
		if(!str.IsNullOrEmpty())
			_luckyFreeSpinHits = StringUtility.ParseToArray<float> (str);
		else
			_luckyFreeSpinHits = new float[0];

		str = ValueFromKey ("FreeSpinForceWinTime");
		if (!str.IsNullOrEmpty ())
			_freeSpinForceWinTime = StringUtility.ParseTo<int> (str, out success);
		else
			_freeSpinForceWinTime = 0;

		str = ValueFromKey ("FreeSpinFixCounts");
		if(!str.IsNullOrEmpty())
			_freeSpinFixCounts = StringUtility.ParseToArray<int>(str);
		else
			_freeSpinFixCounts = new int[0];

		str = ValueFromKey ("WheelSheetNames");
		if(!str.IsNullOrEmpty())
			_wheelSheetNames = StringUtility.ParseToArray<string>(str);
		else
			_wheelSheetNames = new string[0];

		// TapBox
		_tapBoxPuzzleType = ValueFromKey("TapBoxPuzzleType");

		str = ValueFromKey("TapBoxWinProbsOfNormal");
		if(!str.IsNullOrEmpty())
			_tapBoxWinProbsOfNormal = StringUtility.ParseToArray<float>(str);

		str = ValueFromKey("TapBoxWinProbsOfLucky");
		if(!str.IsNullOrEmpty())
			_tapBoxWinProbsOfLucky = StringUtility.ParseToArray<float>(str);

		str = ValueFromKey("TapBoxWinRatios");
		if(!str.IsNullOrEmpty())
			_tapBoxWinRatios = StringUtility.ParseToArray<float>(str);

		str = ValueFromKey("TapBoxRatioProbsOfNormal");
		if(!str.IsNullOrEmpty())
		{
			_tapBoxRatioProbsOfNormal = StringUtility.ParseToArray<float>(str);
			CoreDebugUtility.Assert(_tapBoxWinRatios.Length == _tapBoxRatioProbsOfNormal.Length);
		}

		str = ValueFromKey("TapBoxRatioProbsOfLucky");
		if(!str.IsNullOrEmpty())
		{
			_tapBoxRatioProbsOfLucky = StringUtility.ParseToArray<float>(str);
			CoreDebugUtility.Assert(_tapBoxWinRatios.Length == _tapBoxRatioProbsOfLucky.Length);
		}

		// SymbolSwitch
		_symbolSwitchTriggerCount = GetKeyValue<int>("SymbolSwitchTriggerCount");
		_symbolSwitchTriggerNames = GetKeyValues<string>("SymbolSwitchTriggerNames");
	}

	void InitPuzzleGeneralConfig()
	{
		string str;
		bool success = false;

		//reelStartPos
		str = ValueFromKey("ReelStartPos");
		CoreDebugUtility.Assert(!str.IsNullOrEmpty());
		_reelStartPos = StringUtility.ParseToArray<int>(str);

		_multiLinePayLineNumber = ValueFromKey("MultiLinePayLineNumber");
		_multiFrameAsset = ValueFromKey("MultiFrameAsset");
		_multiLineBackAsset = ValueFromKey("MultiLineBackAsset");

		//reelSkin
		str = ValueFromKey("ReelSkin");
		CoreDebugUtility.Assert(!str.IsNullOrEmpty());
		_reelSkin = str;

		//reelSkinFrame
		str = ValueFromKey("ReelSkinFrame");
		_reelSkinFrame = str;

		_spinSpeed = GetKeyValue<float>("SpinSpeed");
		_reelSpinTimes = GetKeyValues<float>("ReelSpinTimes");

		str = ValueFromKey ("SymbolUnitDistanceFactor");
		if(!str.IsNullOrEmpty())
			_symbolUnitDistanceFactor = StringUtility.ParseTo<float>(str, out success);
		else
			_symbolUnitDistanceFactor = 1.0f;

		//background prefab
		str = ValueFromKey("BackgroundPrefab");
		CoreDebugUtility.Assert (!str.IsNullOrEmpty ());
		_backgroundPrefab = str;

		//background effect prefab
		str = ValueFromKey("BackgroundEffectPrefab");
		_backgroundEffectPrefab = str;

		str = ValueFromKey ("BackgroundSpinEffect");
		_backgroundEffectSpinPrefab = str;

		str = ValueFromKey ("BackgroundIdleEffect");
		_backgroundEffectIdlePrefab = str;

		str = ValueFromKey ("BackgroundFreespinEffect");
		_backgroundEffectFreespinPrefab = str;

		//payLine
		str = ValueFromKey("PayLine");
		_payLine = str;

		//paylineRectPos
		str = ValueFromKey("PayLineRectPos");
		if (!string.IsNullOrEmpty (str)) {
			_payLineRectPos = StringUtility.ParseToArray<float> (str);
		} else {
			_payLineRectPos = new float[0];
		}

		//reel special effect pos
		str = ValueFromKey("ReelSpecialEffectOffsets");
		if (!string.IsNullOrEmpty (str)) {
			_reelSpecialEffectOffsets = StringUtility.ParseToArray<float> (str);
			CoreDebugUtility.Assert (_reelSpecialEffectOffsets.Length == _reelCount * 2, "ReelSpecialEffectOffsets length out of indexrange");
		} else {
			_reelSpecialEffectOffsets = new float[0];
		}

		str = ValueFromKey("ReelNormalWinEffectOffsets");
		if (!string.IsNullOrEmpty (str)) {
			_reelNormalWinEffectOffsets = StringUtility.ParseToArray<float> (str);
			CoreDebugUtility.Assert (_reelNormalWinEffectOffsets.Length == _reelCount * 2, "_reelNormalWinEffectOffsets length out of indexrange");
		} else {
			_reelNormalWinEffectOffsets = new float[0];
		}

		str = ValueFromKey("ReelBigWinEffectOffsets");
		if (!string.IsNullOrEmpty (str)) {
			_reelBigWinEffectOffsets = StringUtility.ParseToArray<float> (str);
			CoreDebugUtility.Assert (_reelBigWinEffectOffsets.Length == _reelCount * 2, "_reelBigWinEffectOffsets length out of indexrange");
		} else {
			_reelBigWinEffectOffsets = new float[0];
		}

		//highLight
		str = ValueFromKey("HighLight");
		_highLight = str;

		//highLight Local Pos
		str = ValueFromKey("HighLightLocalPos");
		if (!str.IsNullOrEmpty ()) {
			_highLightLocalPos = StringUtility.ParseToArray<float> (str);
			CoreDebugUtility.Assert (_highLightLocalPos.Length == 3, "_highLightLocalPos length != 3");
		} else {
			_highLightLocalPos = new float[0];
		}

		//no reel shadow
		str = ValueFromKey("NoReelShadow");
		if(!str.IsNullOrEmpty())
			_noReelShadow = int.Parse(str) != 0;
		else
			_noReelShadow = false;

		//reel shadow up
		str = ValueFromKey("MirrorReelShadowUpSimple");
		_mirrorReelShadowUpSimple = str;

		str = ValueFromKey("ReelShadowUp");
		_reelShadowUp = str;

		str = ValueFromKey ("ReelShadowUpSimple");
		_reelShadowUpSimple = str;

		//reel shadow up property
		str = ValueFromKey("ReelShadowUpProperty");
		_reelShadowUpProperty = str;

		//reel shadow down
		str = ValueFromKey("ReelShadowDown");
		_reelShadowDown = str;

		//reel shadow down property
		str = ValueFromKey("ReelShadowDownProperty");
		_reelShadowDownProperty = str;

		str = ValueFromKey ("SingleReelShadowUp");
		_singleReelShadowUp = str;

		str = ValueFromKey ("SingleReelShadowUpSimple");
		_singleReelShadowUpSimple = str;

		str = ValueFromKey("SingleReelShadowDownSimple");
		_singleReelShadowDownSimple = str;

		str = ValueFromKey ("SingleReelShadowDown");
		_singleReelShadowDown = str;

		_hasIdleReelSideEffect = GetBoolValueFromKey("HasIdleReelSideEffect");
		_hasSpinReelSideEffect = GetBoolValueFromKey("HasSpinReelSideEffect");
		_hasRespinReelSideEffect = GetBoolValueFromKey("HasRespinReelSideEffect");
		_hasNormalWinReelSideEffect = GetBoolValueFromKey("HasNormalWinReelSideEffect");
		_hasBigWinReelSurroundingsEffect = GetBoolValueFromKey("HasBigWinReelSurroundingsEffect");

		_useBlurImage = GetBoolKeyValue("UserBlurImage");

		// 强主题特效是否要在支付线时才能播放
		str = ValueFromKey ("IsPaylineStrongSpecialEffect");
		if (!str.IsNullOrEmpty ()) {
			_isPaylineStrongSpecialEffect = int.Parse(str) != 0;
		} else {
			_isPaylineStrongSpecialEffect = true;
		}

		str = ValueFromKey ("SpinReelSoundNumber");
		if (!str.IsNullOrEmpty ())
			_spinReelSoundNum = int.Parse (str);
		else
			_spinReelSoundNum = 0;

		_spinReelSoundNames = GetAudioKeyValues("SpinReelSoundNames");

		str = ValueFromKey ("MachineBGM");
		if (!str.IsNullOrEmpty ()) {
			_machineBGM = StringUtility.ParseToEnumArray<AudioType>(str);
		}
		else{
			_machineBGM = new AudioType[0];
		}

		str = ValueFromKey ("SpecialBGM");
		if (!str.IsNullOrEmpty ()) {
			_specialBGM = StringUtility.ParseToEnumArray<AudioType>(str);
		}
		else{
			_specialBGM = new AudioType[0];
		}

		_reelBackSlice = GetBoolKeyValue("IsReelBackSlice");
		_betWinFrameImage = GetKeyValue<string>("BetWinFrameImage");
		_isBetWinFrameSimpleMode = GetBoolKeyValue("IsBetWinFrameSimpleMode");
		_isCanvasAlphaEnable = GetBoolKeyValue("CanvasAlphaEnable");
		_canvasAlphaValue = GetKeyValue<float>("CanvasAlphaValue");
	}

	void InitPuzzleSmallGameConfig()
	{
		string str;

		_smallGamePrefab = ValueFromKey("SmallGamePrefab");
		_smallGameEffect = ValueFromKey ("SmallGameEffect");

		// frozen effect
		_frozenFadeInEffect = ValueFromKey("FrozenFadeInEffect");
		_frozenFadeOutEffect = ValueFromKey("FrozenFadeOutEffect");

		_butterflyHitEffect = ValueFromKey ("ButterFlyHitEffect");
		_butterflyHitTwinkleEffect = ValueFromKey ("ButterFlyHitTwinkleEffect");

		_respinPreEffect = ValueFromKey("RespinPreEffect");
		_freeSpinSymbolFlyEffect = ValueFromKey("FreeSpinSymbolFlyEffect");

		str = ValueFromKey("WheelPrefabNameDict");
		if(!str.IsNullOrEmpty())
			_wheelPrefabNameDict = StringUtility.ParseToDictionary<int, string>(str);
		else
			_wheelPrefabNameDict = null;

		_collectEffect = ValueFromKey ("CollectEffect");
		_collectCompleteEffect = ValueFromKey ("CollectCompleteEffect");
		_collectHintEffect = ValueFromKey ("CollectHintEffect");
		_collectAlwaysEffect = ValueFromKey("CollectAlwaysEffect");
		_collectSlider = ValueFromKey ("CollectSlider");
		_collectBottom = ValueFromKey ("CollectButton");

		_jackpotScore = ValueFromKey ("JackpotScore");

		_specialSymbolLightEffect = ValueFromKey ("SpecialSymbolLightEffect");
		_superSymbolLightEffect = ValueFromKey ("SuperSymbolLightEffect");

		str = ValueFromKey ("RewindAnimations");
		if (!string.IsNullOrEmpty (str))
			_rewindAnimations = StringUtility.ParseToArray<string> (str);
		else
			_rewindAnimations = null;

		//IsJackpot
		str = ValueFromKey("IsJackpot");
		if(!str.IsNullOrEmpty())
			_isJackpot = int.Parse(str) != 0;
		else
			_isJackpot = false;

		str = ValueFromKey ("PostSpinWaitTimeWhenRespin");
		if (!str.IsNullOrEmpty ()) {
			_postSpinWaitTimeWhenRespin = float.Parse (str);
		} else {
			_postSpinWaitTimeWhenRespin = -1.0f;
		}

		_isOnlyMiddleReelHighLowWinEffect = GetBoolKeyValue("IsOnlyMiddleReelHighLowWinEffect");

		_dragonMachineReelEffect = ValueFromKey ("DragonMachineReelEffect");

		_switchSymbolSound1 = GetAudioKeyValue("SwitchSymbolSound1");
		_switchSymbolSound1Delay = GetKeyValue<float>("SwitchSymbolSound1Delay");
		_switchSymbolSound2 = GetAudioKeyValue("SwitchSymbolSound2");
		_switchSymbolSound2Delay = GetKeyValue<float>("SwitchSymbolSound2Delay");
		_switchSymbolSound3 = GetAudioKeyValue("SwitchSymbolSound3");
		_switchSymbolSound3Delay = GetKeyValue<float>("SwitchSymbolSound3Delay");

		str = ValueFromKey ("LeastBet");
		if (!str.IsNullOrEmpty ())
			_leastBet = ulong.Parse (str);
		else
			_leastBet = 0;

		str = ValueFromKey ("JackpotMinBet");
		if (!str.IsNullOrEmpty ())
			_jackpotMinBet = ulong.Parse (str);
		else
			_jackpotMinBet = 0;

		_jackpotSelect = ValueFromKey ("JackpotSelect");
		_jackpotTips = ValueFromKey ("JackpotTips");

		str = ValueFromKey ("JackpotPayoutIndexes");
		if(!str.IsNullOrEmpty())
			_jackpotPayoutIndexes = StringUtility.ParseToArray<int>(str);
		else
			_jackpotPayoutIndexes = new int[0];

		// logic switch flag
		str = ValueFromKey ("CloseBackgroundEffectType");
		if (!str.IsNullOrEmpty ()) {
			_closeBackgroundEffectType = TypeUtility.GetEnumFromString<CloseBackgroundEffectType> (str);
		} else {
			_closeBackgroundEffectType = CloseBackgroundEffectType.Max;
		}

		str = ValueFromKey ("ShowBackgroundEffectType");
		if (!str.IsNullOrEmpty ()) {
			_showBackgroundEffectType = TypeUtility.GetEnumFromString<ShowBackgroundEffectType> (str);
		} else {
			_showBackgroundEffectType = ShowBackgroundEffectType.Max;
		}

		str = ValueFromKey ("CollectionProcessType");
		if (!str.IsNullOrEmpty ()) {
			_collectionProcessType = TypeUtility.GetEnumFromString<CollectionProcessType> (str);
		} else {
			_collectionProcessType = CollectionProcessType.Max;
			CoreDebugUtility.Assert(_triggerType != TriggerType.Collect);
		}

		_isSpecialMachine = GetBoolKeyValue("IsSpecialMachine");
		_canPlaySpecialSymbolEffect = GetBoolKeyValue("CanPlaySpecialSymbolEffect");
		_respinReelSpecialEffect = GetBoolKeyValue("RespinReelSpecialEffect");
		_hitTwinkle = GetBoolKeyValue("HitTwinkle");

		_backgroundDecorate = ValueFromKey ("BackgroundDecorate");

		str = ValueFromKey ("BackgroundDecorateOffsets");
		if (!str.IsNullOrEmpty ()) {
			_backgroundDecorateOffsets = StringUtility.ParseToArray<float> (str);
		} else {
			_backgroundDecorateOffsets = new float[0];
		}

		_backgroundDecoratePrefab = ValueFromKey ("BackgroundDecoratePrefab");

		_reelFrameOffsets = GetKeyValues<float>("ReelFrameOffsets");
		_jackpotPoolOffsets = GetKeyValues<float>("JackpotPoolOffsets");
		_reelFrameTopBoard = ValueFromKey("ReelFrameTopBoard");

		// effect
		_smallgameSpecialEffect = ValueFromKey ("SmallGameSpecialEffect");

		_smallgameSpecialEffectDelayClose = GetKeyValue<float>("SmallGameSpecialEffectDelayClose");

		_specialReelBackEffect = ValueFromKey ("SpecialReelBackEffect");
		_specialReelBackAnim = ValueFromKey ("SpecialReelBackAnim");

		_specialReelBackStartDelay = GetKeyValue<float>("SpecialReelBackStartDelay");

		_specialReelBackIdleAnimTrigger = ValueFromKey ("SpecialReelBackIdleAnimTrigger");

		_collectFlyDuration = GetKeyValue<float>("CollectFlyDuration");
		_collectAnimatorDuration = GetKeyValue<float>("CollectAnimatorDuration");

		_collectSymbolEffect = ValueFromKey ("CollectSymbolEffect");

		_butterFlyStartPos = GetKeyValues<float>("ButterFlyStartPos");
		_butterFlyHitTwinkleEnterDelay = GetKeyValue<float>("ButterFlyHitTwinkleEnterDelay");
		_butterFlyHitTwinkleEnterAndLeaveDelay = GetKeyValue<float>("ButterFlyHitTwinkleEnterAndLeaveDelay");

		_switchSymbolEffectStartDelay = GetKeyValue<float>("SwitchSymbolEffectStartDelay");
		_switchSymbolEffectEndDelay = GetKeyValue<float>("SwitchSymbolEffectEndDelay");
		_switchSymbolDelays = GetKeyValues<float>("SwitchSymbolDelays");

		_backgroundEffectStartDelay = GetKeyValue<float>("BackgroundEffectStartDelay");
		_changeAnim = ValueFromKey("ChangeAnim");

		_mirrorReelShadowUpSortOrder = GetKeyValue<int>("MirrorReelShadowUpSortOrder");
		_showBackgroundEffectWhenFix = GetBoolKeyValue("ShowBackgroundEffectWhenFix");
		_butterflyHitAudio = GetAudioKeyValue("ButterflyHitAudio");
		_butterflyUpAudio = GetAudioKeyValue("ButterflyUpAudio");
		_butterflyDownAudio = GetAudioKeyValue("ButterflyDownAudio");
		_frozenAudio = GetAudioKeyValue("FrozenAudio");
		_breakFrozenAudio = GetAudioKeyValue("BreakFrozenAudio");
		_backgroundEffectMultiType = GetBoolKeyValue("BackgroundEffectMultiType");
		_collectFlyAudio = GetAudioKeyValue("CollectFlyAudio");
		_specialAudio = GetAudioKeyValue("SpecialAudio");
		_backgroundEffectStartOn = GetBoolKeyValue("BackgroundEffectStartOn");
		_hideJackpotScoreWhenSmallGame = GetBoolKeyValue("HideJackpotScoreWhenSmallGame");
		_freespinHintDelay = GetKeyValue<float>("FreespinHintDelay");
		_freespinHintAudio = GetAudioKeyValue("FreespinHintAudio");
		_freespinBGM = GetAudioKeyValue("FreespinBGM");
		_freespinBackgroundEffectType = GetBackgroundTypeKeyValue("FreespinBackgroundEffectType");
		_freespinEffectNoDelay = GetBoolKeyValue("FreespinEffectNoDelay");
		_normalBlurFactor = GetKeyValue<float>("NormalBlurFactor");
		_hypeBlurFactor = GetKeyValue<float>("HypeBlurFactor");
		_enableTriggerSmallGameDelay = GetBoolKeyValue("EnableTriggerSmallGameDelay");
		_triggerSmallGameDelay = GetKeyValue<float>("TriggerSmallGameDelay");
		_enableTriggerSmallGameEffectsDelay = GetBoolKeyValue("EnableTriggerSmallGameEffectsDelay");
		_triggerSmallGameEffectsDelay = GetKeyValue<float>("TriggerSmallGameEffectsDelay");
		_triggerSmallGameEffectsDelayInterval = GetKeyValue<float>("TriggerSmallGameEffectsDelayInterval");
		_enableSpecialSymbolEffectIndex = GetBoolKeyValue("EnableSpecialSymbolEffectIndex");
		_triggerSmallGameEffectsDuration = GetKeyValue<float>("TriggerSmallGameEffectsDuration");
		_triggerSmallGameWinEffectDelay = GetKeyValue<float>("TriggerSmallGameWinEffectDelay");
		_darkSymbolColors = GetKeyValues<float>("DarkSymbolColors");
		_hypeAudios = GetAudioKeyValues("HypeAudios");
		_respinAudio = GetAudioKeyValue("RespinAudio");
	}

	private bool GetBoolValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		CoreDebugUtility.Assert(!string.IsNullOrEmpty(str));
		return int.Parse(str) != 0;
	}

	private string ValueFromKey(string key)
	{
		string result = "";
		int index = ListUtility.Find(_sheet.dataArray, (BasicData data) => {
			return data.Key == key;
		});
		if(index >= 0)
			result = _sheet.dataArray[index].Val;
		return result;
	}

	private AudioType[] GetAudioKeyValues(string key){
		AudioType[] result = new AudioType[0];
		string[] strs = GetKeyValues<string>(key);
		if (strs.Length > 0){
			result = ListUtility.MapList(strs, (string s)=>{
				return TypeUtility.GetEnumFromString<AudioType>(s);
			}).ToArray();
		}
		return result;
	}

	private AudioType GetAudioKeyValue(string key){
		string str = ValueFromKey(key);
		AudioType value = AudioType.None;
		if (!str.IsNullOrEmpty()){
			value = TypeUtility.GetEnumFromString<AudioType>(str);
		}
		return value;
	}

	private BackgroundType GetBackgroundTypeKeyValue(string key){
		string str = ValueFromKey(key);
		BackgroundType value = BackgroundType.Max;
		if (!str.IsNullOrEmpty()){
			value = TypeUtility.GetEnumFromString<BackgroundType>(str);
		}
		return value;
	}

	private bool GetBoolKeyValue(string key){
		string str = ValueFromKey (key);
		bool value = false;
		if (!str.IsNullOrEmpty ()) {
			value = int.Parse (str) != 0;
		}
		return value;
	}

	private T GetKeyValue<T>(string key){
		string str = ValueFromKey (key);
		T value = default(T);
		if (!str.IsNullOrEmpty ()) {
			value = (T)Convert.ChangeType((object)str, typeof(T));
		} 
		return value;
	}

	private T[] GetKeyValues<T>(string key){
		string str = ValueFromKey(key);
		T[] values = new T[0];
		if (!str.IsNullOrEmpty()){
			values = StringUtility.ParseToArray<T>(str);
		}
		return values;
	}
}

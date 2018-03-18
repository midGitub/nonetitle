using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ShowBackgroundEffectType{
	None,// 正常开启
	HighOrSpecialWin,// spin结束时开启
	Respin,// respin开启
	HighWin,
	Max,
}

public enum CloseBackgroundEffectType{
	None,// 正常关闭
	NoRespin,// 只有在非respin时关闭
	Max,
}

public enum CollectionProcessType{
	SingleLocation, //  飞往单一位置的收集
	MultiLocation,// 飞往多位置的收集
	Max,
}

public enum CompassSliderDir{
	Horizontal,// 横排
	Vertical,// 竖排
	Max,
}

public enum HitTwinkleType{
	Enter,// 往上飞
	Leave,// 往下飞
	Max,
}

public class PuzzleDefine
{
	public static float Reel4Scale = 0.77f;
	public static Vector3 SmallGameCanvasLocalPos = new Vector3(0.0f, 0.0f, -2000.0f);
	public static int MaxVisibleSymbolOffsetOfBasicReels = 2;
	public static int MaxVisibleSymbolOffsetOfSubordinateReel4 = 1;

	public static readonly Dictionary<SmallGameState, bool> CanSmallGameStateHypeDict = new Dictionary<SmallGameState, bool>() {
		{SmallGameState.None,		true},
		{SmallGameState.FreeSpin,	false},
		{SmallGameState.Rewind,		false},
		{SmallGameState.FixWild,	false},
		{SmallGameState.Wheel,		false},
		{SmallGameState.Jackpot,	false},
		{SmallGameState.TapBox,		false},
		{SmallGameState.SwitchSymbol, false},
	};

	public enum ReelEffectType{
		NormalWin,
		BigWin,
		Special,
		Max,
	}
}

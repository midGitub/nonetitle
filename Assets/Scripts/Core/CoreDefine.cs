using System.Collections;
using System.Collections.Generic;
using System;

public enum SymbolType
{
	Wild,
	Bonus,
	Jackpot,
	Cherry,
	Seven,
	SevenBar,
	Bar,
	Blank,
	BonusWild,
	Collection,
	// 新加的type需要加在最后，因为symboltype在.asset文件里会被解析成int，如果新加的插在中间的话，那所有的配置表里带有symboltype的项都必须重导。
	Wild7,// 特别的wild,可以替代High7, Mid7, Low7  
	Cherry2,// 经典钻石机台用，与任何7或BAR都不配对，也不会被Wild替换
}

public enum SlideType
{
	None,
	Up,
	Down,
	UpDown
}

// Note:
// UnOrdered:
// - only used in single line machine
// Continuous, Start:
// - only used when method is MultiLineMethod.SymbolProb in multiLine machine
public enum PayoutType
{
	Ordered,
	All,
	Any,
	UnOrdered,
	Continuous,
	Start,
	Count
}

public enum SpinResultType
{
	None = -1,
	Win,
	NearHit,
	Loss
}

public enum SpinDirection
{
	None = -1,
	Up,
	Down
}

public enum CoreCheckMode
{
	All,
	PayoutOnly,
	Count
}

public enum CoreLuckyMode
{
	Normal,
	LongLucky,
	ShortLucky,
	Count
}

public enum CoreLuckySheetMode
{
	Normal,
	Lucky,
	Count
}

public enum JoyType
{
	Payout,
	NearHit,
	Count
}

public enum WinType
{
	None,
	Normal,
	Big,
	Epic,
	Jackpot,
}

public enum NormalWinType
{
	None,
	Low,
	High,
}

public enum CompareType
{
	Equal,
	LessEqual,
	GreaterEqual,
	Count
}

public enum MultiLineMethod
{
	Exhaustive,
	SymbolProb,
}

public enum MachineTheme{
	None,// 无主题
	FireFreespin,// 火主题freespin
	Lighting,// 闪电机台(rewind)
	FourReel,// 四轴机台
	Diamond,// 钻石（slide）
	Cinderella,// 灰姑娘(收集+freeespin）
	Multiline,// 多线
	FrozenWorld,// 冰雪（fixwild）
	EygptJackpot,// 埃及jackpot
	BonusWheel,// 双转盘 m10
	FastWin,// 快速freespin
	Dragon,// 龙主题（双轴freespin)
	FishHunting, // 捕鱼主题（fixwild）
	ButterFly,// 蝴蝶主题 （1 shot freespin， =  fixcount)
	American,// 美国主题 m16
	Neon,// 霓虹灯 m21
	Clover,// 四叶草 m23
	Fruit,// 水果 m22
	ClassicDaimond,// 经典钻石m28
	NoneTwo, // 无主题2 m29
	Kongfu, // 功夫主题 m18
	Bell,  // 铃铛主题 m32
	MultilineTwo, // 经典9线无主题 m27
	Pirates,// 海盗主题 m30
	Piggy, // 小猪转盘 m15
	China,// 中国主题 m24
	DemonAngel,// 天使和魔鬼 m34
	ColomboDay, // 哥伦布日 m36
	Basketball,// 篮球 m37
	Halloween,// 万圣节 m40
	Clown,// 小丑 m26
	Vegas,// 赌城 m20
	Thanksgiving,// 感恩节 m41
	JingleBell,// 铃铛 m17
	Christmas,// 圣诞节 m42
	Mining,// 挖矿 m25
	Rainbow,// 彩虹 m31
	Ninja,// 忍者 m19
	WheelOfDiamond, //钻石转盘 m44
	GoldApple, // 金苹果 v5
	Classic7, // 经典7 v1
	Crazy7, // 狂暴7 v2
	DiamondJackpot, //钻石Jackpot v3
	VIPRainbow, //VIP彩虹 v4
	FiveReelFire,// 五卷轴火主题 m47
	PresidentDay,// 总统日 m46
	WildPoker,// 扑克，m38
	Max,
}

public static class CoreDefine
{
	public static readonly int MinReelCount = 3;
	public static readonly int MaxReelCount = 5;
	public static readonly int Reel3 = 3;
	public static readonly int Reel4 = 4;
	public static readonly int Reel5 = 5;
	public static readonly int InvalidIndex = -1;
	public static readonly uint DefaultMachineRandSeed = 1;
	public static readonly int PaylineHorizonCount = 3;
	public static readonly int UnorderSymbolCount = 3;
	public static readonly string WildCardSymbol = "@";
	public static readonly int MaxPaylineCountOfMultiLine3Reels = 9;

	public static readonly SymbolType[] SevenBarTypes = new SymbolType[]{SymbolType.Seven, SymbolType.Bar, SymbolType.SevenBar};

	public static readonly Dictionary<SymbolType, SymbolType[]> SymbolTypeMatchDict = new Dictionary<SymbolType, SymbolType[]> {
		{SymbolType.Wild, 		new SymbolType[] {SymbolType.Wild} },
		{SymbolType.Wild7, 		new SymbolType[] {SymbolType.Wild7} },
		{SymbolType.Bonus, 		new SymbolType[] {SymbolType.Bonus} },
		{SymbolType.BonusWild, 	new SymbolType[] {SymbolType.BonusWild, SymbolType.Bonus, SymbolType.Wild} },
		{SymbolType.Jackpot, 	new SymbolType[] {SymbolType.Jackpot} },
		{SymbolType.Cherry, 	new SymbolType[] {SymbolType.Cherry} },
		{SymbolType.Seven, 		new SymbolType[] {SymbolType.Seven} },
		{SymbolType.Bar, 		new SymbolType[] {SymbolType.Bar} },
		{SymbolType.SevenBar, 	new SymbolType[] {SymbolType.SevenBar, SymbolType.Seven, SymbolType.Bar} },
		{SymbolType.Blank, 		new SymbolType[] {SymbolType.Blank} },
		{SymbolType.Cherry2, 	new SymbolType[] {SymbolType.Cherry2} }
	};

	public static readonly Dictionary<SymbolType, SymbolType[]> SymbolTypeReplaceDict = new Dictionary<SymbolType, SymbolType[]> {
		{SymbolType.Wild, 		new SymbolType[] {SymbolType.Seven, SymbolType.Bar, SymbolType.SevenBar} },
		{SymbolType.Wild7, 		new SymbolType[] {SymbolType.Seven} },
	};

	public static readonly SymbolType[] Wild7ExcludeSymbols = new SymbolType[] {
		SymbolType.Bar, SymbolType.SevenBar
	};

	public static readonly Dictionary<string, MachineTheme> MachineThemeDict = new Dictionary<string, MachineTheme>(){
		{"M1", MachineTheme.None},
		{"M2", MachineTheme.FireFreespin},
		{"M3", MachineTheme.Lighting},
		{"M4", MachineTheme.FourReel},
		{"M5", MachineTheme.Diamond},
		{"M6", MachineTheme.Cinderella},
		{"M7", MachineTheme.Multiline},
		{"M8", MachineTheme.FrozenWorld},
		{"M9", MachineTheme.EygptJackpot},
		{"M10", MachineTheme.BonusWheel},
		{"M11", MachineTheme.FastWin},
		{"M12", MachineTheme.Dragon},
		{"M13", MachineTheme.FishHunting},
		{"M14", MachineTheme.ButterFly},
		{"M16", MachineTheme.American},
		{"M21", MachineTheme.Neon},
		{"M23", MachineTheme.Clover},
		{"M22", MachineTheme.Fruit},
		{"M28", MachineTheme.ClassicDaimond},
		{"M29", MachineTheme.NoneTwo},
		{"M18", MachineTheme.Kongfu},
		{"M32", MachineTheme.Bell},
		{"M27", MachineTheme.MultilineTwo},
		{"M30", MachineTheme.Pirates},
		{"M15", MachineTheme.Piggy},
		{"M24", MachineTheme.China},
		{"M34", MachineTheme.DemonAngel},
		{"M36", MachineTheme.ColomboDay},
		{"M37", MachineTheme.Basketball},
		{"M40", MachineTheme.Halloween},
		{"M26", MachineTheme.Clown},
		{"M20", MachineTheme.Vegas},
		{"M41", MachineTheme.Thanksgiving},
		{"M17", MachineTheme.JingleBell},
		{"M42", MachineTheme.Christmas},
		{"M25", MachineTheme.Mining},
		{"M31", MachineTheme.Rainbow},
		{"M19", MachineTheme.Ninja},
		{"M44", MachineTheme.WheelOfDiamond},
		{"V5", MachineTheme.GoldApple},
		{"V1", MachineTheme.Classic7},
		{"V2", MachineTheme.Crazy7},
		{"V3", MachineTheme.DiamondJackpot},
		{"V4", MachineTheme.VIPRainbow},
		{"M47", MachineTheme.FiveReelFire},
		{"M46", MachineTheme.PresidentDay},
		{"M38", MachineTheme.WildPoker},
	};

	public static readonly string[] AllMachineNames = new string[]{"M1", "M2", "M3", "M4", "M5", "M6", "M7", "M8", "M9",
		"M10", "M11", "M12", "M13", "M14", "M16", "M21", "M23", "M22", "M28", "M29", "M18", "M32", "M27", "M30",
		"M15", "M24", "M34", "M36", "M37", "M40", "M26", "M20", "M41", "M17", "M42", "M25", "M31", "M19", "M44", 
		"V5", "V1", "V2", "V3", "V4", "M47", "M46", "M38"};
	public static readonly string[] AllMachineNamesVersion1_3 = new string[]{"M1", "M2", "M3", "M4", "M5", "M6", "M7", "M8", "M9", "M10"};

	public static readonly string[] singleJackpotMachines = new string[]{"M16", "M41", "V5"};
	public static readonly string[] fourJackpotMachines = new string[]{"M9", "M18", "V3"};
}
// 
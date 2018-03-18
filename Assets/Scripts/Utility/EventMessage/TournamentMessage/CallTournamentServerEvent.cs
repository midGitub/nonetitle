using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class CallTournamentServerEvent : CitrusGameEvent
{
	public TournamentServerFunction FunctionName;
	public int Bet;
	public ulong AllScore, WinScore;

	public CallTournamentServerEvent(TournamentServerFunction tf) : base()
	{
		FunctionName = tf;
	}

	public CallTournamentServerEvent(TournamentServerFunction tf, int bet) : base()
	{
		Bet = bet;
		FunctionName = tf;
	}

	public CallTournamentServerEvent(TournamentServerFunction tf, int bet, ulong score, ulong winScore) : base()
	{

		Bet = bet;
		AllScore = score;
		WinScore = winScore;
		FunctionName = tf;
	}
}

using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class TournamentServerErrorEvent : CitrusGameEvent
{
	public TournamentServerFunction ErrorFunction;
	public string Error = "";
	public TournamentServerErrorEvent(TournamentServerFunction sf, string error) : base()
	{
		Error = error;
		ErrorFunction = sf;
	}
}

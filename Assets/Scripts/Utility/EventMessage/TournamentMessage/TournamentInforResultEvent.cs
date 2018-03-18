using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class TournamentInforResultEvent : CitrusGameEvent 
{
	public TournamentInformation Infor;

	public TournamentInforResultEvent(TournamentInformation tf):base()
	{
		Infor = tf;
	}
}

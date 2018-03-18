using System.Collections;
using System.Collections.Generic;

public class JackpotPoolFactory  {
	public static JackpotBonusPool CreateBonusPool(JackpotPoolType type){
		JackpotBonusPool pool = null;
		switch (type) {
		case JackpotPoolType.None:
			CoreDebugUtility.Log ("wrong pool name "+type.ToString());
			break;
		case JackpotPoolType.Single:
			pool = new JackpotBonusSinglePool ();
			break;
		case JackpotPoolType.Colossal:
			pool = new JackpotBonusColossalPool ();
			break;
		case JackpotPoolType.Mega:
			pool = new JackpotBonusMegaPool ();
			break;
		case JackpotPoolType.Huge:
			pool = new JackpotBonusHugePool ();
			break;
		case JackpotPoolType.Big:
			pool = new JackpotBonusBigPool ();
			break;
		default:
			CoreDebugUtility.Log ("not suitable pool name "+type.ToString());
			break;
		}
		return pool;
	}
}

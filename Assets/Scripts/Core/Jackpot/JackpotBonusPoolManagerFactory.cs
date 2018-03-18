using System.Collections;
using System.Collections.Generic;

public class JackpotBonusPoolManagerFactory {

	public static JackpotBonusPoolManager CreateManager(string name){
		if (name.Equals ("M9")) {
			return new JackpotBonusPoolManager (name);
		} else if (name.Equals ("M16")) {
			return new JackpotBonusPoolManagerM16 (name);
		} else if (name.Equals ("M18")) {
			return new JackpotBonusPoolManagerM18 (name);
		} else if (name.Equals ("M41")) {
			return new JackpotBonusPoolManagerM41 (name);
		} else if (name.Equals ("V5")) {
			return new JackpotBonusPoolManagerV5 (name);
		} else if (name.Equals ("V3")) {
			return new JackpotBonusPoolManagerV3 (name);
		}

		CoreDebugUtility.LogError ("JackpotBonusPoolManagerFactory create failed "+name);
		return null;
	}

}

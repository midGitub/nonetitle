using System.Collections;
using System.Collections.Generic;

public class JackpotBonusPoolManagerM16 : JackpotBonusPoolManager {
	public JackpotBonusPoolManagerM16(string name) : base(name){
		_jackpotType = JackpotType.Single;
	}
}

public class JackpotBonusPoolManagerM18 : JackpotBonusPoolManager {
	public JackpotBonusPoolManagerM18(string name) : base(name){
		_jackpotType = JackpotType.FourJackpot;
	}
}

public class JackpotBonusPoolManagerM41 : JackpotBonusPoolManager {
	public JackpotBonusPoolManagerM41(string name) : base(name){
		_jackpotType = JackpotType.Single;
	}
}

public class JackpotBonusPoolManagerV5 : JackpotBonusPoolManager {
	public JackpotBonusPoolManagerV5(string name) : base(name){
		_jackpotType = JackpotType.Single;
	}
}

public class JackpotBonusPoolManagerV3 : JackpotBonusPoolManager {
	public JackpotBonusPoolManagerV3(string name) : base(name){
		_jackpotType = JackpotType.FourJackpot;
	}
}
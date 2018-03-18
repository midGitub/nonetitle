using NUnit.Framework;
using System;

namespace NUnitTest
{
	[TestFixture]
	public class Test
	{
		private Config _config;

		[Test]
		public void TestCases()
		{
			_config = ConfigHelper.GetConfig();

			foreach(var caseConfig in _config._caseConfigs)
			{
				UserConfig userConfig = _config._userConfigDict[caseConfig._userName];
				MachineTestEngine engine = new MachineTestEngine(caseConfig._machineName, userConfig);
				engine.RunSingleCase();
			}
		}
	}
}


using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace SXN.Azure
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public sealed class RelaySettingsTest
	{
		#region Nested Types

		[ServiceContract(Namespace = "urn:ps")]
		private interface IProblemSolver
		{
			[OperationContract]
			int AddNumbers(int a, int b);
		}

		private interface IProblemSolverChannel : IProblemSolver, IClientChannel
		{
		}

		private class ProblemSolver : IProblemSolver
		{
			public int AddNumbers(int a, int b)
			{
				return a + b;
			}
		}

		#endregion

		[TestMethod]
		[TestCategory("Manual")]
		public void ParseUnitTest()
		{
			var settings = new RelaySettings
			{
				EnvironmentId = "test",
				ServiceNamespaceRoot = "-sniffer",
				Token = new SharedAccessSignatureTokenSettings
				{
					KeyName = "RootManageSharedAccessKey",
					SharedAccessKey = "o6SrAGM016UBMPFZkYMf7DFfGUlVOBLZSx7sxrUFjWw="
				}
			};

			var s = JsonConvert.SerializeObject(settings);

			var sh = new ServiceHost(typeof(ProblemSolver));

			settings.ConfigureServiceHost(sh, typeof(IProblemSolver), "deployment24187\\deployment24187snifferservicesnifferrolein0", 10100);

			sh.Open();

			sh.Close();
		}
	}
}
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a set of tests for <see cref="GlobalTimeDataStorage{T}"/> class.
	/// </summary>
	[TestClass]
	public sealed class GlobalTimeDataStorageTests
	{
		#region Fields

		private readonly GlobalTimeDataStorageSettings settings;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="TimeDataStorageTests"/> class.
		/// </summary>
		public GlobalTimeDataStorageTests()
		{
			settings = new GlobalTimeDataStorageSettings
			{
				RowKeyFormats = new Dictionary<TimeUnit, String>
				{
					{
						TimeUnit.Minute, @"yyyy-MM-dd-HH-mm"
					},
					{
						TimeUnit.Hour, @"yyyy-MM-dd-HH"
					},
					{
						TimeUnit.Day, @"yyyy-MM-dd"
					}
				},
				TablesNameRoot = @"localTestsAggregate",
				StoragesCredentials = new Dictionary<AzureRegion, String>
				{
					{
						AzureRegion.None, @"UseDevelopmentStorage=true"
					}
				}
			};
		}

		#endregion

		#region Test methods

		[TestMethod]
		[TestCategory("UnitTests")]
		public void SettingsSerialization()
		{
			var expectedResult = settings;

			var intermediateResult = JsonConvert.SerializeObject(settings, new StringEnumConverter());

			var actualResult = JsonConvert.DeserializeObject<GlobalTimeDataStorageSettings>(intermediateResult);

			Assert.AreEqual(expectedResult.TablesNameRoot, actualResult.TablesNameRoot);
		}

		#endregion
	}
}
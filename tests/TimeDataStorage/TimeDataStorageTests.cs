using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a set of tests for <see cref="TimeDataStorage{TEntity}"/> class.
	/// </summary>
	[TestClass]
	public sealed class TimeDataStorageTests
	{
		#region Constant and Static Fields

		private const Int32 campaignsCount = 16;

		private static readonly TimeInterval testTimeInterval = new TimeInterval(TimeUnit.Minute, 32);

		#endregion

		#region Fields

		private readonly Value128[] campaigns;

		private readonly Random random = new Random(DateTime.UtcNow.Millisecond);

		private readonly TimeDataStorageSettings settings;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="TimeDataStorageTests"/> class.
		/// </summary>
		public TimeDataStorageTests()
		{
			settings = new TimeDataStorageSettings
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
				//@"local-TestsAggregate"
				//@"DefaultEndpointsProtocol=https;AccountName=devsniffer;AccountKey=+Xe9sNsAvWipRfMlgGSsvfteqW4rnOzkY1MDtT4YpHDIuGRtMVE0aTfEplCa/LwrXcClpbAEBjHmbSKYxOvSwQ==",
				TablesNameRoot = @"localCampaignsCounters",
				StorageCredentials = @"UseDevelopmentStorage=true"
			};

			// Generate campaigns
			campaigns = new Value128[campaignsCount];

			for (var index = 0; index < campaignsCount; index++)
			{
				campaigns[index] = (Value128) Guid.NewGuid();
			}

			var storageAccount = CloudStorageAccount.Parse(settings.StorageCredentials);

			ServicePointManager.FindServicePoint(storageAccount.TableEndpoint).ConnectionLimit = 1024;
		}

		#endregion

		#region Test methods

		[TestMethod]
		[TestCategory("UnitTests")]
		public void SettingsSerialization()
		{
			var expectedResult = settings;

			var intermediateResult = JsonConvert.SerializeObject(settings, new StringEnumConverter());

			var actualResult = JsonConvert.DeserializeObject<TimeDataStorageSettings>(intermediateResult);

			Assert.AreEqual(expectedResult.TablesNameRoot, actualResult.TablesNameRoot);

			Assert.IsTrue(expectedResult.RowKeyFormats.SafeSequenceEqual(actualResult.RowKeyFormats));
		}

		[TestMethod]
		[TestCategory("ManualTests")]
		public async Task InsertAndRetrieveAsync()
		{
			var cancellationToken = CancellationToken.None;

			var cumulativeTable = new TimeDataStorage<TestCounter>(settings, new TestCounterProvider());

			try
			{
				await cumulativeTable.CreateTablesIfNotExistAsync(cancellationToken);
			}
			catch (StorageException e)
			{
				Trace.TraceWarning(e.Message);
			}

			var testSamples = GenerateTestSamples();

			// 0. Insert data
			var stopWatch = Stopwatch.StartNew();

			await cumulativeTable.InsertAsync(TimeUnit.Minute, testSamples, cancellationToken);

			stopWatch.Stop();

			Trace.TraceInformation("Insert time :{0} ms. Count of inserted items: {1}. Average velocity is {2:N0} per second.", stopWatch.ElapsedMilliseconds, testSamples.Count, testSamples.Count / stopWatch.Elapsed.TotalSeconds);

			// 1. Retrieve data
			var minuteInterval = testSamples.Range(x => x.Time);

			var retrieveTasks = new Task<IReadOnlyList<TestCounter>>[campaignsCount];

			// Start stopwatch
			stopWatch = Stopwatch.StartNew();

			var hourInterval = new Range<DateTime>(minuteInterval.Begin.Floor(TimeUnit.Hour), minuteInterval.End.Ceiling(TimeUnit.Hour));

			for (var campaignIndex = 0; campaignIndex < campaigns.Length; campaignIndex++)
			{
				var partitionKey = campaigns[campaignIndex].ToString();

				retrieveTasks[campaignIndex] = cumulativeTable.RetrieveAsync(TimeUnit.Minute, hourInterval, partitionKey, cancellationToken);
			}

			// Await retrieve tasks to complete
			await Task.WhenAll(retrieveTasks);

			// Stop stopwatch
			stopWatch.Stop();

			var totalRetrieveItemsCount = retrieveTasks.Sum(x => x.Result.Count);

			Trace.TraceInformation("Retrieve time :{0} ms. Count of retrieved items: {1}. Average velocity is {2:N0} per second.", stopWatch.ElapsedMilliseconds, totalRetrieveItemsCount, totalRetrieveItemsCount / stopWatch.Elapsed.TotalSeconds);

			Assert.AreEqual(testSamples.Count, totalRetrieveItemsCount);
		}

		[TestMethod]
		[TestCategory("ManualTests")]
		public async Task InsertAndUpdateAndRetrieveAsync()
		{
			var cancellationToken = CancellationToken.None;

			var cumulativeTable = new TimeDataStorage<TestCounter>(settings, new TestCounterProvider());

			try
			{
				await cumulativeTable.CreateTablesIfNotExistAsync(cancellationToken);
			}
			catch (StorageException e)
			{
				Trace.TraceWarning(e.Message);
			}

			var testSamples = GenerateTestSamples();

			// 0. Write data
			var stopWatch = Stopwatch.StartNew();

			await cumulativeTable.InsertAsync(TimeUnit.Minute, testSamples, cancellationToken);

			stopWatch.Stop();

			Trace.TraceInformation("Put time :{0} ms. Count of putted items: {1}. Average velocity is {2:N0} per second.", stopWatch.ElapsedMilliseconds, testSamples.Count, testSamples.Count / stopWatch.Elapsed.TotalSeconds);

			// 1. Update data
			var updateData = new[]
			{
				testSamples[0], testSamples[1]
			};

			stopWatch = Stopwatch.StartNew();

			var updateResult = (await cumulativeTable.UpdateAsync(TimeUnit.Minute, updateData, cancellationToken)).ToArray();

			stopWatch.Stop();

			Trace.TraceInformation("Update time :{0} ms. Count of updated items: {1}. Average velocity is {2:N2} per second.", stopWatch.ElapsedMilliseconds, updateResult.Length, updateResult.Length / stopWatch.Elapsed.TotalSeconds);

			// 2. Read updated data
			var updateCheckReadResult = await cumulativeTable.TryRetrieveAsync(TimeUnit.Minute, testSamples[0].CampaignId.ToString(), testSamples[0].Time, cancellationToken);

			Assert.IsTrue(updateCheckReadResult.Success);

			Assert.AreEqual(2, updateCheckReadResult.Result.Count);
		}

		#endregion

		#region Private methods

		private IReadOnlyList<TestCounter> GenerateTestSamples()
		{
			var rowKeyFormat = settings.RowKeyFormats[testTimeInterval.Unit];

			var sampleTime = DateTime.UtcNow.Floor(TimeUnit.Day) + new TimeInterval(TimeUnit.Day, random.Next(365));

			Trace.TraceInformation("Test samples time starts at: {0}", sampleTime.ToString(rowKeyFormat));

			var timeIncrement = new TimeInterval(testTimeInterval.Unit);

			var testData = new List<TestCounter>((Int32) (campaignsCount * testTimeInterval.Length));

			for (var timeIndex = 0; timeIndex < testTimeInterval.Length; timeIndex++)
			{
				for (var index = 0ul; index < campaignsCount; index++)
				{
					var testCounter = new TestCounter
					{
						CampaignId = campaigns[index],
						Time = sampleTime,
						Count = 1
					};

					testData.Add(testCounter);
				}

				sampleTime += timeIncrement;
			}

			Trace.TraceInformation("Test samples time ends at: {0}", sampleTime.ToString(rowKeyFormat));

			Trace.TraceInformation("Test samples count is: {0} and takes {1}", testData.Count, testTimeInterval);

			return testData.ToArray();
		}

		#endregion
	}
}
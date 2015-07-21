using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a set of methods to work with time-data stored in the Azure Table Storage.
	/// </summary>
	/// <typeparam name="T">The type of the time-data entities stored in the the Azure Table Storage.</typeparam>
	public sealed class GlobalTimeDataStorage<T> : TimeDataStorageBase<T>
	{
		#region Fields

		private readonly IReadOnlyDictionary<TimeUnit, IReadOnlyDictionary<AzureRegion, CloudTable>> tables;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="GlobalTimeDataStorage{TEntity}"/> class.
		/// </summary>
		/// <param name="settings"> An instance of <see cref="GlobalTimeDataStorageSettings"/> that specifies the configuration settings for instance.</param>
		/// <param name="entityProvider">The entity provider.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GlobalTimeDataStorage(GlobalTimeDataStorageSettings settings, ITimeDataEntityProvider<T> entityProvider)
			: base(settings, entityProvider)
		{
			// Initialize regions - tables dictionary
			var tempTables = new Dictionary<TimeUnit, IReadOnlyDictionary<AzureRegion, CloudTable>>();

			foreach (var partitionKeyFormatPair in rowKeyFormats)
			{
				var timeUnit = partitionKeyFormatPair.Key;

				var regionTables = new Dictionary<AzureRegion, CloudTable>();

				foreach (var pair in settings.StoragesCredentials)
				{
					// Get cloud storage account
					var cloudStorageAccount = CloudStorageAccount.Parse(pair.Value);

					// Create cloud table client
					var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

					// Get table name
					var tableName = settings.GetTableName(timeUnit);

					// Get table references
					var table = cloudTableClient.GetTableReference(tableName);

					// Add table
					regionTables.Add(pair.Key, table);
				}

				// Add to dictionary
				tempTables.Add(timeUnit, new ReadOnlyDictionary<AzureRegion, CloudTable>(regionTables));
			}

			// Initialize tables dictionary
			tables = new ReadOnlyDictionary<TimeUnit, IReadOnlyDictionary<AzureRegion, CloudTable>>(tempTables);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Initiates an asynchronous operation to create the tables if they does not already exist.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task"/> object that represents the current operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task CreateTablesIfNotExistAsync(CancellationToken cancellationToken)
		{
			return Task.WhenAll
				(
					from
						timeUnitTables in tables.Values
					from
						table in timeUnitTables.Values
					select
						table.CreateIfNotExistsAsync(cancellationToken)
				);
		}

		/// <summary>
		/// Initiates an asynchronous operation to retrieve entities from the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="timeInterval">The time timeInterval.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IEnumerable{TData}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to enumeration of read data.
		/// </returns>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IDictionary<AzureRegion, IReadOnlyList<T>>> RetrieveAsync(TimeUnit timeUnit, Range<DateTime> timeInterval, CancellationToken cancellationToken)
		{
			// Create retrieve query
			var query = new TableQuery<DynamicTableEntity>
			{
				FilterString = CreateRetrieveFilter(timeUnit, timeInterval)
			};

			// Get time unit tables
			var timeUnitTables = tables[timeUnit];

			// Initiate retrieve
			var regionsTasks = timeUnitTables.ToDictionary(pair => pair.Key, pair => RetrieveAsync(pair.Value, timeUnit, query, cancellationToken));

			// Wait tasks to complete
			await Task.WhenAll(regionsTasks.Values);

			// Return result
			return regionsTasks.ToDictionary(pair => pair.Key, pair => pair.Value.Result);
		}

		/// <summary>
		/// Initiates an asynchronous operation to retrieve entities from the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="timeInterval">The time timeInterval.</param>
		/// <param name="partitionKey">The key of the partition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IEnumerable{TData}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to enumeration of read data.
		/// </returns>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IDictionary<AzureRegion, IReadOnlyList<T>>> RetrieveAsync(TimeUnit timeUnit, Range<DateTime> timeInterval, String partitionKey, CancellationToken cancellationToken)
		{
			// Create retrieve query
			var query = new TableQuery<DynamicTableEntity>
			{
				FilterString = CreateRetrieveFilter(timeUnit, timeInterval, partitionKey)
			};

			// Get time unit tables
			var timeUnitTables = tables[timeUnit];

			// Initiate retrieve
			var regionsTasks = timeUnitTables.ToDictionary(pair => pair.Key, pair => RetrieveAsync(pair.Value, timeUnit, query, cancellationToken));

			// Wait tasks to complete
			await Task.WhenAll(regionsTasks.Values);

			// Return result
			return regionsTasks.ToDictionary(pair => pair.Key, pair => pair.Value.Result);
		}

		/// <summary>
		/// Initiates an asynchronous operation to retrieve entities from the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="exactTime">The exact time.</param>
		/// <param name="partitionKey">The key of the partition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IEnumerable{TData}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to enumeration of read data.
		/// </returns>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IReadOnlyList<KeyValuePair<AzureRegion, T>>> RetrieveAsync(TimeUnit timeUnit, DateTime exactTime, String partitionKey, CancellationToken cancellationToken)
		{
			// Get time unit tables
			var timeUnitTables = tables[timeUnit];

			// Initiate retrieve
			var regionsTasks =
				(
					from
						timeUnitTable in timeUnitTables
					select
						new KeyValuePair<AzureRegion, Task<TryResult<T>>>(timeUnitTable.Key, TryRetrieveAsync(timeUnitTable.Value, timeUnit, partitionKey, exactTime, cancellationToken))
					).ToArray();

			// Wait tasks to complete
			await Task.WhenAll(regionsTasks.Select(x => x.Value));

			// Return result
			return
				(
					from
						pair in regionsTasks
					let
						retrieveResult = pair.Value.Result
					where
						retrieveResult.Success
					select
						new KeyValuePair<AzureRegion, T>(pair.Key, retrieveResult.Result)
					).ToArray();
		}

		#endregion
	}
}
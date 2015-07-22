using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SXN.Azure.Extensions;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a set of methods to work with time-data stored in the Azure Table Storage.
	/// </summary>
	/// <typeparam name="T">The type of the time-data entities stored in the the Azure Table Storage.</typeparam>
	public sealed class TimeDataStorage<T> : TimeDataStorageBase<T>
	{
		#region Fields

		private readonly IReadOnlyDictionary<TimeUnit, CloudTable> tables;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="TimeDataStorage{TEntity}"/> class.
		/// </summary>
		/// <param name="settings"> An instance of <see cref="TimeDataStorageSettings"/> that specifies the configuration settings for instance.</param>
		/// <param name="provider">The entity provider.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TimeDataStorage(TimeDataStorageSettings settings, ITimeDataEntityProvider<T> provider)
			: base(settings, provider)
		{
			// Check argument
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			// Get cloud storage account
			var cloudStorageAccount = CloudStorageAccount.Parse(settings.StorageCredentials);

			// Create cloud table client
			var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

			// Get table references
			var tempTables = settings.RowKeyFormats.Keys.ToDictionary(interval => interval, interval => cloudTableClient.GetTableReference(settings.GetTableName(interval)));

			// Initialize tables dictionary
			tables = new ReadOnlyDictionary<TimeUnit, CloudTable>(tempTables);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Creates an instance of <see cref="DynamicTableEntity"/> from <paramref name="entity"/>.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="entity">The entity to be converted.</param>
		/// <returns>A new table entity.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private DynamicTableEntity CreateTableEntity(TimeUnit timeUnit, T entity)
		{
			// Get partition key
			var partitionKey = provider.GetKey(entity);

			// Get row key format
			var rowKeyFormat = rowKeyFormats[timeUnit];

			// Get time
			var time = provider.GetTime(entity).Floor(timeUnit);

			// Get row key
			var rowKey = time.ToString(rowKeyFormat);

			// Get properties
			var properties = provider.GetProperties(entity);

			// Create new table entity
			return new DynamicTableEntity(partitionKey, rowKey, String.Empty, properties);
		}

		/// <summary>
		/// Gets the instance of <see cref="CloudTable"/>.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private CloudTable GetTable(TimeUnit timeUnit)
		{
			// Try get table
			CloudTable table;

			if (!tables.TryGetValue(timeUnit, out table))
			{
				throw new ArgumentException(@"There is no table for specified Time Unit.", nameof(timeUnit));
			}

			return table;
		}

		/// <summary>
		/// Initiates an asynchronous operation to update entity on the Azure Table Storage.
		/// </summary>
		/// <param name="table">The <see cref="CloudTable"/>.</param>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="partitionKey">The key of the partition.</param>
		/// <param name="time">The time of the <paramref name="updateEntity"/>.</param>
		/// <param name="updateEntity">The entity that holds update information.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task"/> object that represents the current operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task<TableResult> UpdateAsync(CloudTable table, TimeUnit timeUnit, String partitionKey, DateTime time, T updateEntity, CancellationToken cancellationToken)
		{
			// Retrieve entity from the storage
			var retrieveResult = await TryRetrieveAsync(table, timeUnit, partitionKey, time, cancellationToken);

			if (retrieveResult.Success)
			{
				var currentEntity = retrieveResult.Result;

				// Update
				var updateResult = provider.TryUpdate(currentEntity, updateEntity);

				if (updateResult.Success)
				{
					// Create new table entity
					var updatedTableEntity = CreateTableEntity(timeUnit, updateResult.Result);

					// Execute update operation
					return await table.ExecuteAsync(TableOperationType.InsertOrReplace, updatedTableEntity, cancellationToken);
				}
			}

			// Create table entity
			var tableEntity = CreateTableEntity(timeUnit, updateEntity);

			// Execute update operation
			return await table.ExecuteAsync(TableOperationType.InsertOrReplace, tableEntity, cancellationToken);
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
						table in tables.Values
					select
						table.CreateIfNotExistsAsync(cancellationToken)
				);
		}

		/// <summary>
		/// Initiates an asynchronous operation to insert time-data entities into the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="entities">The collection of <typeparamref name="T"/> to put.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task"/> object that represents the current operation.</returns>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task InsertAsync(TimeUnit timeUnit, IEnumerable<T> entities, CancellationToken cancellationToken)
		{
			// Get table
			var table = GetTable(timeUnit);

			// Create table entities
			var tableEntities =
				from
					dataItem in entities
				select
					CreateTableEntity(timeUnit, dataItem);

			// Initiate table write operation and return task
			return table.ExecuteGroupBatchAsync(TableOperationType.InsertOrReplace, tableEntities, cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to retrieve entities from the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="timeInterval">The time interval.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IReadOnlyList{TData}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to the collection of read data items.
		/// </returns>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<IReadOnlyList<T>> RetrieveAsync(TimeUnit timeUnit, Range<DateTime> timeInterval, CancellationToken cancellationToken)
		{
			// Get table
			var table = GetTable(timeUnit);

			// Create retrieve query
			var query = new TableQuery<DynamicTableEntity>
			{
				FilterString = CreateRetrieveFilter(timeUnit, timeInterval)
			};

			// Initiate retrieve
			return RetrieveAsync(table, timeUnit, query, cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to retrieve entities from the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="timeInterval">The time interval.</param>
		/// <param name="partitionKey">The key of the partition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IReadOnlyList{TData}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to the collection of read data items.
		/// </returns>
		/// <exception cref="ArgumentException">If there is no table for specified <paramref name="timeUnit"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<IReadOnlyList<T>> RetrieveAsync(TimeUnit timeUnit, Range<DateTime> timeInterval, String partitionKey, CancellationToken cancellationToken)
		{
			// Get table
			var table = GetTable(timeUnit);

			// Create retrieve query
			var query = new TableQuery<DynamicTableEntity>
			{
				FilterString = CreateRetrieveFilter(timeUnit, timeInterval, partitionKey)
			};

			// Initiate retrieve
			return RetrieveAsync(table, timeUnit, query, cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to try retrieve time-data entity from the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="partitionKey">The key of the partition.</param>
		/// <param name="exactTime">The exact time of the data.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IReadOnlyList{TData}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to the result of the operation.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<TryResult<T>> TryRetrieveAsync(TimeUnit timeUnit, String partitionKey, DateTime exactTime, CancellationToken cancellationToken)
		{
			// Get table
			var table = GetTable(timeUnit);

			// Initiate retrieve
			return TryRetrieveAsync(table, timeUnit, partitionKey, exactTime, cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to update entities on the Azure Table Storage.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="entities">The entities to be update.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task"/> object that represents the current operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IEnumerable<TableResult>> UpdateAsync(TimeUnit timeUnit, IEnumerable<T> entities, CancellationToken cancellationToken)
		{
			// Get table
			var table = GetTable(timeUnit);

			var tasks =
				(
					from
						entity in entities
					group
						entity by new
						{
							PartitionKey = provider.GetKey(entity),
							Time = provider.GetTime(entity)
						} into entitiesGroup
					from
						gEntity in entitiesGroup
					select
						UpdateAsync(table, timeUnit, entitiesGroup.Key.PartitionKey, entitiesGroup.Key.Time, gEntity, cancellationToken)
					).ToArray();

			// Await tasks to complete
			await Task.WhenAll(tasks);

			// Return combined result
			return tasks.Select(task => task.Result);
		}

		#endregion
	}
}
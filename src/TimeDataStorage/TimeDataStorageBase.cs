using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using SXN.Azure.Extensions;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a base class for types that works with time-data stored in the Azure Table Storage.
	/// </summary>
	/// <typeparam name="T">The type of the time-data entities stored in the the Azure Table Storage.</typeparam>
	public abstract class TimeDataStorageBase<T>
	{
		#region Constant and Static Fields

		private static readonly TryResult<T> tryResolveFailResult = TryResult<T>.CreateFail();

		#endregion

		#region Fields

		/// <summary>
		/// The time-data entity provider.
		/// </summary>
		protected readonly ITimeDataEntityProvider<T> provider;

		/// <summary>
		/// The collection of row key formats.
		/// </summary>
		protected readonly IReadOnlyDictionary<TimeUnit, String> rowKeyFormats;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="TimeDataStorageBase{TTimeDataEntity}"/> class.
		/// </summary>
		/// <param name="settings"> An instance of <see cref="TimeDataStorageSettings"/> that specifies the configuration settings for instance.</param>
		/// <param name="provider">The time-data entity provider.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected TimeDataStorageBase(TimeDataStorageSettingsBase settings, ITimeDataEntityProvider<T> provider)
		{
			// Check argument
			settings.CheckArgument(@"settings");

			// Check and set
			this.provider = provider.CheckArgument(@"provider");

			// Initialize row keys formats dictionary
			rowKeyFormats = new ReadOnlyDictionary<TimeUnit, String>(settings.RowKeyFormats);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Tries to resolve the instance of <see cref="DynamicTableEntity"/> and get instance of <typeparamref name="T"/>.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="tableEntity">The table entity to resolve.</param>
		/// <returns>
		/// An instance of <see cref="TryResult{TResult}"/> which encapsulates result of the operation.
		/// <see cref="TryResult{TResult}.Success"/> contains <c>true</c> if operation was successful, <c>false</c> otherwise.
		/// <see cref="TryResult{TResult}.Result"/> valid <typeparamref name="T"/> object if operation was successful, <c>default</c>(<typeparamref name="T"/>) otherwise.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TryResult<T> TryResolveTableEntity(TimeUnit timeUnit, DynamicTableEntity tableEntity)
		{
			// Check arguments
			if (tableEntity == null)
			{
				return tryResolveFailResult;
			}

			// Try get row key format
			String rowKeyFormat;

			if (!rowKeyFormats.TryGetValue(timeUnit, out rowKeyFormat))
			{
				return tryResolveFailResult;
			}

			// Try get time
			DateTime time;

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (!DateTime.TryParseExact(tableEntity.RowKey, rowKeyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
			{
				return tryResolveFailResult;
			}

			// Try resolve entity via provider
			return provider.TryResolve(tableEntity.PartitionKey, time, tableEntity.Properties);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates the filter for the retrieve query.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="timeInterval">The time interval.</param>
		/// <returns>The query filter.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public String CreateRetrieveFilter(TimeUnit timeUnit, Range<DateTime> timeInterval)
		{
			// Get row key format
			var rowKeyFormat = rowKeyFormats[timeUnit];

			return TableQuery.CombineFilters
				(
					TableQuery.GenerateFilterCondition(@"RowKey", QueryComparisons.GreaterThanOrEqual, timeInterval.Begin.ToString(rowKeyFormat)),
					TableOperators.And,
					TableQuery.GenerateFilterCondition(@"RowKey", QueryComparisons.LessThan, timeInterval.End.ToString(rowKeyFormat))
				);
		}

		/// <summary>
		/// Creates the filter for the retrieve query.
		/// </summary>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="timeInterval">The time interval.</param>
		/// <returns>The query filter.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public String CreateRetrieveFilter(TimeUnit timeUnit, Range<DateTime> timeInterval, String partitionKey)
		{
			return TableQuery.CombineFilters
				(
					TableQuery.GenerateFilterCondition(@"PartitionKey", QueryComparisons.Equal, partitionKey),
					TableOperators.And,
					CreateRetrieveFilter(timeUnit, timeInterval)
				);
		}

		/// <summary>
		/// Initiates an asynchronous operation to retrieve entities from the Azure Table Storage.
		/// </summary>
		/// <param name="table">The cloud table.</param>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="query">The table query.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IReadOnlyList{T}"/> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> refers to the collection of read data items.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected async Task<IReadOnlyList<T>> RetrieveAsync(CloudTable table, TimeUnit timeUnit, TableQuery<DynamicTableEntity> query, CancellationToken cancellationToken)
		{
			// Execute query
			var tableEntities = await table.ExecuteQueryAsync(query, cancellationToken);

			// Convert to array to avoid capturing of variables
			return
				(
					from
						tableEntity in tableEntities
					let
						resolverResult = TryResolveTableEntity(timeUnit, tableEntity)
					where
						resolverResult.Success
					select
						resolverResult.Result
					).ToArray();
		}

		/// <summary>
		/// Initiates an asynchronous operation to try retrieve entity from the Azure Table Storage.
		/// </summary>
		/// <param name="table">The cloud table.</param>
		/// <param name="timeUnit">The time unit.</param>
		/// <param name="partitionKey">The key of the partition.</param>
		/// <param name="exactTime">The exact time of the data.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="TryResult{T}"/> that represents the asynchronous operation.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected async Task<TryResult<T>> TryRetrieveAsync(CloudTable table, TimeUnit timeUnit, String partitionKey, DateTime exactTime, CancellationToken cancellationToken)
		{
			// Get row key format
			var rowKeyFormat = rowKeyFormats[timeUnit];

			// Create retrieve operation
			var retrieveTableOperation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, exactTime.ToString(rowKeyFormat));

			// Execute retrieve operation
			var readResult = await table.ExecuteAsync(retrieveTableOperation, cancellationToken);

			// Return result
			return TryResolveTableEntity(timeUnit, readResult.Result as DynamicTableEntity);
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Table
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="CloudTable"/> class.
	/// </summary>
	public static class CloudTableEx
	{
		#region Constant and Static Fields

		/// <summary>
		/// The maximal count of operations in the table batch.
		/// </summary>
		private const Int32 tableBatchMaxOperationsCount = 100;

		/// <summary>
		/// The maximal total payload of the table batch.
		/// </summary>
		private const Int32 tableBatchMaxSize = 4194304;

		#endregion

		#region Private methods

		/// <summary>
		/// Initiates an asynchronous operation to execute the <paramref name="query"/> on the <paramref name="table"/>.
		/// </summary>
		/// <param name="table">The instance of <see cref="CloudTable"/>.</param>
		/// <param name="query">The <see cref="TableQuery"/> representing the query to execute.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IEnumerable{TEntity}"/>> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> contain an enumerable collection of results.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static async Task<IEnumerable<TEntity>> ExecuteQueryInternalAsync<TEntity>(this CloudTable table, TableQuery<TEntity> query, CancellationToken cancellationToken)
			where TEntity : ITableEntity, new()
		{
			IEnumerable<TEntity> result = null;

			TableContinuationToken continuationToken = null;

			do
			{
				// Execute segmented query
				var segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken, cancellationToken);

				// Assign continuation token
				continuationToken = segment.ContinuationToken;

				// Concat segment to result
				result = result?.Concat(segment) ?? segment;
			}
			while (continuationToken != null && !cancellationToken.IsCancellationRequested);

			// Return result
			return result;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Initiates an asynchronous operation to execute the <paramref name="operation"/> on the <paramref name="table"/>.
		/// </summary>
		/// <param name="table">The instance of <see cref="CloudTable"/>.</param>
		/// <param name="operation">The type of operation to execute.</param>
		/// <param name="entity">The instance of <see cref="ITableEntity"/> to use in operation.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task"/> object that represents the current operation.</returns>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="TableResult"/>> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> encapsulates the HTTP response and any entities returned for a particular <paramref name="operation"/> type.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<TableResult> ExecuteAsync(this CloudTable table, TableOperationType operation, ITableEntity entity, CancellationToken cancellationToken)
		{
			// Check table argument
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}

			// Get create operation delegate
			var createOperation = operation.GetCreateOperationDelegate();

			// Create insert operation
			var tableOperation = createOperation(entity);

			// Execute operation
			return table.ExecuteAsync(tableOperation, cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to execute a batch of <paramref name="batchOperationType"/> on the <paramref name="table"/>.
		/// </summary>
		/// <remarks>All entities must have same partition key.</remarks>
		/// <param name="table">The instance of <see cref="CloudTable"/>.</param>
		/// <param name="batchOperationType">The type of the batch operation to execute.</param>
		/// <param name="entities">An enumeration of <see cref="ITableEntity"/> to use in operation.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IReadOnlyList{TableResult}"/>> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> contains an enumeration of objects that encapsulates the HTTP response and any entities returned for a particular <paramref name="batchOperationType"/> type.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
		public static async Task<IEnumerable<TableResult>> ExecuteBatchAsync(this CloudTable table, TableOperationType batchOperationType, IEnumerable<ITableEntity> entities, CancellationToken cancellationToken)
		{
			// Check table argument
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}

			// Get create operation delegate
			var createOperation = batchOperationType.GetCreateOperationDelegate();

			var operationsTasks = new List<Task<IList<TableResult>>>();

			Int32 batchItemsCount = 0, batchSize = 0;

			// Create table batch operation
			var batchOperation = new TableBatchOperation();

			// Get enumerator
			var entityEnumerator = entities.GetEnumerator();

			while (entityEnumerator.MoveNext())
			{
				// Get table entity
				var tableEntity = entityEnumerator.Current;

				// Get size of the table entity
				var tableEntitySize = tableEntity.GetSize();

				// Check if count of operations has exited, or adding current table entity will cause overriding of the batch size limit
				if ((batchItemsCount == tableBatchMaxOperationsCount) || (batchSize + tableEntitySize) >= tableBatchMaxSize)
				{
					// Initiate batch operation
					var batchTask = table.ExecuteBatchAsync(batchOperation, cancellationToken);

					// Add task to the list
					operationsTasks.Add(batchTask);

					// Create new batch operation
					batchOperation = new TableBatchOperation();

					// Reset count
					batchItemsCount = 0;

					// Reset size
					batchSize = 0;
				}

				// Create table operation
				var tableOperation = createOperation(tableEntity);

				// Add operation to the batch
				batchOperation.Add(tableOperation);

				// Increment count
				batchItemsCount++;

				// Increment size
				batchSize += tableEntitySize;
			}

			if (batchItemsCount != 0)
			{
				// Initiate batch operation
				var batchTask = table.ExecuteBatchAsync(batchOperation, cancellationToken);

				// Add task to the list
				operationsTasks.Add(batchTask);
			}

			// Wait tasks to complete
			await Task.WhenAll(operationsTasks);

			// Return result
			return operationsTasks.SelectMany(task => task.Result);
		}

		/// <summary>
		/// Initiates an asynchronous operation to group entities by partition key and execute table <paramref name="batchOperationType"/> on the <paramref name="table"/>.
		/// </summary>
		/// <param name="table">The instance of <see cref="CloudTable"/>.</param>
		/// <param name="batchOperationType">The type of the batch operation to execute.</param>
		/// <param name="entities">An enumeration of <see cref="ITableEntity"/> to use in operation.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IReadOnlyList{TableResult}"/>> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> contains an enumeration of objects that encapsulates the HTTP response and any entities returned for a particular <paramref name="batchOperationType"/> type.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IEnumerable<TableResult>> ExecuteGroupBatchAsync(this CloudTable table, TableOperationType batchOperationType, IEnumerable<ITableEntity> entities, CancellationToken cancellationToken)
		{
			// Group by partition key and execute batch
			var tasks =
				(
					from
						tableEntity in entities
					group
						tableEntity by tableEntity.PartitionKey into entitiesGroup
					select
						table.ExecuteBatchAsync(batchOperationType, entitiesGroup, cancellationToken)
					).ToArray();

			// Await tasks to complete
			await Task.WhenAll(tasks);

			// Return result
			return tasks.SelectMany(task => task.Result);
		}

		/// <summary>
		/// Initiates an asynchronous operation to execute the parallel <paramref name="queries"/> on the <paramref name="table"/>.
		/// </summary>
		/// <param name="table">The instance of <see cref="CloudTable"/>.</param>
		/// <param name="queries">The collection of queries to execute in parallel.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IEnumerable{TEntity}"/>> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> contain an enumerable collection of results.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IEnumerable<TEntity>> ExecuteQueriesAsync<TEntity>(this CloudTable table, IEnumerable<TableQuery<TEntity>> queries, CancellationToken cancellationToken)
			where TEntity : ITableEntity, new()
		{
			// Check table argument
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}

			// Initiate query operations
			var queriesTasks = queries.Select(query => table.ExecuteQueryInternalAsync(query, cancellationToken)).ToArray();

			// Await tasks to complete
			await Task.WhenAll(queriesTasks);

			// Return result
			return queriesTasks.SelectMany(queryResult => queryResult.Result);
		}

		/// <summary>
		/// Initiates an asynchronous operation to execute the <paramref name="query"/> on the <paramref name="table"/>.
		/// </summary>
		/// <param name="table">The instance of <see cref="CloudTable"/>.</param>
		/// <param name="query">The <see cref="TableQuery"/> representing the query to execute.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> object of type <see cref="IEnumerable{TEntity}"/>> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result"/> contain an enumerable collection of results.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="query"/> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<IEnumerable<TEntity>> ExecuteQueryAsync<TEntity>(this CloudTable table, TableQuery<TEntity> query, CancellationToken cancellationToken)
			where TEntity : ITableEntity, new()
		{
			// Check argument
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}

			// Check argument
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			// Execute query
			return table.ExecuteQueryInternalAsync(query, cancellationToken);
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Queue
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="CloudQueue" /> class.
	/// </summary>
	public static class CloudQueueEx
	{
		#region Constant and Static Fields

		private const Int32 maxMessagesCount = 32;

		#endregion

		#region Methods

		/// <summary>
		/// Initiates an asynchronous operation to get all messages from the queue.
		/// </summary>
		/// <param name="cloudQueue">The instance of <see cref="CloudQueue" />.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IEnumerable{CloudQueueMessage}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> contain an enumerable collection of <see cref="CloudQueueMessage" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="cloudQueue" /> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IList<CloudQueueMessage>> GetAllMessagesAsync(this CloudQueue cloudQueue, CancellationToken cancellationToken)
		{
			// check argument
			if (cloudQueue == null)
			{
				throw new ArgumentNullException(nameof(cloudQueue));
			}

			var result = new List<CloudQueueMessage>();

			while (true)
			{
				// Try get maximum messages at once
				var segment = await cloudQueue.GetMessagesAsync(maxMessagesCount, cancellationToken);

				var currentMessagesCount = result.Count;

				// Add segment to result
				result.AddRange(segment);

				// Check count of read messages
				if (result.Count == currentMessagesCount)
				{
					return result;
				}
			}
		}

		/// <summary>
		/// Initiates an asynchronous operation to get all messages from the queue.
		/// </summary>
		/// <param name="cloudQueue">The instance of <see cref="CloudQueue" />.</param>
		/// <param name="visibilityTimeout">A <see cref="TimeSpan" /> specifying the visibility timeout interval.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IEnumerable{CloudQueueMessage}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> contain an enumerable collection of <see cref="CloudQueueMessage" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="cloudQueue" /> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IList<CloudQueueMessage>> GetAllMessagesAsync(this CloudQueue cloudQueue, TimeSpan? visibilityTimeout, CancellationToken cancellationToken)
		{
			// check argument
			if (cloudQueue == null)
			{
				throw new ArgumentNullException(nameof(cloudQueue));
			}

			var result = new List<CloudQueueMessage>();

			while (true)
			{
				// Try get maximum messages at once
				var segment = await cloudQueue.GetMessagesAsync(maxMessagesCount, visibilityTimeout, null, null, cancellationToken);

				var currentMessagesCount = result.Count;

				// Add segment to result
				result.AddRange(segment);

				// Check count of read messages
				if (result.Count == currentMessagesCount)
				{
					return result;
				}
			}
		}

		#endregion
	}
}
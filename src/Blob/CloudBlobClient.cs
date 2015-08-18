using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Blob
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="CloudBlobClient" /> class.
	/// </summary>
	public static class CloudBlobClientEx
	{
		#region Methods

		/// <summary>
		/// Initiates an asynchronous operation that returns a result segment containing a collection of blob items in the container.
		/// </summary>
		/// <param name="client">A container in the Windows Azure Blob service.</param>
		/// <param name="prefix">A string containing the blob name prefix.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IEnumerable{T}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to enumeration of instances of <see cref="CloudBlobContainer" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="client" /> is <c>null</c>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IEnumerable<CloudBlobContainer>> ListContainersAsync(this CloudBlobClient client, String prefix, CancellationToken cancellationToken)
		{
			// check argument
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			var result = (IEnumerable<CloudBlobContainer>) null;

			BlobContinuationToken continuationToken = null;

			do
			{
				// list containers
				var resultSegmented = await client.ListContainersSegmentedAsync(prefix, continuationToken, cancellationToken);

				// add to result
				result = result?.Concat(resultSegmented.Results) ?? resultSegmented.Results;

				// set continuation token
				continuationToken = resultSegmented.ContinuationToken;
			}
			while (continuationToken != null);

			return result;
		}

		#endregion
	}
}
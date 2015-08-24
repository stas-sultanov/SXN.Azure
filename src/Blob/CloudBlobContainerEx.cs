using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Blob
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="CloudBlobContainer" /> class.
	/// </summary>
	public static class CloudBlobContainerEx
	{
		#region Private methods

		/// <summary>
		/// Initiates an asynchronous operation to get operations handles.
		/// </summary>
		/// <param name="sourceContainer">A source blob container.</param>
		/// <param name="sourceBlobNamePrefix">A prefix of the source blob name.</param>
		/// <param name="sourceBlobNamePattern">A <see cref="Regex" /> pattern of the source blob name.</param>
		/// <param name="sourceBlobLeaseTime">A period of time on which a read access to the source containers should be granted.</param>
		/// <param name="targetContainer">A target container in which blobs are to be copied.</param>
		/// <param name="rename">A Function to apply on blob name.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IReadOnlyList{OperationHandle}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to collection of <see cref="OperationHandle" />.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static async Task<IReadOnlyList<OperationHandle>> GetOperationsHandlesAsync(this CloudBlobContainer sourceContainer, String sourceBlobNamePrefix, String sourceBlobNamePattern, TimeSpan sourceBlobLeaseTime, CloudBlobContainer targetContainer, Func<String, String> rename, CancellationToken cancellationToken)
		{
			// List source blobs
			var sourceBlobs = await sourceContainer.ListCloudBlobsAsync(sourceBlobNamePrefix, sourceBlobNamePattern, cancellationToken);

			// Create access policy
			var policy = new SharedAccessBlobPolicy
			{
				Permissions = SharedAccessBlobPermissions.Read,
				SharedAccessExpiryTime = DateTime.UtcNow + sourceBlobLeaseTime,
				SharedAccessStartTime = DateTime.UtcNow - sourceBlobLeaseTime
			};

			var sourceSasToken = sourceContainer.GetSharedAccessSignature(policy);

			// Get handles
			return
				(
					from
						sourceBlob in sourceBlobs
					let
						targetBlobName = rename(sourceBlob.Name)
					let
						targetBlob = targetContainer.GetBlockBlobReference(targetBlobName)
					select
						new OperationHandle(sourceBlob, targetBlob, sourceSasToken)
					).ToList().AsReadOnly();
		}

		#endregion

		#region Methods

		#region Copy

		/// <summary>
		/// Initiates an asynchronous operation to copy block blobs from <paramref name="sourceContainer" /> to <paramref name="targetContainer" />.
		/// </summary>
		/// <param name="sourceContainer">A source blob container.</param>
		/// <param name="sourceBlobNamePrefix">A prefix of the source blob name.</param>
		/// <param name="sourceBlobNamePattern">A <see cref="Regex" /> pattern of the source blob name.</param>
		/// <param name="sourceBlobLeaseTime">A period of time on which a read access to the source containers should be granted.</param>
		/// <param name="targetContainer">A target container in which blobs are to be copied.</param>
		/// <param name="rename">A Function to apply on blob name.</param>
		/// <param name="retryCount">The number of times a copy operation must be retried, if error occurs.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="CloudBlobCopyBlobsResult" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to object that encapsulates copy results information.
		/// </returns>
		public static async Task<CloudBlobCopyBlobsResult> CopyBlobsAsync(this CloudBlobContainer sourceContainer, String sourceBlobNamePrefix, String sourceBlobNamePattern, TimeSpan sourceBlobLeaseTime, CloudBlobContainer targetContainer, Func<String, String> rename, UInt32 retryCount, CancellationToken cancellationToken)
		{
			var handles = await GetOperationsHandlesAsync(sourceContainer, sourceBlobNamePrefix, sourceBlobNamePattern, sourceBlobLeaseTime, targetContainer, rename, cancellationToken);

			// Wait copy to complete
			await OperationHandle.WaitCopyToCompleteAsync(handles, retryCount, cancellationToken);

			// Get and return result
			return OperationHandle.GetCopyResult(handles);
		}

		#endregion

		#region Move

		/// <summary>
		/// Initiates an asynchronous operation to move block blobs from <paramref name="sourceContainer" /> to <paramref name="targetContainer" />.
		/// </summary>
		/// <param name="sourceContainer">A source blob container.</param>
		/// <param name="sourceBlobNamePrefix">A prefix of the source blob name.</param>
		/// <param name="sourceBlobNamePattern">A <see cref="Regex" /> pattern of the source blob name.</param>
		/// <param name="sourceBlobLeaseTime">A period of time on which a read access to the source containers should be granted.</param>
		/// <param name="targetContainer">A target container in which blobs are to be copied.</param>
		/// <param name="rename">A Function to apply on blob name.</param>
		/// <param name="retryCount">The number of times a copy operation must be retried, if error occurs.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="CloudBlobCopyBlobsResult" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to object that encapsulates move results information.
		/// </returns>
		public static async Task<CloudBlobMoveBlobsResult> MoveBlobsAsync(this CloudBlobContainer sourceContainer, String sourceBlobNamePrefix, String sourceBlobNamePattern, TimeSpan sourceBlobLeaseTime, CloudBlobContainer targetContainer, Func<String, String> rename, UInt32 retryCount, CancellationToken cancellationToken)
		{
			// List source blobs
			var sourceBlobs = await sourceContainer.ListCloudBlobsAsync(sourceBlobNamePrefix, sourceBlobNamePattern, cancellationToken);

			// Create access policy
			var policy = new SharedAccessBlobPolicy
			{
				Permissions = SharedAccessBlobPermissions.Read,
				SharedAccessExpiryTime = DateTime.UtcNow + sourceBlobLeaseTime,
				SharedAccessStartTime = DateTime.UtcNow - sourceBlobLeaseTime
			};

			var sourceSasToken = sourceContainer.GetSharedAccessSignature(policy);

			// Get handles
			var handles =
				(
					from
						sourceBlob in sourceBlobs
					let
						targetBlobName = rename(sourceBlob.Name)
					let
						targetBlob = targetContainer.GetBlockBlobReference(targetBlobName)
					select
						new OperationHandle(sourceBlob, targetBlob, sourceSasToken)
					).ToArray();

			// Wait copy to complete
			await OperationHandle.WaitCopyToCompleteAsync(handles, retryCount, cancellationToken);

			// Delete copied blobs
			await OperationHandle.DeleteCopiedSourceBlobsAsync(handles, cancellationToken);

			// Get and return result
			return OperationHandle.GetMoveResult(handles);
		}

		#endregion

		#region Rename

		/// <summary>
		/// Initiates an asynchronous operation to rename block blobs within the <paramref name="container" />.
		/// </summary>
		/// <param name="container">A blob container.</param>
		/// <param name="blobNamePrefix">A prefix of the blob name.</param>
		/// <param name="blobNamePattern">A <see cref="Regex" /> pattern of the blob name.</param>
		/// <param name="blobLeaseTime">A period of time on which a read access to the container should be granted.</param>
		/// <param name="rename">A Function to apply on blob name.</param>
		/// <param name="retryCount">The number of times a copy operation must be retried, if error occurs.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="CloudBlobCopyBlobsResult" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to object that encapsulates rename results information.
		/// </returns>
		public static Task<CloudBlobMoveBlobsResult> RenameBlobsAsync(this CloudBlobContainer container, String blobNamePrefix, String blobNamePattern, TimeSpan blobLeaseTime, Func<String, String> rename, UInt32 retryCount, CancellationToken cancellationToken)
		{
			return MoveBlobsAsync(container, blobNamePrefix, blobNamePattern, blobLeaseTime, container, rename, retryCount, cancellationToken);
		}

		#endregion

		#endregion

		#region List Blobs

		/// <summary>
		/// Initiates an asynchronous operation to enumerate <see cref="ICloudBlob" /> within the <paramref name="container" /> specified which names starts with <paramref name="blobNamePrefix" />.
		/// </summary>
		/// <param name="container">A blob container.</param>
		/// <param name="blobNamePrefix">A prefix of the blob name.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IEnumerable{ICloudBlob}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to enumeration of instances of <see cref="ICloudBlob" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="container" /> is <c>null</c>.</exception>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IEnumerable<ICloudBlob>> ListCloudBlobsAsync(this CloudBlobContainer container, String blobNamePrefix, CancellationToken cancellationToken)
		{
			// Check argument
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			var result = (IEnumerable<IListBlobItem>) null;

			BlobContinuationToken continuationToken = null;

			do
			{
				// List blobs
				var resultSegmented = await container.ListBlobsSegmentedAsync(blobNamePrefix, true, BlobListingDetails.None, null, continuationToken, null, null, cancellationToken);

				// Add to result
				result = result?.Concat(resultSegmented.Results) ?? resultSegmented.Results;

				// Set continuation token
				continuationToken = resultSegmented.ContinuationToken;
			}
			while (continuationToken != null);

			return result.OfType<ICloudBlob>();
		}

		/// <summary>
		/// Initiates an asynchronous operation to enumerate all cloud blobs within the <paramref name="container" /> specified.
		/// </summary>
		/// <param name="container">A blob container.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IEnumerable{ICloudBlob}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to enumeration of instances of <see cref="ICloudBlob" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="container" /> is <c>null</c>.</exception>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<IEnumerable<ICloudBlob>> ListCloudBlobsAsync(this CloudBlobContainer container, CancellationToken cancellationToken)
		{
			return container.ListCloudBlobsAsync(null, cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to enumerate all <see cref="ICloudBlob" /> which names matches to <paramref name="blobNamePattern" />.
		/// </summary>
		/// <param name="container">A blob container.</param>
		/// <param name="blobNamePrefix">A prefix of the blob name.</param>
		/// <param name="blobNamePattern">A <see cref="Regex" /> pattern of the blob name.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>
		/// A <see cref="Task{TResult}" /> object of type <see cref="IEnumerable{ICloudBlob}" /> that represents the asynchronous operation.
		/// <see cref="Task{TResult}.Result" /> refers to enumeration of instances of <see cref="ICloudBlob" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="container" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="blobNamePattern" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="blobNamePattern" /> is empty.</exception>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task<IEnumerable<ICloudBlob>> ListCloudBlobsAsync(this CloudBlobContainer container, String blobNamePrefix, String blobNamePattern, CancellationToken cancellationToken)
		{
			// Check argument
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			// Check argument
			if (blobNamePattern == null)
			{
				throw new ArgumentNullException(nameof(blobNamePattern));
			}

			// Compile regex
			var regex = new Regex(blobNamePattern, RegexOptions.Compiled | RegexOptions.Singleline);

			// List cloud blobs
			var result = await ListCloudBlobsAsync(container, blobNamePrefix, cancellationToken);

			return result.Where(blob => regex.IsMatch(blob.Name));
		}

		#endregion
	}
}
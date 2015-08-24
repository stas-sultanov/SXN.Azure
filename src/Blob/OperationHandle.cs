using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Blob
{
	/// <summary>
	/// Represents an information about the move blob operation.
	/// </summary>
	internal sealed class OperationHandle
	{
		#region Fields

		private readonly String sasToken;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="OperationHandle" /> class.
		/// </summary>
		/// <param name="source">A source blob.</param>
		/// <param name="target">A destination blob.</param>
		/// <param name="sasToken">A security access token.</param>
		public OperationHandle(ICloudBlob source, ICloudBlob target, String sasToken)
		{
			DeleteIsCompleted = false;

			CopyIsCompleted = false;

			SourceBlob = source;

			TargetBlob = target;

			this.sasToken = sasToken;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or set a <see cref="bool" /> value which indicates whether copy operation is completed.
		/// </summary>
		public Boolean CopyIsCompleted
		{
			get;

			private set;
		}

		/// <summary>
		/// Gets or sets count of times copy has been retried.
		/// </summary>
		public UInt32 CopyTryCount
		{
			get;

			private set;
		}

		/// <summary>
		/// Gets or set a <see cref="bool" /> value which indicates whether delete operation is completed.
		/// </summary>
		public Boolean DeleteIsCompleted
		{
			get;

			private set;
		}

		/// <summary>
		/// Gets or set a <see cref="bool" /> value which indicates
		/// </summary>
		public Boolean IsCompleted
		{
			get;

			private set;
		}

		/// <summary>
		/// Gets the source blob reference.
		/// </summary>
		public ICloudBlob SourceBlob
		{
			get;
		}

		/// <summary>
		/// Gets the target blob reference.
		/// </summary>
		public ICloudBlob TargetBlob
		{
			get;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Initiates an asynchronous operation to delete blobs which were successfully copied.
		/// </summary>
		/// <param name="handles">An enumeration of operations handles.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task" /> object that represents the current operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static async Task DeleteCopiedSourceBlobsAsync(IEnumerable<OperationHandle> handles, CancellationToken cancellationToken)
		{
			var deleteTasks = handles.Where(x => x.CopyIsCompleted).ToDictionary(handle => handle.SourceBlob.DeleteAsync(cancellationToken));

			try
			{
				// Wait delete to complete
				await Task.WhenAll(deleteTasks.Keys);
			}
			catch (StorageException)
			{
			}

			// Set delete completion flag
			foreach (var pair in deleteTasks)
			{
				pair.Value.DeleteIsCompleted = pair.Key.IsCompleted;
			}
		}

		/// <summary>
		/// Gets result of copy operation.
		/// </summary>
		/// <param name="handles">An enumeration of operations handles.</param>
		/// <returns>Object that describes result of copying cloud blobs.</returns>
		internal static CloudBlobCopyBlobsResult GetCopyResult(IEnumerable<OperationHandle> handles)
		{
			var newBlobs = new List<ICloudBlob>();

			var notCopiedBlobs = new List<ICloudBlob>();

			foreach (var handle in handles)
			{
				if (handle.CopyIsCompleted)
				{
					newBlobs.Add(handle.TargetBlob);
				}
				else
				{
					notCopiedBlobs.Add(handle.SourceBlob);
				}
			}

			return new CloudBlobCopyBlobsResult(newBlobs, notCopiedBlobs);
		}

		/// <summary>
		/// Gets result of move operation.
		/// </summary>
		/// <param name="handles">An enumeration of operations handles.</param>
		/// <returns>Object that describes result of moving cloud blobs.</returns>
		internal static CloudBlobMoveBlobsResult GetMoveResult(IEnumerable<OperationHandle> handles)
		{
			var newBlobs = new List<ICloudBlob>();

			var notCopiedSourceBlobs = new List<ICloudBlob>();

			var notDeletedSourceBlobs = new List<ICloudBlob>();

			foreach (var handle in handles)
			{
				if (handle.CopyIsCompleted)
				{
					if (handle.DeleteIsCompleted)
					{
						newBlobs.Add(handle.TargetBlob);
					}
					else
					{
						notDeletedSourceBlobs.Add(handle.SourceBlob);
					}
				}
				else
				{
					notCopiedSourceBlobs.Add(handle.SourceBlob);
				}
			}

			return new CloudBlobMoveBlobsResult(newBlobs, notCopiedSourceBlobs, notDeletedSourceBlobs);
		}

		/// <summary>
		/// Initiates an asynchronous operation to start the copy operation.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task" /> object that represents the current operation.</returns>
		public async Task StartCopyAsync(CancellationToken cancellationToken)
		{
			if (CopyIsCompleted)
			{
				throw new InvalidOperationException();
			}

			// Increment copy try counter
			CopyTryCount++;

			// Check if target exists
			if (await TargetBlob.ExistsAsync(cancellationToken))
			{
				// Resolve ?
				return;
			}

			// Start copy
			await TargetBlob.StartCopyFromBlobAsync(new Uri(SourceBlob.Uri + sasToken), cancellationToken);
		}

		/// <summary>
		/// Initiates an asynchronous operation to wait for copy operations to complete.
		/// </summary>
		/// <param name="handles">An enumeration of operations handles.</param>
		/// <param name="retryLimit">A count of times to retry copy operation if failed.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for a task to complete.</param>
		/// <returns>A <see cref="Task" /> object that represents the current operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static async Task WaitCopyToCompleteAsync(IReadOnlyList<OperationHandle> handles, UInt32 retryLimit, CancellationToken cancellationToken)
		{
			// Repeat until all files have been copied or operation fails for the specified number of times.
			while (true)
			{
				// Select all not completed operations
				var notCompletedHandles = handles.Where(x => !x.IsCompleted).ToArray();

				// Check if there are any
				if (notCompletedHandles.Length == 0)
				{
					return;
				}

				foreach (var handle in notCompletedHandles)
				{
					if (handle.CopyTryCount == 0)
					{
						// has not been started yet
						try
						{
							await handle.StartCopyAsync(cancellationToken);
						}
						catch (StorageException)
						{
						}

						continue;
					}

					// Get actual properties
					await handle.TargetBlob.FetchAttributesAsync(cancellationToken);

					// Check status
					switch (handle.TargetBlob.CopyState.Status)
					{
						case CopyStatus.Pending:
						{
							break;
						}

						case CopyStatus.Success:
						{
							handle.CopyIsCompleted = true;

							handle.IsCompleted = true;

							break;
						}

						default:
						{
							// Check if retry limit is reached
							if (handle.CopyTryCount > retryLimit)
							{
								handle.IsCompleted = true;

								break;
							}

							// Restart copy operation
							await handle.StartCopyAsync(cancellationToken);

							break;
						}
					}
				}
			}
		}

		#endregion
	}
}
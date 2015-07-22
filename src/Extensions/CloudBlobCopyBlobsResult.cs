using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides an information about performed bulk copy operation.
	/// </summary>
	public class CloudBlobCopyBlobsResult
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="CloudBlobCopyBlobsResult"/> class.
		/// </summary>
		/// <param name="newBlobs">A list of copied new blobs.</param>
		/// <param name="notCopiedBlobs">A list of not copied source blobs.</param>
		internal CloudBlobCopyBlobsResult(IList<ICloudBlob> newBlobs, IList<ICloudBlob> notCopiedBlobs)
		{
			NewBlobs = new ReadOnlyCollection<ICloudBlob>(newBlobs);

			NotCopiedBlobs = new ReadOnlyCollection<ICloudBlob>(notCopiedBlobs);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the list of new blobs.
		/// </summary>
		public IReadOnlyList<ICloudBlob> NewBlobs
		{
			get;
		}

		/// <summary>
		/// Gets the list of not copied blobs.
		/// </summary>
		public IReadOnlyList<ICloudBlob> NotCopiedBlobs
		{
			get;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Applies an accumulator over a sequence.
		/// </summary>
		/// <param name="results">An enumeration of <see cref="CloudBlobCopyBlobsResult"/> to aggregate over.</param>
		/// <returns>The accumulated result.</returns>
		public static CloudBlobCopyBlobsResult Aggregate(IEnumerable<CloudBlobCopyBlobsResult> results)
		{
			if (results == null)
			{
				throw new ArgumentNullException(nameof(results));
			}

			var newBlobs = new List<ICloudBlob>();

			var notCopiedBlobs = new List<ICloudBlob>();

			foreach (var result in results)
			{
				newBlobs.AddRange(result.NewBlobs);

				notCopiedBlobs.AddRange(result.NotCopiedBlobs);
			}

			return new CloudBlobCopyBlobsResult(newBlobs, notCopiedBlobs);
		}

		#endregion
	}
}
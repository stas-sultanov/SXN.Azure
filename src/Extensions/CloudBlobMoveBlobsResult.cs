using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides an information about performed bulk move operation.
	/// </summary>
	public sealed class CloudBlobMoveBlobsResult : CloudBlobCopyBlobsResult
	{
		#region Fields

		private readonly IReadOnlyList<ICloudBlob> notDeletedBlobs;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="CloudBlobMoveBlobsResult"/> class.
		/// </summary>
		/// <param name="newBlobs">A list of moved new blobs.</param>
		/// <param name="notCopiedBlobs">A list of not copied source blobs.</param>
		/// <param name="notDeletedBlobs">A list of not deleted source blobs.</param>
		internal CloudBlobMoveBlobsResult(IList<ICloudBlob> newBlobs, IList<ICloudBlob> notCopiedBlobs, IList<ICloudBlob> notDeletedBlobs)
			: base(newBlobs, notCopiedBlobs)
		{
			this.notDeletedBlobs = new ReadOnlyCollection<ICloudBlob>(notDeletedBlobs);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the list of not deleted blobs.
		/// </summary>
		public IReadOnlyList<ICloudBlob> NotDeletedBlobs
		{
			get
			{
				return notDeletedBlobs;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Applies an accumulator over a sequence.
		/// </summary>
		/// <param name="results">An enumeration of <see cref="CloudBlobMoveBlobsResult"/> to aggregate over.</param>
		/// <returns>The accumulated result.</returns>
		public static CloudBlobMoveBlobsResult Aggregate(IEnumerable<CloudBlobMoveBlobsResult> results)
		{
			if (results == null)
			{
				throw new ArgumentNullException(@"results");
			}

			var newBlobs = new List<ICloudBlob>();

			var notCopiedBlobs = new List<ICloudBlob>();

			var notDeletedBlobs = new List<ICloudBlob>();

			foreach (var result in results)
			{
				newBlobs.AddRange(result.NewBlobs);

				notCopiedBlobs.AddRange(result.NotCopiedBlobs);

				notDeletedBlobs.AddRange(result.NotDeletedBlobs);
			}

			return new CloudBlobMoveBlobsResult(newBlobs, notCopiedBlobs, notDeletedBlobs);
		}

		#endregion
	}
}
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides a set of tests for <see cref="CloudBlobContainerEx"/> class.
	/// </summary>
	[TestClass]
	public sealed class CloudBlobContainerExtensionsTests
	{
		#region Constant and Static Fields

		private const String cloudBlobContainerName = @"test-inbox";

		private const String storageConnectionString = @"DefaultEndpointsProtocol=https;AccountName=usdata;AccountKey=ubfCkSOxYXXcFaDvbP8Xuk/JDmf9luKuBw+521eP/iyfao0hHEriVRvgZWTq/czpjglxzeyqTI5adhaG/3yUeQ==";

		#endregion

		#region Fields

		private readonly CloudBlobClient cloudBlobClient;

		private readonly CloudBlobContainer cloudBlobContainer;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="CloudBlobContainerExtensionsTests"/> class.
		/// </summary>
		public CloudBlobContainerExtensionsTests()
		{
			//set the azure container
			var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);

			cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

			cloudBlobContainer = cloudBlobClient.GetContainerReference(cloudBlobContainerName);
		}

		#endregion

		#region Test methods

		[TestMethod]
		[TestCategory("Manual")]
		public void CopyBlobsAsyncTest()
		{
			var targetContainer = cloudBlobClient.GetContainerReference(@"test");

			targetContainer.CreateIfNotExists();

			var blobs = cloudBlobContainer.CopyBlobsAsync
				(
					@"2015-01-20-16-00-00",
					@"^.*[\d]{18}\.avro$",
					TimeSpan.FromMinutes(5),
					targetContainer,
					s =>
					{
						var sourceFileName = Path.GetFileNameWithoutExtension(s);

						var sourceFileNameAsDate = new DateTime(Int64.Parse(sourceFileName));

						return Path.Combine(Path.GetDirectoryName(s), sourceFileNameAsDate.ToString(@"yyyy-MM-dd-HH-mm-ss-fffffff.avro"));
					},
					5,
					CancellationToken.None
				);

			var result = blobs.Result;
		}

		[TestMethod]
		[TestCategory("Manual")]
		public void ListCloudBlobsAsyncTest()
		{
			var blobs = cloudBlobContainer.ListCloudBlobsAsync(@"[\d]{18}\.avro$", CancellationToken.None).Result;
		}

		[TestMethod]
		[TestCategory("Manual")]
		public void MoveBlobsAsyncTest()
		{
			var targetContainer = cloudBlobClient.GetContainerReference(@"test");

			targetContainer.CreateIfNotExists();

			var blobs = cloudBlobContainer.CopyBlobsAsync
				(
					@"2015-01-21-12-00-00",
					@"^.*[\d]{18}\.avro$",
					TimeSpan.FromMinutes(5),
					targetContainer,
					s =>
					{
						var sourceFileName = Path.GetFileNameWithoutExtension(s);

						var sourceFileNameAsDate = new DateTime(Int64.Parse(sourceFileName));

						return Path.Combine(Path.GetDirectoryName(s), sourceFileNameAsDate.ToString(@"yyyy-MM-dd-HH-mm-ss-fffffff.avro"));
					},
					5,
					CancellationToken.None
				);

			var result = blobs.Result;

			foreach (var newBlob in result.NewBlobs)
			{
				Trace.TraceInformation("New Blob {0}", newBlob.Name);
			}

			foreach (var notCopiedSourceBlob in result.NotCopiedBlobs)
			{
				Trace.TraceInformation("Not Copied Blob {0}", notCopiedSourceBlob.Name);
			}
		}

		[TestMethod]
		[TestCategory("Manual")]
		public void RenameBlobsAsyncTest()
		{
			var blobs = cloudBlobContainer.RenameBlobsAsync
				(
					null,
					@"^.*[\d]{18}\.avro$",
					TimeSpan.FromMinutes(5),
					s =>
					{
						var sourceFileName = Path.GetFileNameWithoutExtension(s);

						var sourceFileNameAsDate = new DateTime(Int64.Parse(sourceFileName));

						return Path.Combine(Path.GetDirectoryName(s), sourceFileNameAsDate.ToString(@"yyyy-MM-dd-HH-mm-ss-fffffff.avro"));
					},
					5,
					CancellationToken.None
				);

			var result = blobs.Result;

			foreach (var newBlob in result.NewBlobs)
			{
				Trace.TraceInformation("New Blob {0}", newBlob.Name);
			}

			foreach (var notCopiedSourceBlob in result.NotCopiedBlobs)
			{
				Trace.TraceInformation("Not Copied Blob {0}", notCopiedSourceBlob.Name);
			}

			foreach (var notCopiedSourceBlob in result.NotCopiedBlobs)
			{
				Trace.TraceInformation("Not Deleted Blob {0}", notCopiedSourceBlob.Name);
			}
		}

		#endregion
	}
}
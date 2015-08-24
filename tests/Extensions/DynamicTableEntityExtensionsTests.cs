using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides a set of tests for <see cref="DynamicTableEntityEx" /> class.
	/// </summary>
	[TestClass]
	public sealed class DynamicTableEntityExtensionsTests
	{
		#region Test methods

		[TestMethod]
		[TestCategory("UnitTests")]
		public void AddPropertyUnitTest()
		{
			var entity = new DynamicTableEntity("TestPartition", "TestRow");

			entity.AddProperty("Guid", Guid.NewGuid());

			entity.AddProperty("Guid?", (Guid?) Guid.NewGuid());

			entity.AddProperty("DateTime", DateTime.UtcNow);

			entity.AddProperty("DateTime?", (DateTime?) DateTime.UtcNow);

			entity.AddProperty("DateTimeOffset", DateTimeOffset.UtcNow);

			entity.AddProperty("DateTimeOffset?", (DateTimeOffset?) DateTimeOffset.UtcNow);
		}

		#endregion
	}
}
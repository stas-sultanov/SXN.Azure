using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides a set of tests for <see cref="ITableEntityExtensions"/> class.
	/// </summary>
	[TestClass]
	public sealed class ITableEntityExtensionsTests
	{
		#region Test methods

		[TestMethod]
		[TestCategory("Manual")]
		public void GetSizeNullTest()
		{
			var tableEntity = new DynamicTableEntity("TestPartitionKey", "TestRowKey");

			tableEntity.AddProperty("suppa", "duppa");

			tableEntity.AddProperty("suppa2", 10);

			tableEntity.Properties.Add("suppa3", new EntityProperty((Int32?) null));

			tableEntity.Properties.Add("suppa4", new EntityProperty((Byte[]) null));

			var size = tableEntity.GetSize();
		}

		[TestMethod]
		[TestCategory("Manual")]
		public void GetSizeTest()
		{
			var tableEntity = new DynamicTableEntity("TestPartitionKey", "TestRowKey");

			tableEntity.AddProperty("suppa", "duppa");

			tableEntity.AddProperty("suppa2", 10);

			var size = tableEntity.GetSize();
		}

		#endregion
	}
}
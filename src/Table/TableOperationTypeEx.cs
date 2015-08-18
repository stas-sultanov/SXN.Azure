using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Table
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="TableOperationType"/> enumeration.
	/// </summary>
	internal static class TableOperationTypeEx
	{
		#region Methods

		/// <summary>
		/// Gets a delegate to function that creates an instance of <see cref="TableOperation"/>.
		/// </summary>
		/// <param name="operationType">A type of table operation.</param>
		/// <returns>A delegate to create function.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Func<ITableEntity, TableOperation> GetCreateOperationDelegate(this TableOperationType operationType)
		{
			switch (operationType)
			{
				case TableOperationType.Insert:
				{
					return TableOperation.Insert;
				}
				case TableOperationType.Delete:
				{
					return TableOperation.Delete;
				}
				case TableOperationType.Replace:
				{
					return TableOperation.Replace;
				}
				case TableOperationType.Merge:
				{
					return TableOperation.Merge;
				}
				case TableOperationType.InsertOrReplace:
				{
					return TableOperation.InsertOrReplace;
				}
				case TableOperationType.InsertOrMerge:
				{
					return TableOperation.InsertOrMerge;
				}
				default:
				{
					throw new ArgumentOutOfRangeException(nameof(operationType), operationType, @"Is not supported.");
				}
			}
		}

		#endregion
	}
}
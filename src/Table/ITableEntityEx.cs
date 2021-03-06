﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Table
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="ITableEntity" /> interface.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static class ITableEntityEx
	{
		#region Methods

		/// <summary>
		/// Gets the number of bytes the <paramref name="entityProperty" /> takes in Azure Table.
		/// </summary>
		/// <param name="entityProperty">The entity property.</param>
		/// <returns>The number of bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Int32 GetSize(this EntityProperty entityProperty)
		{
			if (entityProperty == null)
			{
				return 0;
			}

			switch (entityProperty.PropertyType)
			{
				case EdmType.String:
				{
					return 4 + (entityProperty.StringValue?.Length * 2 ?? 0);
				}
				case EdmType.Binary:
				{
					return 4 + (entityProperty.BinaryValue?.Length ?? 0);
				}
				case EdmType.Boolean:
				{
					return 1;
				}
				case EdmType.DateTime:
				{
					return 8;
				}
				case EdmType.Double:
				{
					return 8;
				}
				case EdmType.Guid:
				{
					return 16;
				}
				case EdmType.Int32:
				{
					return 4;
				}
				case EdmType.Int64:
				{
					return 8;
				}
				default:
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Gets the number of bytes the table entity takes in Azure Table.
		/// </summary>
		/// <param name="tableEntity">The table entity.</param>
		/// <returns>The size in bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Int32 GetSize(this ITableEntity tableEntity)
		{
			var partitionKeyLength = tableEntity.PartitionKey?.Length * 2 ?? 0;

			var rowKeyLength = tableEntity.RowKey?.Length * 2 ?? 0;

			return 4 + partitionKeyLength + rowKeyLength + tableEntity.WriteEntity(null).Sum(pair => 8 + pair.Key.Length * 2 + pair.Value.GetSize());
		}

		#endregion
	}
}
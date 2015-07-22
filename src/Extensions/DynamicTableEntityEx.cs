using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides a set of extension methods for <see cref="DynamicTableEntity"/> class.
	/// </summary>
	public static class DynamicTableEntityEx
	{
		#region Constant and Static Fields

		/// <summary>
		/// A dictionary of <see cref="EntityProperty"/> constructors, where key is type of the object.
		/// </summary>
		private static readonly IReadOnlyDictionary<Type, Func<Object, EntityProperty>> entityCtors = new Dictionary<Type, Func<Object, EntityProperty>>
		{
			{
				typeof(Value128), o => new EntityProperty((Guid?) (Value128) o)
			},
			{
				typeof(Value128?), o => new EntityProperty((Guid?) (Value128) o)
			},
			{
				typeof(Guid), o => new EntityProperty((Guid?) o)
			},
			{
				typeof(Guid?), o => new EntityProperty((Guid?) o)
			},
			{
				typeof(DateTime), o => new EntityProperty((DateTime?) o)
			},
			{
				typeof(DateTime?), o => new EntityProperty((DateTime?) o)
			},
			{
				typeof(DateTimeOffset), o => new EntityProperty((DateTimeOffset?) o)
			},
			{
				typeof(DateTimeOffset?), o => new EntityProperty((DateTimeOffset?) o)
			},
			{
				typeof(Boolean), o => new EntityProperty((Boolean) o)
			},
			{
				typeof(Boolean?), o => new EntityProperty((Boolean?) o)
			},
			{
				typeof(Byte[]), o => new EntityProperty((Byte[]) o)
			},
			{
				typeof(Double), o => new EntityProperty((Double) o)
			},
			{
				typeof(Double?), o => new EntityProperty((Double?) o)
			},
			{
				typeof(Int32), o => new EntityProperty((Int32) o)
			},
			{
				typeof(Int32?), o => new EntityProperty((Int32?) o)
			},
			{
				typeof(Int64), o => new EntityProperty((Int64) o)
			},
			{
				typeof(Int64?), o => new EntityProperty((Int64?) o)
			},
			{
				typeof(String), o => new EntityProperty((String) o)
			}
		};

		#endregion

		#region Methods

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="DynamicTableEntity"/>.
		/// </summary>
		/// <param name="tableEntity">A target table entity.</param>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		public static void AddProperty(this DynamicTableEntity tableEntity, String key, Object value)
		{
			if (tableEntity == null)
			{
				throw new ArgumentNullException(nameof(tableEntity));
			}

			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Func<Object, EntityProperty> entityCtor;

			if (!entityCtors.TryGetValue(value.GetType(), out entityCtor))
			{
				throw new ArgumentException("the type of value is not supported", nameof(value));
			}

			tableEntity.Properties.Add(key, entityCtor(value));
		}

		#endregion
	}
}
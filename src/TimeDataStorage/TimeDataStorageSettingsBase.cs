using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a base class for classes that provides configuration settings for classes inherited from <see cref="TimeDataStorageBase{T}"/> class.
	/// </summary>
	[DataContract]
	public abstract class TimeDataStorageSettingsBase
	{
		#region Properties

		/// <summary>
		/// The formats of the row keys.
		/// </summary>
		[DataMember]
		public IDictionary<TimeUnit, String> RowKeyFormats
		{
			get;

			set;
		}

		/// <summary>
		/// The root of the names of the tables.
		/// </summary>
		[DataMember]
		public String TablesNameRoot
		{
			get;

			set;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the name of the table.
		/// </summary>
		public String GetTableName(TimeUnit timeUnit)
		{
			return TablesNameRoot + timeUnit;
		}

		#endregion
	}
}
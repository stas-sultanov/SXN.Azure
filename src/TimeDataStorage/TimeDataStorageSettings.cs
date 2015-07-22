using System;
using System.Runtime.Serialization;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a configuration settings for <see cref="TimeDataStorage{TEntity}"/> class.
	/// </summary>
	[DataContract]
	public sealed class TimeDataStorageSettings : TimeDataStorageSettingsBase
	{
		#region Properties

		/// <summary>
		/// The storage account credentials.
		/// </summary>
		[DataMember]
		public String StorageCredentials
		{
			get;

			set;
		}

		#endregion
	}
}
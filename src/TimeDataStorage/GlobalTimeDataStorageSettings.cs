using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a configuration settings for <see cref="GlobalTimeDataStorage{T}"/> class.
	/// </summary>
	[DataContract]
	public sealed class GlobalTimeDataStorageSettings : TimeDataStorageSettingsBase
	{
		#region Properties

		/// <summary>
		/// The dictionary of the storage accounts credentials with <see cref="AzureRegion"/> as key.
		/// </summary>
		[DataMember]
		public IDictionary<AzureRegion, String> StoragesCredentials
		{
			get;

			set;
		}

		#endregion
	}
}
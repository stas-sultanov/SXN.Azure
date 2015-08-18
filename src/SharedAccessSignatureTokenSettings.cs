using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Storage
{
	/// <summary>
	/// Represents the Azure Service Bus Token.
	/// </summary>
	[DataContract]
	public sealed class SharedAccessSignatureTokenSettings
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="SharedAccessSignatureTokenSettings"/> class.
		/// </summary>
		/// <param name="keyName">The key name.</param>
		/// <param name="sharedAccessKey">The shared access key.</param>
		public SharedAccessSignatureTokenSettings(String keyName, String sharedAccessKey)
		{
			if (keyName == null)
			{
				throw new ArgumentNullException(nameof(keyName));
			}

			if (sharedAccessKey == null)
			{
				throw new ArgumentNullException(nameof(sharedAccessKey));
			}

			KeyName = keyName;

			SharedAccessKey = sharedAccessKey;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The key name.
		/// </summary>
		[DataMember]
		public String KeyName
		{
			get;
		}

		/// <summary>
		/// The shared access key.
		/// </summary>
		[DataMember]
		public String SharedAccessKey
		{
			get;
		}

		#endregion
	}
}
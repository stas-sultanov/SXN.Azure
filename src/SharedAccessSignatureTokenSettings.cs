using System;
using System.Runtime.Serialization;

namespace SXN.Azure
{
	/// <summary>
	/// Represents the Azure Service Bus Token.
	/// </summary>
	[DataContract]
	public sealed class SharedAccessSignatureTokenSettings
	{
		#region Fields

		[DataMember(Name = @"KeyName")]
		private readonly String keyName;

		[DataMember(Name = @"SharedAccessKey")]
		private readonly String sharedAccessKey;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="SharedAccessSignatureTokenSettings"/> class.
		/// </summary>
		/// <param name="keyName">The key name.</param>
		/// <param name="sharedAccessKey">The shared access key.</param>
		public SharedAccessSignatureTokenSettings(String keyName, String sharedAccessKey)
		{
			this.keyName = keyName.CheckArgument(@"keyName");

			this.sharedAccessKey = sharedAccessKey.CheckArgument(@"sharedAccessKey");
		}

		#endregion

		#region Properties

		/// <summary>
		/// The key name.
		/// </summary>
		public String KeyName
		{
			get
			{
				return keyName;
			}
		}

		/// <summary>
		/// The shared access key.
		/// </summary>
		public String SharedAccessKey
		{
			get
			{
				return sharedAccessKey;
			}
		}

		#endregion
	}
}
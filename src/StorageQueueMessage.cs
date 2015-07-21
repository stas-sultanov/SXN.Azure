using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SXN.Azure
{
	/// <summary>
	/// Represents a message sent by Azure Scheduler service.
	/// </summary>
	[XmlRoot("StorageQueueMessage")]
	public sealed class StorageQueueMessage
	{
		#region Properties

		/// <summary>
		/// Gets or sets the client's unique request id.
		/// </summary>
		[DataMember]
		public Guid ClientRequestId
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or sets the execution tag.
		/// </summary>
		[DataMember]
		public Guid ExecutionTag
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or sets message.
		/// </summary>
		[DataMember]
		public String Message
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or sets Azure region.
		/// </summary>
		[DataMember]
		public String Region
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or set the name of the Azure Scheduler Job Collection.
		/// </summary>
		[DataMember]
		public String SchedulerJobCollectionId
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or set the name of the Azure Scheduler Job.
		/// </summary>
		[DataMember]
		public String SchedulerJobId
		{
			get;

			set;
		}

		#endregion
	}
}
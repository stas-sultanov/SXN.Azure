using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage
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
		public Guid ClientRequestId
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or sets the execution tag.
		/// </summary>
		public Guid ExecutionTag
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or sets message.
		/// </summary>
		public String Message
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or sets Azure region.
		/// </summary>
		public String Region
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or set the name of the Azure Scheduler Job Collection.
		/// </summary>
		public String SchedulerJobCollectionId
		{
			get;

			set;
		}

		/// <summary>
		/// Gets or set the name of the Azure Scheduler Job.
		/// </summary>
		public String SchedulerJobId
		{
			get;

			set;
		}

		#endregion
	}
}
using System;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides a set of the counters of the campaign.
	/// </summary>
	internal struct TestCounter
	{
		#region Properties

		/// <summary>
		/// The identifier of the campaign.
		/// </summary>
		public Value128 CampaignId
		{
			get;

			set;
		}

		/// <summary>
		/// The Compute Service Counter data.
		/// </summary>
		public Int64 Count
		{
			get;

			set;
		}

		/// <summary>
		/// The time of the range.
		/// </summary>
		public DateTime Time
		{
			get;

			set;
		}

		#endregion
	}
}
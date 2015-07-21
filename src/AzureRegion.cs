namespace SXN.Azure
{
	/// <summary>
	/// Specifies the Microsoft Azure region.
	/// </summary>
	public enum AzureRegion : ushort
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// Name: 'Central US', Region: 'North America', Location: 'Iowa'.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		CentralUS = 0x0101,

		/// <summary>
		/// Name: 'East US, Region: 'North America', Location: 'Virginia'.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		EastUS = 0x0102,

		/// <summary>
		/// Name: 'East US 2', Region: 'North America', Location: 'Virginia'.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		EastUS2 = 0x0103,

		/// <summary>
		/// Name: 'North Central US', Region: 'North America', Location: 'Illinois'.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		NorthCentralUS = 0x0104,

		/// <summary>
		/// Name: 'South Central US', Region: 'North America', Location: 'Texas'.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		SouthCentralUS = 0x0105,

		/// <summary>
		/// Name: 'West US', Region: 'North America', Location: 'California'.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		WestUS = 0x0106,

		/// <summary>
		/// Name: 'Brazil South', Region: 'South America', Location: 'Sao Paulo State'.
		/// </summary>
		BrazilSouth = 0x0201,

		/// <summary>
		/// Name: 'North Europe', Region: 'Europe', Location: 'Ireland'.
		/// </summary>
		NorthEurope = 0x0301,

		/// <summary>
		/// Name: 'West Europe', Region: 'Europe', Location: 'Netherlands'.
		/// </summary>
		WestEurope = 0x0302,

		/// <summary>
		/// Name: 'East Asia', Region: 'Asia', Location: 'Hong Kong'.
		/// </summary>
		EastAsia = 0x0401,

		/// <summary>
		/// Name: 'Southeast Asia', Region: 'Asia', Location: 'Singapore'.
		/// </summary>
		SoutheastAsia = 0x0402,

		/// <summary>
		/// Name: 'Japan East', Region: 'Japan', Location: 'Saitama Prefecture'.
		/// </summary>
		JapanEast = 0x0501,

		/// <summary>
		/// Name: 'Japan West', Region: 'Japan', Location: 'Osaka Prefecture'.
		/// </summary>
		JapanWest = 0x0502,

		/// <summary>
		/// Name: 'Australia East', Region: 'Australia', Location: 'New South Wales'.
		/// </summary>
		AustraliaEast = 0x0601,

		/// <summary>
		/// Name: 'Australia Southeast', Region: 'Australia', Location: 'Victoria'.
		/// </summary>
		AustraliaSoutheast = 0x0602
	}
}
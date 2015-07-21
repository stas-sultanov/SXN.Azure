using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure.TimeDataStorage
{
	internal sealed class TestCounterProvider : ITimeDataEntityProvider<TestCounter>
	{
		#region Constant and Static Fields

		private static readonly TryResult<TestCounter> defaultTryResolveOrUpdateResult = new TryResult<TestCounter>();

		#endregion

		#region ITimeDataEntityProvider<TestCounter> Members

		public DateTime GetTime(TestCounter entity)
		{
			return entity.Time;
		}

		public String GetKey(TestCounter entity)
		{
			return entity.CampaignId.ToString();
		}

		public IDictionary<String, EntityProperty> GetProperties(TestCounter entity)
		{
			return new Dictionary<String, EntityProperty>
			{
				{
					@"Count", new EntityProperty(entity.Count)
				}
			};
		}

		public TryResult<TestCounter> TryResolve(String key, DateTime time, IDictionary<String, EntityProperty> properties)
		{
			// Try get campaign id
			var campaignId = Value128Converter.TryFromBase64String(key, 0, Base64Encoding.Lex);

			if (!campaignId.Success)
			{
				return defaultTryResolveOrUpdateResult;
			}

			// Try get count
			EntityProperty countProperty;

			if (!properties.TryGetValue("Count", out countProperty))
			{
				return defaultTryResolveOrUpdateResult;
			}

			if (countProperty.Int64Value == null)
			{
				return defaultTryResolveOrUpdateResult;
			}

			var count = countProperty.Int64Value.Value;

			// Compose result
			return TryResult<TestCounter>.CreateSuccess(new
				TestCounter
			{
				CampaignId = campaignId,
				Time = time,
				Count = count
			});
		}

		public TryResult<TestCounter> TryUpdate(TestCounter currentEntity, TestCounter newEntity)
		{
			if (currentEntity.CampaignId != newEntity.CampaignId)
			{
				return defaultTryResolveOrUpdateResult;
			}

			currentEntity.Count += newEntity.Count;

			return TryResult<TestCounter>.CreateSuccess(currentEntity);
		}

		#endregion
	}
}
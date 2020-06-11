using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rgom.StorageAccountLogs.Functions.Models
{
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class StorageAccountConfiguration
	{
		[JsonProperty("logProcessingMode"), JsonConverter(typeof(StringEnumConverter))]
		public LogProcessingMode LogProcessingMode { get; set; }

		[JsonProperty("shouldAlwaysLogAnonymousRequests")]
		public bool ShouldAlwaysLogAnonymousRequests { get; set; }

		[JsonProperty("ignoredOperationTypes", NullValueHandling = NullValueHandling.Ignore)]
		public string[] IgnoredOperationTypes { get; set; }

		[JsonProperty("ignoredContainers", NullValueHandling = NullValueHandling.Ignore)]
		public string[] IgnoredContainers { get; set; }

		[JsonProperty("ignoredIps", NullValueHandling = NullValueHandling.Ignore)]
		public string[] IgnoredIps { get; set; }

		[JsonProperty("ignoredPrincipals", NullValueHandling = NullValueHandling.Ignore)]
		public string[] IgnoredPrincipals { get; set; }

		[JsonProperty("ignoredApplicationIds", NullValueHandling = NullValueHandling.Ignore)]
		public string[] IgnoredApplicationIds { get; set; }

		[JsonProperty("logAnalyticsWorkspaceId", NullValueHandling = NullValueHandling.Ignore)]
		public string LogAnalyticsWorkspaceId { get; set; }

		[JsonProperty("logAnalyticsKey", NullValueHandling = NullValueHandling.Ignore)]
		public string LogAnalyticsKey { get; set; }

		[JsonProperty("logAnalyticsTable", NullValueHandling = NullValueHandling.Ignore)]
		public string LogAnalyticsTable { get; set; }
	}
}

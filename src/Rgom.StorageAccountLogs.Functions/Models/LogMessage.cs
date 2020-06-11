using Newtonsoft.Json;
using System;

namespace Rgom.StorageAccountLogs.Functions.Models
{
	/// <summary>
	/// Represents a LogMessage from Azure Storage Diagnostics
	/// </summary>
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class LogMessage
	{
		public string RequestTime { get; set; }
		public string OriginIp { get; set; }
		public string AuthenticationType { get; set; }
		public string RequestType { get; set; }
		public string RequestStatus { get; set; }
		public int HttpStatusCode { get; set; }
		public string Url { get; set; }
		public string UserAgent { get; set; }
		public string ApplicationId { get; set; }
		public string UserPrincipalName { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is LogMessage)
			{
				var otherLog = obj as LogMessage;
				return this.RequestTime == otherLog.RequestTime &&
					   this.RequestType == otherLog.RequestTime &&
					   this.OriginIp == otherLog.OriginIp &&
					   this.Url == otherLog.Url &&
					   this.UserAgent == otherLog.UserAgent;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (RequestTime + Url + RequestType + OriginIp.ToString()).GetHashCode();
		}
	}
}

using System;

namespace StorageEventFunctions
{
	/// <summary>
	/// Represents a LogMessage from Azure Storage Diagnostics
	/// </summary>
	[Serializable]
	public class LogMessage
	{
		public string RequestTime;
		public string OriginIp;
		public string Url;
		public string RequestType;
		public string UserAgent;
		public string AuthenticationType;
		public string ApplicationId;
		public string UserPrincipalName;

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

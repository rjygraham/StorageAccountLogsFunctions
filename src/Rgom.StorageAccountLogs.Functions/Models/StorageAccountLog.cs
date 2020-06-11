using CsvHelper.Configuration;
using System;

namespace StorageEventFunctions.Models
{
	public class StorageAccountLog
	{
		public decimal VersionNumber { get; set; }
		public DateTime RequestStartTime { get; set; }
		public string OperationType { get; set; }
		public string RequestStatus { get; set; }
		public int HttpStatusCode { get; set; }
		public int EndToEndLatencyInMs { get; set; }
		public int ServerLatencyInMs { get; set; }
		public string AuthenticationType { get; set; }
		public string RequesterAccountName { get; set; }
		public string OwnerAccountName { get; set; }
		public string ServiceType { get; set; }
		public string RequestUrl { get; set; }
		public string RequestedObjectKey { get; set; }
		public string RequestIdHeader { get; set; }
		public int OperationCount { get; set; }
		public string RequesterIpAddress { get; set; }
		public string RequestVersionHeader { get; set; }
		public long RequestHeaderSize { get; set; }
		public long RequestPacketSize { get; set; }
		public long ResponseHeaderSize { get; set; }
		public long ResponsePacketSize { get; set; }
		public long RequestContentLength { get; set; }
		public string RequestMd5 { get; set; }
		public string ServerMd5 { get; set; }
		public string EtagIdentifier { get; set; }
		public string ConditionsUsed { get; set; }
		public string UserAgentHeader { get; set; }
		public string ReferrerHeader { get; set; }
		public string ClientRequestId { get; set; }
		public string UserObjectId { get; set; }
		public string TenantId { get; set; }
		public string ApplicationId { get; set; }
		public string Audience { get; set; }
		public string Issuer { get; set; }
		public string UserPrincipalName { get; set; }
		public string ReservedField { get; set; }
		public string AuthorizationDetail { get; set; }
	}

	public class StorageAccountLogMap : ClassMap<StorageAccountLog>
	{
		public StorageAccountLogMap()
		{
			Map(m => m.VersionNumber).Index(0);
			Map(m => m.RequestStartTime).ConvertUsing(row => DateTime.ParseExact(row.GetField(1), "yyyy-MM-ddTHH:mm:ss.fffffffZ", null));
			Map(m => m.OperationType).Index(2);
			Map(m => m.RequestStatus).Index(3);
			Map(m => m.HttpStatusCode).Index(4);
			Map(m => m.EndToEndLatencyInMs).Index(5);
			Map(m => m.ServerLatencyInMs).Index(6);
			Map(m => m.AuthenticationType).Index(7);
			Map(m => m.RequesterAccountName).Index(8);
			Map(m => m.OwnerAccountName).Index(9);
			Map(m => m.ServiceType).Index(10);
			Map(m => m.RequestUrl).Index(11);
			Map(m => m.RequestedObjectKey).Index(12);
			Map(m => m.RequestIdHeader).Index(13);
			Map(m => m.OperationCount).Index(14);
			Map(m => m.RequesterIpAddress).Index(15);
			Map(m => m.RequestVersionHeader).Index(16);
			Map(m => m.RequestHeaderSize).Index(17);
			Map(m => m.RequestPacketSize).Index(18);
			Map(m => m.ResponseHeaderSize).Index(19);
			Map(m => m.ResponsePacketSize).Index(20);
			Map(m => m.RequestContentLength).Index(21);
			Map(m => m.RequestMd5).Index(22);
			Map(m => m.ServerMd5).Index(23);
			Map(m => m.EtagIdentifier).Index(24);
			Map(m => m.ConditionsUsed).Index(26);
			Map(m => m.UserAgentHeader).Index(27);
			Map(m => m.ReferrerHeader).Index(28);
			Map(m => m.ClientRequestId).Index(29);
			Map(m => m.UserObjectId).Index(30);
			Map(m => m.TenantId).Index(31);
			Map(m => m.ApplicationId).Index(32);
			Map(m => m.Audience).Index(33);
			Map(m => m.Issuer).Index(34);
			Map(m => m.UserPrincipalName).Index(35);
			Map(m => m.ReservedField).Index(36);
			Map(m => m.AuthorizationDetail).Index(37);
		}
	}
}

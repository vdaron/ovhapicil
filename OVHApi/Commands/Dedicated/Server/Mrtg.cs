using System;
using OVHApi.Commands;

namespace OVHApi.Commands.Dedicated.Server
{
	public enum MrtgPeriod
	{
		Hourly,
		Daily,
		Weekly,
		Monthly,
		Yearly
	}

	public enum MrtgType
	{
		ErrorsDownload,
		ErrorsUpload,
		PacketsDownload,
		PacketsUpload,
		TrafficDownload,
		TrafficUpload
	}

	public class Mrtg
	{
		public long Timestamp{get;set;}
		public UnitAndValue<double> Value{get;set;}
	}
}


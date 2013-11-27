using System;

namespace OVHApi.Commands
{
	public enum OvhTaskStatus
	{
		Cancelled,
		Doing,
		Done,
		Error,
		Todo
	}

	public class OvhTask
	{
		public long Id{ get; set;}
		public string Function{get;set;}
		public DateTime LastUpdate{get;set;}
		public string Comment{get;set;}
		public OvhTaskStatus Status{get;set;}
		public DateTime TodoDate{get;set;}
		public DateTime DoneDate{get;set;}
	}
}


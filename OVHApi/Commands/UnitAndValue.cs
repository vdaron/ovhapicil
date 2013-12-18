using System;

namespace OVHApi.Commands
{
	public class UnitAndValue<T>
	{
		public string Unit{get;set;}
		public T Value{get;set;}
	}
}


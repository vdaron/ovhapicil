using System;

namespace OVHApi
{
	public enum LegalForm
	{
		Association,
		Corporation,
		Individual,
		Other
	}

	public class NickHandle
	{
		public string Firstname{get;set;}
		public string Country{get;set;}
		public string Language{get;set;}
		public string Organisation{get;set;}
		public string Area{get;set;}
		public string Name{get;set;}
		public string Phone{get;set;}
		public string Email{get;set;}
		public string City{get;set;}
		
		public string Zip{get;set;}
		public string Fax{get;set;}
		public string Nickhandle{get;set;}
		public string Address{ get; set;}
		public LegalForm LegalForm{get;set;}
	}
}


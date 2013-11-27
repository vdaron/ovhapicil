using System;
using OVHApi.Commands;

namespace OVHApi.Tools
{
	[Serializable]
	public class OvhException : Exception
	{
		public Error Error{ get; set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:OvhException"/> class
		/// </summary>
		public OvhException()
		{

		}

		public OvhException(Error error)
			:base(error.Message)
		{
			Error = error;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:OvhException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public OvhException(string message) : base (message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:OvhException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public OvhException(string message, Exception inner)
			: base (message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:OvhException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected OvhException(System.Runtime.Serialization.SerializationInfo info,
		                       System.Runtime.Serialization.StreamingContext context)
			: base (info, context)
		{
		}
	}
}


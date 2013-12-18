//
// BackupFtp.cs
//
// Author:
//       Vincent DARON <vda@depfac.com>
//
// Copyright (c) 2013 Vincent DARON
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace OVHApi.Commands.Dedicated.Server
{
	public class BackupFtp
	{
		/// <summary>
		/// The disk space available in gigabytes
		/// </summary>
		/// <value>The quota.</value>
		public UnitAndValue<long> Quota{ get; set;}
		/// <summary>
		/// The disk space currently used on your backup FTP in gigabytes
		/// </summary>
		/// <value>The usage.</value>
		public UnitAndValue<long> Usage{get;set;}
		/// <summary>
		/// The backup FTP server name
		/// </summary>
		/// <value>The name of the ftp backup.</value>
		public string FtpBackupName{get;set;}
		/// <summary>
		/// If not-null, gives the date since when your account was set in read-only mode because the quota was exceeded
		/// </summary>
		/// <value>The read only date.</value>
		public DateTime? ReadOnlyDate{get;set;}
	}
}


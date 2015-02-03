//
// Ensure.cs
//
// Author:
//       flynn <>
//
// Copyright (c) 2013 flynn
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

namespace OVHApi.Tools
{
	internal class Ensure
	{
		public static void IdIsValid(string parameterName, long parameterValue)
		{
			if(parameterValue <= 0)
				throw new ArgumentNullException(String.Format("Invalid argument '{0}', cannot be 0.",parameterName));
		}
		public static void NotNull(string parameterName, object parameterValue)
		{
			if(parameterValue == null)
				throw new ArgumentNullException(String.Format("Invalid argument '{0}', cannot be null.",parameterName));
		}
		public static void NotNullNorEmpty(string parameterName, string parameterValue)
		{
			if(string.IsNullOrEmpty(parameterValue))
				throw new ArgumentException(String.Format("Invalid argument '{0}', cannot be null or empty",parameterName));
		}
	}
}


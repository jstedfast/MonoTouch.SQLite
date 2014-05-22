//
// DynamicExportAttribute.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
//

using System;
using System.Reflection;

using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;

namespace MonoTouch.SQLite
{
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property)]
	internal sealed class DynamicExportAttribute : Attribute
	{
		public DynamicExportAttribute (string selector, ArgumentSemantic semantic)
		{
			ArgumentSemantic = semantic;
			Selector = selector;
		}

		public DynamicExportAttribute (string selector)
		{
			Selector = selector;
		}

		public DynamicExportAttribute ()
		{
		}

		public ArgumentSemantic ArgumentSemantic {
			get; set;
		}

		public string Selector {
			get; set;
		}

		public ExportAttribute Export {
			get {
				return new ExportAttribute (Selector, ArgumentSemantic);
			}
		}

		public ExportAttribute GetterExport {
			get { return Export; }
		}

		public ExportAttribute SetterExport {
			get {
				var setter = string.Format ("set{0}{1}:", char.ToUpperInvariant (Selector[0]), Selector.Substring (1));

				return new ExportAttribute (setter, ArgumentSemantic);
			}
		}
	}
}

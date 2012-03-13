// 
// SQLiteSearchExpression.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Jeffrey Stedfast
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
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MonoTouch.SQLite {
	public abstract class SQLiteSearchExpression
	{
		public virtual List<SQLiteSearchExpression> Children {
			get { return null; }
		}
		
		public virtual bool HasChildren {
			get { return false; }
		}
		
		public abstract void AppendToQuery (StringBuilder query, List<object> args);
		
		public string ToString (out object[] args)
		{
			StringBuilder query = new StringBuilder ();
			List<object> list = new List<object> ();
			
			AppendToQuery (query, list);
			args = list.ToArray ();
			
			return query.ToString ();
		}
	}
	
	public class SQLiteWhereExpression : SQLiteSearchExpression
	{
		List<SQLiteSearchExpression> children = new List<SQLiteSearchExpression> ();
		
		public SQLiteWhereExpression ()
		{
		}
		
		public override List<SQLiteSearchExpression> Children {
			get { return children; }
		}
		
		public override bool HasChildren {
			get { return children.Count > 0; }
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			if (!HasChildren)
				return;
			
			query.Append ("where");
			
			for (int i = 0; i < children.Count; i++) {
				query.Append (" ");
				children[i].AppendToQuery (query, args);
			}
		}
	}
	
	public class SQLiteLikeExpression : SQLiteSearchExpression
	{
		public SQLiteLikeExpression (string field, string match)
		{
			FieldName = field;
			Match = match;
		}
		
		public string FieldName {
			get; private set;
		}
		
		public string Match {
			get; private set;
		}
		
		static char[] LikeSpecials = new char[] { '\\', '_', '%' };
		
		internal static string EscapeTextForLike (string text, out bool escaped)
		{
			int first = text.IndexOfAny (LikeSpecials);
			
			escaped = false;
			
			if (first == -1)
				return text;
			
			var sb = new StringBuilder (text, 0, first, text.Length + 1);
			
			for (int i = first; i < text.Length; i++) {
				switch (text[i]) {
				case '\\': // escape character
				case '_': // matches any single character
				case '%': // matches any sequence of zero or more characters
					sb.Append ('\\');
					escaped = true;
					break;
				default:
					break;
				}
				
				sb.Append (text[i]);
			}
			
			return sb.ToString ();
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			string pattern;
			bool escaped;
			
			pattern = EscapeTextForLike (Match, out escaped);
			if (escaped) {
				query.AppendFormat ("{0} like ? escape = ?", FieldName);
				args.Add ("%" + pattern + "%");
				args.Add ("\\");
			} else {
				query.AppendFormat ("{0} like ?", FieldName);
				args.Add ("%" + pattern + "%");
			}
		}
	}
	
	public class SQLiteExactExpression : SQLiteSearchExpression
	{
		public SQLiteExactExpression (string field, string match)
		{
			FieldName = field;
			Match = match;
		}
		
		public string FieldName {
			get; private set;
		}
		
		public string Match {
			get; private set;
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			query.AppendFormat ("{0} is \"{1}\"", FieldName, Match);
		}
	}
	
	public class SQLiteEqualsExpression : SQLiteSearchExpression
	{
		public SQLiteEqualsExpression (string field, object match)
		{
			FieldName = field;
			Match = match;
		}
		
		public string FieldName {
			get; private set;
		}
		
		public object Match {
			get; private set;
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			query.AppendFormat ("{0} = ?", FieldName);
			args.Add (Match);
		}
	}
	
	public class SQLiteIsExpression : SQLiteSearchExpression
	{
		public SQLiteIsExpression (string field, object match)
		{
			FieldName = field;
			Match = match;
		}
		
		public string FieldName {
			get; private set;
		}
		
		public object Match {
			get; private set;
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			query.AppendFormat ("{0} is ?", FieldName);
			args.Add (Match);
		}
	}
	
	public class SQLiteAndExpression : SQLiteSearchExpression
	{
		List<SQLiteSearchExpression> children = new List<SQLiteSearchExpression> ();
		
		public SQLiteAndExpression ()
		{
		}
		
		public override List<SQLiteSearchExpression> Children {
			get { return children; }
		}
		
		public override bool HasChildren {
			get { return children.Count > 0; }
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			if (children.Count > 1)
				query.Append ('(');
			
			for (int i = 0; i < children.Count; i++) {
				if (i > 0)
					query.Append (" and ");
				
				children[i].AppendToQuery (query, args);
			}
			
			if (children.Count > 1)
				query.Append (')');
		}
	}
	
	public class SQLiteOrExpression : SQLiteSearchExpression
	{
		List<SQLiteSearchExpression> children = new List<SQLiteSearchExpression> ();
		
		public SQLiteOrExpression ()
		{
		}
		
		public override List<SQLiteSearchExpression> Children {
			get { return children; }
		}
		
		public override bool HasChildren {
			get { return children.Count > 0; }
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			if (children.Count > 1)
				query.Append ('(');
			
			for (int i = 0; i < children.Count; i++) {
				if (i > 0)
					query.Append (" or ");
				
				children[i].AppendToQuery (query, args);
			}
			
			if (children.Count > 1)
				query.Append (')');
		}
	}
}

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
using System.Collections.ObjectModel;

namespace MonoTouch.SQLite {
	public interface ISQLiteSearchExpression
	{
		void AppendToQuery (StringBuilder query, List<object> args);
		string ToString (out object[] args);
	}
	
	public abstract class SQLiteSearchExpression : ISQLiteSearchExpression
	{
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
	
	public abstract class SQLiteComparisonExpression : SQLiteSearchExpression
	{
		public SQLiteComparisonExpression (string fieldName, object match)
		{
			FieldName = fieldName;
			Match = match;
		}
		
		public string FieldName {
			get; private set;
		}
		
		public abstract string Operator {
			get;
		}
		
		public object Match {
			get; private set;
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			query.AppendFormat ("{0} {1} ?", FieldName, Operator);
			args.Add (Match);
		}
	}
	
	public class SQLiteLikeExpression : SQLiteComparisonExpression
	{
		public SQLiteLikeExpression (string fieldName, string match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return "like"; }
		}
		
		static char[] LikeSpecials = new char[] { '\\', '_', '%' };
		
		internal static string EscapeTextForLike (string text, out bool escaped)
		{
			int first = text.IndexOfAny (LikeSpecials);
			
			if (first == -1) {
				escaped = false;
				return text;
			}
			
			var sb = new StringBuilder (text, 0, first, text.Length + 1);
			
			escaped = true;
			
			for (int i = first; i < text.Length; i++) {
				switch (text[i]) {
				case '\\': // escape character
				case '_': // matches any single character
				case '%': // matches any sequence of zero or more characters
					sb.Append ('\\');
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
			
			pattern = EscapeTextForLike ((string) Match, out escaped);
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
	
	public class SQLiteIsExpression : SQLiteComparisonExpression
	{
		public SQLiteIsExpression (string fieldName, object match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return "is"; }
		}
	}
	
	public class SQLiteEqualToExpression : SQLiteComparisonExpression
	{
		public SQLiteEqualToExpression (string fieldName, object match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return "="; }
		}
	}
	
	public class SQLiteGreaterThanOrEqualToExpression : SQLiteComparisonExpression
	{
		public SQLiteGreaterThanOrEqualToExpression (string fieldName, object match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return ">="; }
		}
	}
	
	public class SQLiteGreaterThanExpression : SQLiteComparisonExpression
	{
		public SQLiteGreaterThanExpression (string fieldName, object match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return ">"; }
		}
	}
	
	public class SQLiteLessThanOrEqualToExpression : SQLiteComparisonExpression
	{
		public SQLiteLessThanOrEqualToExpression (string fieldName, object match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return "<="; }
		}
	}
	
	public class SQLiteLessThanExpression : SQLiteComparisonExpression
	{
		public SQLiteLessThanExpression (string fieldName, object match) : base (fieldName, match)
		{
		}
		
		public override string Operator {
			get { return "<"; }
		}
	}
	
	public class SQLiteWhereExpression : SQLiteSearchExpression
	{
		public SQLiteWhereExpression ()
		{
		}
		
		public SQLiteWhereExpression (SQLiteCollectionExpression expr)
		{
			Expression = expr;
		}
		
		public SQLiteCollectionExpression Expression {
			get; set;
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			if (Expression == null || Expression.Count == 0)
				return;
			
			query.Append ("where ");
			Expression.AppendToQuery (query, args);
		}
	}
	
	public abstract class SQLiteCollectionExpression : SQLiteSearchExpression, ICollection<ISQLiteSearchExpression>
	{
		List<ISQLiteSearchExpression> children = new List<ISQLiteSearchExpression> ();
		
		public SQLiteCollectionExpression ()
		{
		}
		
		public SQLiteCollectionExpression (IEnumerable<ISQLiteSearchExpression> collection)
		{
			children.AddRange (collection);
		}
		
		public ReadOnlyCollection<ISQLiteSearchExpression> Children {
			get { return children.AsReadOnly (); }
		}
		
		public abstract string Operator {
			get;
		}
		
		public override void AppendToQuery (StringBuilder query, List<object> args)
		{
			if (children.Count > 1)
				query.Append ('(');
			
			for (int i = 0; i < children.Count; i++) {
				if (i > 0) {
					query.Append (' ');
					query.Append (Operator);
					query.Append (' ');
				}
				
				children[i].AppendToQuery (query, args);
			}
			
			if (children.Count > 1)
				query.Append (')');
		}
		
		public void AddRange (IEnumerable<ISQLiteSearchExpression> collection)
		{
			children.AddRange (collection);
		}

		#region ICollection[ISQLiteSearchExpression] implementation
		public void Add (ISQLiteSearchExpression expr)
		{
			children.Add (expr);
		}

		public void Clear ()
		{
			children.Clear ();
		}

		public bool Contains (ISQLiteSearchExpression expr)
		{
			return children.Contains (expr);
		}

		public void CopyTo (ISQLiteSearchExpression[] array, int arrayIndex)
		{
			children.CopyTo (array, arrayIndex);
		}

		public bool Remove (ISQLiteSearchExpression expr)
		{
			return children.Remove (expr);
		}

		public int Count {
			get { return children.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}
		#endregion

		#region IEnumerable[ISQLiteSearchExpression] implementation
		public IEnumerator<ISQLiteSearchExpression> GetEnumerator ()
		{
			return children.GetEnumerator ();
		}
		#endregion

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return children.GetEnumerator ();
		}
		#endregion
	}
	
	public class SQLiteAndExpression : SQLiteCollectionExpression
	{
		public SQLiteAndExpression ()
		{
		}
		
		public SQLiteAndExpression (IEnumerable<ISQLiteSearchExpression> collection) : base (collection)
		{
		}
		
		public override string Operator {
			get { return "and"; }
		}
	}
	
	public class SQLiteOrExpression : SQLiteCollectionExpression
	{
		public SQLiteOrExpression ()
		{
		}
		
		public SQLiteOrExpression (IEnumerable<ISQLiteSearchExpression> collection) : base (collection)
		{
		}
		
		public override string Operator {
			get { return "or"; }
		}
	}
}

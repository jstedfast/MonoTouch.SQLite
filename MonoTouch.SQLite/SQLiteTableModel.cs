// 
// SQLiteTableModel.cs
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace MonoTouch.SQLite {
	public enum SQLiteOrderByDirection {
		Ascending,
		Descending
	}
	
	public class SQLiteOrderBy {
		public SQLiteOrderByDirection Direction { get; private set; }
		public string FieldName { get; private set; }
		
		public SQLiteOrderBy (string field) : this (field, SQLiteOrderByDirection.Ascending) { }
		
		public SQLiteOrderBy (string field, SQLiteOrderByDirection dir)
		{
			FieldName = field;
			Direction = dir;
		}
		
		public override string ToString ()
		{
			if (Direction == SQLiteOrderByDirection.Descending)
				return string.Format ("\"{0}\" desc", FieldName);
			
			return string.Format ("\"{0}\"", FieldName);
		}
	}
	
	[AttributeUsage (AttributeTargets.Property)]
	public class SQLiteSearchAliasAttribute : Attribute {
		public SQLiteSearchAliasAttribute (string alias)
		{
			Alias = alias;
		}
		
		public string Alias {
			get; private set;
		}
	}
	
	public class SQLiteTableModel<T> : IDisposable where T : new ()
	{
		class SectionTitle {
			public string Title { get; set; }
		}
		
		Dictionary<string, List<string>> aliases = new Dictionary<string, List<string>> (StringComparer.InvariantCultureIgnoreCase);
		Dictionary<string, Type> types = new Dictionary<string, Type> (StringComparer.InvariantCultureIgnoreCase);
		SQLiteWhereExpression searchExpr = null;
		List<T> cache = new List<T> ();
		string searchText = null;
		TableMapping titleMap;
		object[] query_args;
		string[] titles;
		string query;
		int sections;
		int[] rows;
		int offset;
		int count;
		
		void Initialize (Type type)
		{
			List<string> list;
			
			// Generate a search-term alias mapping for all of the fields of 'T'
			foreach (var prop in type.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)) {
				if (!prop.CanWrite)
					continue;
				
				if (prop.GetCustomAttributes (typeof (IgnoreAttribute), true).Length > 0)
					continue;
				
				if (prop.GetCustomAttributes (typeof (PrimaryKeyAttribute), true).Length > 0)
					continue;
				
				var attrs = prop.GetCustomAttributes (typeof (SQLiteSearchAliasAttribute), true);
				
				if (attrs != null && attrs.Length > 0) {
					foreach (var attr in attrs) {
						SQLiteSearchAliasAttribute alias = (SQLiteSearchAliasAttribute) attr;
						
						if (!aliases.TryGetValue (alias.Alias, out list))
							aliases[alias.Alias] = list = new List<string> ();
						
						list.Add (prop.Name);
					}
				} else {
					if (!aliases.TryGetValue (prop.Name, out list))
						aliases[prop.Name] = list = new List<string> ();
					
					list.Add (prop.Name);
				}
				
				types.Add (prop.Name, prop.PropertyType);
			}
		}
		
		public SQLiteTableModel (SQLiteConnection sqlitedb, int pageSize, SQLiteOrderBy orderBy, string sectionExpr)
		{
			SectionExpression = sectionExpr;
			Connection = sqlitedb;
			PageSize = pageSize;
			OrderBy = orderBy;
			
			if (sectionExpr != null) {
				titleMap = new TableMapping (typeof (SectionTitle));
				titleMap.Columns[0].Name = sectionExpr;
			}
			
			Initialize (typeof (T));
			ReloadData ();
		}
		
		/// <summary>
		/// Gets the SQLite connection used by the model.
		/// </summary>
		/// <value>
		/// The SQLite connection used by the model.
		/// </value>
		public SQLiteConnection Connection {
			get; private set;
		}
		
		/// <summary>
		/// Gets the total number of items in the table.
		/// </summary>
		/// <value>
		/// The total number of items in the table.
		/// </value>
		public int Count {
			get {
				if (count == -1) {
					count = 0;
					
					for (int i = 0; i < SectionCount; i++)
						count += GetRowCount (i);
				}
				
				return count;
			}
		}
		
		/// <summary>
		/// Gets the section expression used by the model to query table sections.
		/// </summary>
		/// <value>
		/// The section expression.
		/// </value>
		protected string SectionExpression {
			get; private set;
		}
		
		/// <summary>
		/// Gets the sort order of the model.
		/// </summary>
		/// <value>
		/// The sort order of the model.
		/// </value>
		public SQLiteOrderBy OrderBy {
			get; private set;
		}
		
		/// <summary>
		/// Gets the page size (used for limiting the SQLite queries).
		/// </summary>
		/// <value>
		/// The size of the page.
		/// </value>
		public int PageSize {
			get; private set;
		}
		
		/// <summary>
		/// Gets or sets the current search expression.
		/// </summary>
		/// <value>
		/// The search expression.
		/// </value>
		public SQLiteWhereExpression SearchExpression {
			get { return searchExpr; }
			set {
				if (value == searchExpr)
					return;
				
				searchExpr = value;
				query_args = null;
				query = null;
				
				ReloadData ();
			}
		}
		
		/// <summary>
		/// Gets or sets the search text. Setting this value will also update
		/// <see cref="P:FlightLog.SQLiteTableModel.SearchExpression"/>.
		/// </summary>
		/// <value>
		/// The search text.
		/// </value>
		public string SearchText {
			get { return searchText; }
			set {
				if (value == searchText)
					return;
				
				SearchExpression = ParseSearchExpression (value);
				searchText = value;
			}
		}
		
		/// <summary>
		/// Gets the name of the SQLite table.
		/// </summary>
		/// <value>
		/// The name of the table.
		/// </value>
		public string TableName {
			get { return Connection.Table<T> ().Table.TableName; }
		}
		
		static string GetNextToken (string text, ref int i, bool allowField, out bool quoted)
		{
			while (i < text.Length && char.IsWhiteSpace (text[i]))
				i++;
			
			quoted = false;
			
			if (i == text.Length)
				return null;
			
			int start = i;
			int length;
			
			if (text[i] != '"') {
				while (i < text.Length && !char.IsWhiteSpace (text[i])) {
					if (text[i] == ':' && allowField)
						break;
					
					i++;
				}
				
				length = i - start;
			} else {
				quoted = true;
				
				start = ++i;
				while (i < text.Length && text[i] != '"')
						i++;
				
				length = i - start;
				
				if (i < text.Length) {
					// consume the end quote
					i++;
				}
			}
			
			if (length > 0)
				return text.Substring (start, length);
			
			return null;
		}
		
		SQLiteWhereExpression ParseSearchExpression (string text)
		{
			SQLiteWhereExpression where = new SQLiteWhereExpression ();
			SQLiteAndExpression and = new SQLiteAndExpression ();
			List<string> fields;
			bool quoted;
			
			for (int i = 0; i < text.Length; i++) {
				string token = GetNextToken (text, ref i, true, out quoted);
				
				if (i < text.Length && text[i] == ':') {
					// The user may have requested for a particular field be matched...
					i++;
					
					if (string.IsNullOrEmpty (token)) {
						// lone ':', just ignore...
						continue;
					}
					
					string match = GetNextToken (text, ref i, false, out quoted);
					
					if (match != null) {
						if (aliases.TryGetValue (token, out fields)) {
							SQLiteOrExpression or = new SQLiteOrExpression ();
							
							// Search only the aliased fields for the string token...
							foreach (var field in fields)
								or.Children.Add (new SQLiteLikeExpression (field, match));
							
							and.Children.Add (or);
						}
					} else {
						SQLiteOrExpression or = new SQLiteOrExpression ();
						
						foreach (var col in types) {
							if (col.Value == typeof (string))
								or.Children.Add (new SQLiteLikeExpression (col.Key, token));
						}
						
						and.Children.Add (or);
					}
				} else if (token != null) {
					// Search all fields for this string token...
					SQLiteOrExpression or = new SQLiteOrExpression ();
					
					if (!quoted && aliases.TryGetValue (token, out fields)) {
						foreach (var field in fields) {
							if (types[field] == typeof (bool))
								or.Children.Add (new SQLiteIsExpression (field, true));
						}
					}
					
					foreach (var col in types) {
						if (col.Value == typeof (string))
							or.Children.Add (new SQLiteLikeExpression (col.Key, token));
					}
					
					and.Children.Add (or);
				}
			}
			
			if (!and.HasChildren)
				return null;
			
			where.Children.Add (and);
			
			return where;
		}
		
		/// <summary>
		/// Creates the command to get the number of sections.
		/// </summary>
		/// <returns>
		/// The command to get the number of sections.
		/// </returns>
		protected virtual SQLiteCommand CreateSectionCountCommand ()
		{
			if (SectionExpression == null)
				return null;
			
			string query = "select count (distinct " + SectionExpression + ") from \"" + TableName + "\"";
			object[] args;
			
			if (SearchExpression != null)
				query += " " + SearchExpression.ToString (out args);
			else
				args = new object [0];
			
			return Connection.CreateCommand (query, args);
		}
		
		/// <summary>
		/// Gets the section count.
		/// </summary>
		/// <value>
		/// The section count.
		/// </value>
		public int SectionCount {
			get {
				if (sections == -1) {
					var cmd = CreateSectionCountCommand ();
					if (cmd != null)
						sections = cmd.ExecuteScalar<int> ();
					else
						sections = 1;
				}
				
				return sections;
			}
		}
		
		/// <summary>
		/// Creates the command used to get the list of section titles.
		/// </summary>
		/// <returns>
		/// The command to get the list of section titles or null if
		/// the model does not have any sections or titles.
		/// </returns>
		protected virtual SQLiteCommand CreateSectionTitlesCommand ()
		{
			if (SectionExpression == null)
				return null;
			
			string query = "select distinct " + SectionExpression + " from \"" + TableName + "\" as Title";
			object[] args;
			
			if (SearchExpression != null)
				query += " " + SearchExpression.ToString (out args);
			else
				args = new object [0];
			
			if (OrderBy != null)
				query += " order by " + OrderBy.ToString ();
			
			return Connection.CreateCommand (query, args);
		}
		
		/// <summary>
		/// Gets the section titles.
		/// </summary>
		/// <value>
		/// The section titles or null if there are no titles.
		/// </value>
		public string[] SectionTitles {
			get {
				if (titles == null) {
					var cmd = CreateSectionTitlesCommand ();
					if (cmd != null)
						titles = cmd.ExecuteQuery<SectionTitle> (titleMap).Select (x => x.Title).ToArray ();
				}
				
				return titles;
			}
		}
		
		/// <summary>
		/// Creates the command to get the number of rows in the given section.
		/// </summary>
		/// <returns>
		/// The command to get the number of rows in the given section.
		/// </returns>
		/// <param name='section'>
		/// The section to get the number of rows for.
		/// </param>
		protected virtual SQLiteCommand CreateRowCountCommand (int section)
		{
			string query = "select count (*) from \"" + TableName + "\"";
			object[] args;
			
			if (SectionExpression != null) {
				SQLiteWhereExpression where = new SQLiteWhereExpression ();
				SQLiteAndExpression and = new SQLiteAndExpression ();
				
				and.Children.Add (new SQLiteExactExpression (SectionExpression, SectionTitles[section]));
				if (SearchExpression != null && SearchExpression.HasChildren)
					and.Children.AddRange (SearchExpression.Children);
				
				where.Children.Add (and);
				
				query += " " + where.ToString (out args);
			} else if (SearchExpression != null) {
				query += " " + SearchExpression.ToString (out args);
			} else {
				args = new object [0];
			}
			
			return Connection.CreateCommand (query, args);
		}
		
		/// <summary>
		/// Gets the number of rows in the given section.
		/// </summary>
		/// <returns>
		/// The number of rows in the given section.
		/// </returns>
		/// <param name='section'>
		/// The section to get the number of rows for.
		/// </param>
		public int GetRowCount (int section)
		{
			if (rows == null) {
				rows = new int [SectionCount];
				for (int i = 0; i < rows.Length; i++)
					rows[i] = -1;
			}
			
			if (section >= rows.Length)
				return -1;
			
			if (rows[section] == -1) {
				var cmd = CreateRowCountCommand (section);
				rows[section] = cmd.ExecuteScalar<int> ();
			}
			
			return rows[section];
		}
		
		/// <summary>
		/// Maps the given index to a section and row. <seealso cref="SectionAndRowToIndex"/>
		/// </summary>
		/// <returns>
		/// <c>true</c> if the index was successfully mapped to a section and row, or <c>false</c> otherwise.
		/// </returns>
		/// <param name='index'>
		/// The index to map to a section and row.
		/// </param>
		/// <param name='section'>
		/// The mapped section.
		/// </param>
		/// <param name='row'>
		/// The mapped row.
		/// </param>
		public bool IndexToSectionAndRow (int index, out int section, out int row)
		{
			int curIndex = 0;
			int count = 0;
			int i;
			
			section = 0;
			row = 0;
			
			for (i = 0; i < SectionCount; i++) {
				count = GetRowCount (section);
				if (curIndex + count > index)
					break;
				
				section++;
				
				if (curIndex + count == index)
					break;
				
				curIndex += count;
			}
			
			if (i == SectionCount)
				return false;
			
			row = index - curIndex;
			
			return true;
		}
		
		/// <summary>
		/// Maps a section and row to an index. <seealso cref="IndexToSectionAndRow"/>
		/// </summary>
		/// <returns>
		/// The index.
		/// </returns>
		/// <param name='section'>
		/// The section.
		/// </param>
		/// <param name='row'>
		/// The row.
		/// </param>
		public int SectionAndRowToIndex (int section, int row)
		{
			int index = 0;
			
			for (int i = 0; i < section; i++)
				index += GetRowCount (i);
			
			return index + row;
		}
		
		/// <summary>
		/// Creates the query string and arguments to get row data from
		/// the table using the current <see cref="SearchExpression"/>.
		/// </summary>
		/// <returns>
		/// The query string.
		/// </returns>
		/// <param name='args'>
		/// The argument vector for the query.
		/// </param>
		protected virtual string CreateQuery (out object[] args)
		{
			string query = "select * from \"" + TableName + "\"";
			
			if (SearchExpression != null)
				query += " " + SearchExpression.ToString (out args);
			else
				args = new object[0];
			
			return query;
		}
		
		SQLiteCommand CreateQueryCommand (int limit, int offset)
		{
			if (query == null) {
				query = CreateQuery (out query_args);
				
				if (OrderBy != null)
					query += " order by " + OrderBy.ToString ();
			}
			
			string command = query + " limit " + limit + " offset " + offset;
			
			return Connection.CreateCommand (command, query_args);
		}
		
		/// <summary>
		/// Gets the item specified by the given index.
		/// </summary>
		/// <returns>
		/// The requested item or null if it doesn't exist.
		/// </returns>
		/// <param name='index'>
		/// The index of the item.
		/// </param>
		public T GetItem (int index)
		{
			int limit = PageSize;
			
			if (index < 0)
				return default (T);
			
			//Connection.Trace = true;
			
			if (index == offset - 1) {
				// User is scrolling up. Fetch the previous page of items...
				int first = Math.Max (offset - PageSize, 0);
				
				// Calculate the number of items we need to fetch...
				limit = offset - first;
				
				// Calculate the number of items we need to uncache...
				int rem = limit - ((2 * PageSize) - cache.Count);
				
				if (rem > 0)
					cache.RemoveRange (cache.Count - rem, rem);
				
				var cmd = CreateQueryCommand (limit, first);
				var results = cmd.ExecuteQuery<T> ();
				
				// Insert our new items at the head of our cache list...
				cache.InsertRange (0, results);
				offset = first;
			} else if (index == offset + cache.Count) {
				// User is scrolling down. Fetch the next page of items...
				if (cache.Count > PageSize)
					cache.RemoveRange (0, cache.Count - PageSize);
				
				// Load 2 pages if we are at the beginning
				if (index == 0)
					limit = 2 * PageSize;
				
				var cmd = CreateQueryCommand (limit, index);
				var results = cmd.ExecuteQuery<T> ();
				
				offset = Math.Max (index - PageSize, 0);
				cache.AddRange (results);
			} else if (index < offset || index > offset + cache.Count) {
				// User is requesting an item in the middle of no-where...
				// align to the page enclosing the given index.
				// Note: this only works if PageSize is a power of 2.
				//int first = ((index + (PageSize - 1)) & ~(PageSize - 1)) - PageSize;
				int first = (index / PageSize) * PageSize;
				
				limit = 2 * PageSize;
				cache.Clear ();
				
				var cmd = CreateQueryCommand (limit, first);
				var results = cmd.ExecuteQuery<T> ();
				cache.AddRange (results);
				offset = first;
			}
			
			//Connection.Trace = false;
			
			index -= offset;
			if (index < cache.Count)
				return cache[index];
			
			return default (T);
		}
		
		/// <summary>
		/// Gets the item specified by the given section and row.
		/// </summary>
		/// <returns>
		/// The requested item or null if it doesn't exist.
		/// </returns>
		/// <param name='section'>
		/// The section containing the requested row.
		/// </param>
		/// <param name='row'>
		/// The row of the requested item.
		/// </param>
		public T GetItem (int section, int row)
		{
			int index = SectionAndRowToIndex (section, row);
			
			return GetItem (index);
		}
		
		/// <summary>
		/// Gets the index of the specified item.
		/// </summary>
		/// <returns>
		/// The index of the specified item, or -1 if not found.
		/// </returns>
		/// <param name='item'>
		/// The item to find the index of.
		/// </param>
		/// <param name='cmp'>
		/// The item comparer.
		/// </param>
		public int IndexOf (T item, IComparer<T> cmp)
		{
			SQLiteCommand command;
			List<T> results;
			int hi = Count;
			int lo = 0;
			int index;
			T other;
			int v;
			
			if (hi == 0)
				return -1;
			
			do {
				index = lo + (hi - lo) / 2;
				
				if (index >= offset && index - offset < cache.Count) {
					other = cache[index - offset];
				} else {
					command = CreateQueryCommand (1, index);
					results = command.ExecuteQuery<T> ();
					if (results.Count == 0)
						break;
					
					other = results[0];
				}
				
				if ((v = cmp.Compare (item, other)) == 0)
					return index;
				
				if (v > 0)
					lo = index + 1;
				else
					hi = index;
			} while (lo < hi);
			
			return -1;
		}
		
		/// <summary>
		/// Reloads the model. This should be called whenever the SQLite table changes
		/// (e.g. whenever an item is added or removed).
		/// </summary>
		public virtual void ReloadData ()
		{
			cache.Clear ();
			titles = null;
			sections = -1;
			rows = null;
			offset = 0;
			count = -1;
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			ReloadData ();
		}
		#endregion
	}
}

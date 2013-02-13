//
// SQLiteOrderByCollection.cs
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

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MonoTouch.SQLite {
	public class SQLiteOrderByCollection : IList<SQLiteOrderBy>
	{
		List<SQLiteOrderBy> list = new List<SQLiteOrderBy> ();

		internal SQLiteOrderByCollection ()
		{
		}

		#region ICollection[SQLiteOrderBy] implementation
		public void Add (SQLiteOrderBy orderBy)
		{
			list.Add (orderBy);
			OnChanged ();
		}

		public void Clear ()
		{
			list.Clear ();
			OnChanged ();
		}

		public bool Contains (SQLiteOrderBy orderBy)
		{
			return list.Contains (orderBy);
		}

		public void CopyTo (SQLiteOrderBy[] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove (SQLiteOrderBy orderBy)
		{
			if (list.Remove (orderBy)) {
				OnChanged ();
				return true;
			}

			return false;
		}
		#endregion

		#region IList[SQLiteOrderBy] implementation
		public int IndexOf (SQLiteOrderBy orderBy)
		{
			return list.IndexOf (orderBy);
		}

		public void Insert (int index, SQLiteOrderBy orderBy)
		{
			list.Insert (index, orderBy);
			OnChanged ();
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
			OnChanged ();
		}

		public SQLiteOrderBy this[int index] {
			get { return list[index]; }
			set {
				list[index] = value;
				OnChanged ();
			}
		}
		#endregion

		#region IEnumerable[SQLiteOrderBy] implementation
		public IEnumerator<SQLiteOrderBy> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		#endregion

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable) list).GetEnumerator ();
		}
		#endregion

		public override string ToString ()
		{
			if (list.Count == 0)
				return string.Empty;

			StringBuilder sb = new StringBuilder ("order by");
			for (int i = 0; i < list.Count; i++) {
				if (i > 0)
					sb.Append (',');

				if (list[i].SortOrder == SQLiteSortOrder.Descending)
					sb.AppendFormat (" \"{0}\" desc", list[i].FieldName);
				else
					sb.AppendFormat (" \"{0}\"", list[i].FieldName);
			}

			return sb.ToString ();
		}

		internal event EventHandler<EventArgs> Changed;

		void OnChanged ()
		{
			var handler = Changed;

			if (handler != null)
				handler (this, null);
		}
	}
}

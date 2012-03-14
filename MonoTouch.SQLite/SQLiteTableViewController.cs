// 
// SQLiteTableViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.SQLite
{
	public abstract class SQLiteTableViewController<T> : AllInOneTableViewController where T : new ()
	{
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize, SQLiteOrderBy orderBy, string sectionExpression) : base (UITableViewStyle.Plain)
		{
			SearchModel = new SQLiteTableModel<T> (sqlitedb, pageSize, orderBy, sectionExpression);
			Model = new SQLiteTableModel<T> (sqlitedb, pageSize, orderBy, sectionExpression);
		}
		
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize, SQLiteOrderBy orderBy) : base (UITableViewStyle.Plain)
		{
			SearchModel = new SQLiteTableModel<T> (sqlitedb, pageSize, orderBy, null);
			Model = new SQLiteTableModel<T> (sqlitedb, pageSize, orderBy, null);
		}
		
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize) : base (UITableViewStyle.Plain)
		{
			SearchModel = new SQLiteTableModel<T> (sqlitedb, pageSize, null, null);
			Model = new SQLiteTableModel<T> (sqlitedb, pageSize, null, null);
		}
		
		public SQLiteTableModel<T> SearchModel {
			get; private set;
		}
		
		public SQLiteTableModel<T> Model {
			get; private set;
		}
		
		public void ReloadData ()
		{
			SearchModel.ReloadData ();
			Model.ReloadData ();
			
			if (SearchDisplayController != null)
				SearchDisplayController.SearchResultsTableView.ReloadData ();
			
			TableView.ReloadData ();
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			SearchDisplayController.SearchResultsTableView.AllowsMultipleSelectionDuringEditing = false;
			SearchDisplayController.SearchResultsTableView.AllowsSelectionDuringEditing = false;
			SearchDisplayController.SearchResultsTableView.AllowsMultipleSelection = false;
			SearchDisplayController.SearchResultsTableView.AllowsSelection = false;
			SearchDisplayController.SearchResultsTableView.SectionFooterHeight = 0;
			
			TableView.AllowsMultipleSelectionDuringEditing = false;
			TableView.AllowsSelectionDuringEditing = false;
			TableView.AllowsMultipleSelection = false;
			TableView.AllowsSelection = false;
			TableView.SectionFooterHeight = 0;
		}
		
		protected SQLiteTableModel ModelForTableView (UITableView tableView)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView)
				return SearchModel;
			else
				return Model;
		}
		
		protected NSIndexPath PathForVisibleItem (UITableView tableView, T item)
		{
			SQLiteTableModel model = ModelForTableView (tableView);
			
			foreach (var path in tableView.IndexPathsForVisibleRows) {
				var visibleItem = model.GetItem (path.Section, path.Row);
				
				if (visibleItem == item)
					return path;
			}
			
			return null;
		}
		
		public void ReloadRowForItem (UITableView tableView, T item)
		{
			var path = PathForVisibleItem (tableView, item);
			
			if (path != null)
				tableView.ReloadRows (new NSIndexPath[] { path }, UITableViewRowAnimation.Automatic);
		}
		
		protected override int NumberOfSections (UITableView tableView)
		{
			return ModelForTableView (tableView).SectionCount;
		}
		
		protected override string TitleForHeader (UITableView tableView, int section)
		{
			return ModelForTableView (tableView).SectionTitles[section];
		}
		
		protected override int RowsInSection (UITableView tableView, int section)
		{
			return ModelForTableView (tableView).GetRowCount (section);
		}
		
		protected virtual UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath, T item)
		{
			throw new NotImplementedException ();
		}
		
		protected T GetItem (UITableView tableView, NSIndexPath indexPath)
		{
			return ModelForTableView (tableView).GetItem (indexPath.Section, indexPath.Row);
		}
		
		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			return GetCell (tableView, indexPath, GetItem (tableView, indexPath));
		}
		
		protected override bool ShouldReloadForSearchString (UISearchDisplayController controller, string search)
		{
			SearchModel.SearchText = search;
			return true;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (SearchModel != null) {
				SearchModel.Dispose ();
				SearchModel = null;
			}
			
			if (Model != null) {
				Model.Dispose ();
				Model = null;
			}
		}
	}
}

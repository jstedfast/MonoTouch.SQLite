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
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.SQLite.SQLiteTableViewController`1"/> class.
		/// </summary>
		/// <param name='sqlitedb'>
		/// The SQLite database connection.
		/// </param>
		/// <param name='pageSize'>
		/// The number of items to page-in and page-out as the user scrolls.
		/// </param>
		/// <param name='orderBy'>
		/// The field to sort by and the order in which to display the data or null to display the data in the default order.
		/// </param>
		/// <param name='sectionExpression'>
		/// The sub-expression used to get distinct sections and their titles or null to display the data as a flat list.
		/// </param>
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize, SQLiteOrderBy orderBy, string sectionExpression)
			: base (UITableViewStyle.Plain, true)
		{
			SearchModel = new SQLiteTableModel<T> (sqlitedb, pageSize, orderBy, sectionExpression);
			Model = new SQLiteTableModel<T> (sqlitedb, pageSize, orderBy, sectionExpression);
			AutoHideSearch = true;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.SQLite.SQLiteTableViewController`1"/> class.
		/// </summary>
		/// <param name='sqlitedb'>
		/// The SQLite database connection.
		/// </param>
		/// <param name='pageSize'>
		/// The number of items to page-in and page-out as the user scrolls.
		/// </param>
		/// <param name='sectionExpression'>
		/// The sub-expression used to get distinct sections and their titles or null to display the data as a flat list.
		/// </param>
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize, string sectionExpression)
			: this (sqlitedb, pageSize, null, sectionExpression)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.SQLite.SQLiteTableViewController`1"/> class.
		/// </summary>
		/// <param name='sqlitedb'>
		/// The SQLite database connection.
		/// </param>
		/// <param name='pageSize'>
		/// The number of items to page-in and page-out as the user scrolls.
		/// </param>
		/// <param name='orderBy'>
		/// The field to sort by and the order in which to display the data or null to display the data in the default order.
		/// </param>
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize, SQLiteOrderBy orderBy)
			: this (sqlitedb, pageSize, orderBy, null)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.SQLite.SQLiteTableViewController`1"/> class.
		/// </summary>
		/// <param name='sqlitedb'>
		/// The SQLite database connection.
		/// </param>
		/// <param name='pageSize'>
		/// The number of items to page-in and page-out as the user scrolls.
		/// </param>
		public SQLiteTableViewController (SQLiteConnection sqlitedb, int pageSize)
			: this (sqlitedb, pageSize, null, null)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.SQLite.SQLiteTableViewController`1"/> class.
		/// 
		/// Note: If you use this .ctor, you'll need to implement <see cref="CreateModel(bool)"/>.
		/// </summary>
		/// <param name='style'>
		/// The UITableViewStyle.
		/// </param>
		public SQLiteTableViewController (UITableViewStyle style) : base (style, true)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.SQLite.SQLiteTableViewController`1"/>
		/// class using the UITableViewStyle.Plain style.
		/// 
		/// Note: If you use this .ctor, you'll need to implement <see cref="CreateModel(bool)"/>.
		/// </summary>
		public SQLiteTableViewController () : base (UITableViewStyle.Plain, true)
		{
		}
		
		/// <summary>
		/// Gets the model used when displaying data filtered by the search criteria.
		/// </summary>
		/// <value>
		/// The search data model.
		/// </value>
		public SQLiteTableModel<T> SearchModel {
			get; private set;
		}
		
		/// <summary>
		/// Gets the model used when displaying unfiltered data.
		/// </summary>
		/// <value>
		/// The data model.
		/// </value>
		public SQLiteTableModel<T> Model {
			get; private set;
		}
		
		/// <summary>
		/// Reloads the data.
		/// </summary>
		public void ReloadData ()
		{
			if (CanSearch)
				SearchModel.ReloadData ();

			Model.ReloadData ();
			
			if (SearchDisplayController != null)
				SearchDisplayController.SearchResultsTableView.ReloadData ();
			
			TableView.ReloadData ();
		}
		
		/// <summary>
		/// Creates the model.
		/// </summary>
		/// <returns>
		/// The model.
		/// </returns>
		/// <param name='forSearching'>
		/// Whether or not the model will be used for searching.
		/// </param>
		protected virtual SQLiteTableModel<T> CreateModel (bool forSearching)
		{
			throw new NotImplementedException ("You need to implement CreateModel(bool).");
		}
		
		public override void ViewDidLoad ()
		{
			if (Model == null)
				Model = CreateModel (false);
			
			if (CanSearch) {
				if (SearchModel == null)
					SearchModel = CreateModel (true);
			}

			base.ViewDidLoad ();

			if (CanSearch) {
				SearchDisplayController.SearchResultsTableView.SectionFooterHeight = 0;
				SearchDisplayController.SearchResultsTableView.AllowsSelection = true;
			}

			TableView.SectionFooterHeight = 0;
			TableView.AllowsSelection = true;
		}

		public override void DidReceiveMemoryWarning ()
		{
			if (CanSearch && SearchModel != null)
				SearchModel.ClearCache ();

			if (Model != null)
				Model.ClearCache ();

			base.DidReceiveMemoryWarning ();
		}
		
		/// <summary>
		/// Gets the models associated with the given UITableView. This will
		/// either be the <see cref="Model"/> or the <see cref="SearchModel"/>.
		/// </summary>
		/// <returns>
		/// The model for for the given table view.
		/// </returns>
		/// <param name='tableView'>
		/// The table view.
		/// </param>
		protected SQLiteTableModel<T> ModelForTableView (UITableView tableView)
		{
			if (CanSearch && tableView == SearchDisplayController.SearchResultsTableView)
				return SearchModel;
			else
				return Model;
		}
		
		/// <summary>
		/// Gets the path for the given item in the given table view if it is
		/// currently visible to the user.
		/// </summary>
		/// <returns>
		/// The path for the given item if it is visible, or null if it is not.
		/// </returns>
		/// <param name='tableView'>
		/// The table view that is responsible for displaying the item.
		/// </param>
		/// <param name='item'>
		/// The item to get the path for.
		/// </param>
		protected NSIndexPath PathForVisibleItem (UITableView tableView, T item)
		{
			SQLiteTableModel<T> model = ModelForTableView (tableView);
			
			foreach (var path in tableView.IndexPathsForVisibleRows) {
				var visibleItem = model.GetItem (path.Section, path.Row);
				
				if (visibleItem.Equals (item))
					return path;
			}
			
			return null;
		}
		
		/// <summary>
		/// Reloads the row for the given item if it is currently visible to the user.
		/// This method is useful if the item values have changed.
		/// </summary>
		/// <param name='tableView'>
		/// The table view that is responsible for displaying the item.
		/// </param>
		/// <param name='item'>
		/// The item that should be re-displayed.
		/// </param>
		public void ReloadRowForItem (UITableView tableView, T item)
		{
			var path = PathForVisibleItem (tableView, item);
			
			if (path != null)
				tableView.ReloadRows (new NSIndexPath[] { path }, UITableViewRowAnimation.None);
		}
		
		protected override int NumberOfSections (UITableView tableView)
		{
			return ModelForTableView (tableView).SectionCount;
		}
		
		protected override string TitleForHeader (UITableView tableView, int section)
		{
			string[] titles = ModelForTableView (tableView).SectionTitles;
			
			return titles != null ? titles[section] : null;
		}
		
		//protected override float GetHeightForHeader (UITableView tableView, int section)
		//{
		//	string[] titles = ModelForTableView (tableView).SectionTitles;
		//
		//	if (titles == null)
		//		return 0;
		//
		//	return base.GetHeightForHeader (tableView, section);
		//}
		
		//protected override float GetHeightForFooter (UITableView tableView, int section)
		//{
		//	return 0;
		//}
		
		protected override int RowsInSection (UITableView tableView, int section)
		{
			return ModelForTableView (tableView).GetRowCount (section);
		}
		
		/// <summary>
		/// Gets the cell for the given item.
		/// </summary>
		/// <returns>
		/// The cell used for displaying information about the given item.
		/// </returns>
		/// <param name='tableView'>
		/// The table view responsible for displaying the item.
		/// </param>
		/// <param name='indexPath'>
		/// The path of the item in the table view.
		/// </param>
		/// <param name='item'>
		/// The item to display.
		/// </param>
		protected abstract UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath, T item);
		
		/// <summary>
		/// Gets the item specified by the given path.
		/// </summary>
		/// <returns>
		/// The item.
		/// </returns>
		/// <param name='tableView'>
		/// The table view responsible for displaying the item.
		/// </param>
		/// <param name='indexPath'>
		/// The path of the item in the table view.
		/// </param>
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

// 
// AllInOneTableViewController.cs
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
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.SQLite {
	public abstract class AllInOneTableViewController : UITableViewController
	{
		class AllInOneTableViewDataSource : UITableViewDataSource
		{
			AllInOneTableViewController controller;
			
			public AllInOneTableViewDataSource (AllInOneTableViewController controller)
			{
				this.controller = controller;
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				return controller.NumberOfSections (tableView);
			}
			
			public override int RowsInSection (UITableView tableView, int section)
			{
				return controller.RowsInSection (tableView, section);
			}
			
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.GetCell (tableView, indexPath);
			}
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				return controller.TitleForHeader (tableView, section);
			}
			
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return controller.TitleForFooter (tableView, section);
			}
			
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.CanEditRow (tableView, indexPath);
			}
			
			public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.CanMoveRow (tableView, indexPath);
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				controller.CommitEditingStyle (tableView, editingStyle, indexPath);
			}
			
			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
				controller.MoveRow (tableView, sourceIndexPath, destinationIndexPath);
			}
		}
		
		class AllInOneTableViewDelegate : UITableViewDelegate
		{
			AllInOneTableViewController controller;
			
			public AllInOneTableViewDelegate (AllInOneTableViewController controller)
			{
				this.controller = controller;
			}
			
			public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
			{
				controller.AccessoryButtonTapped (tableView, indexPath);
			}
			
			public override bool ShouldIndentWhileEditing (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.ShouldIndentWhileEditing (tableView, indexPath);
			}
			
			public override int IndentationLevel (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.IndentationLevel (tableView, indexPath);
			}
			
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.EditingStyleForRow (tableView, indexPath);
			}
			
			public override void WillBeginEditing (UITableView tableView, NSIndexPath indexPath)
			{
				controller.WillBeginEditing (tableView, indexPath);
			}
			
			public override void DidEndEditing (UITableView tableView, NSIndexPath indexPath)
			{
				controller.DidEndEditing (tableView, indexPath);
			}
			
			public override UIView GetViewForHeader (UITableView tableView, int section)
			{
				return controller.GetViewForHeader (tableView, section);
			}
			
			public override UIView GetViewForFooter (UITableView tableView, int section)
			{
				return controller.GetViewForFooter (tableView, section);
			}
			
			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.GetHeightForRow (tableView, indexPath);
			}
			
			public override float GetHeightForHeader (UITableView tableView, int section)
			{
				return controller.GetHeightForHeader (tableView, section);
			}
			
			public override float GetHeightForFooter (UITableView tableView, int section)
			{
				return controller.GetHeightForFooter (tableView, section);
			}
			
			public override NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.WillSelectRow (tableView, indexPath);
			}
			
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				controller.RowSelected (tableView, indexPath);
			}
			
			public override void WillDeselectRow (UITableView tableView, NSIndexPath indexPath)
			{
				controller.WillDeselectRow (tableView, indexPath);
			}
			
			public override void RowDeselected (UITableView tableView, NSIndexPath indexPath)
			{
				controller.RowDeselected (tableView, indexPath);
			}
			
			public override string TitleForDeleteConfirmation (UITableView tableView, NSIndexPath indexPath)
			{
				return controller.TitleForDeleteConfirmation (tableView, indexPath);
			}
		}
		
		class AllInOneSearchDisplayDelegate : UISearchDisplayDelegate
		{
			AllInOneTableViewController tableViewController;
			
			public AllInOneSearchDisplayDelegate (AllInOneTableViewController controller)
			{
				this.tableViewController = controller;
			}
			
			public override void WillBeginSearch (UISearchDisplayController controller)
			{
				tableViewController.WillBeginSearch (controller);
			}
			
			public override void DidBeginSearch (UISearchDisplayController controller)
			{
				tableViewController.DidBeginSearch (controller);
			}
			
			public override void WillEndSearch (UISearchDisplayController controller)
			{
				tableViewController.WillEndSearch (controller);
			}
			
			public override void DidEndSearch (UISearchDisplayController controller)
			{
				tableViewController.DidEndSearch (controller);
			}
			
			public override void DidLoadSearchResults (UISearchDisplayController controller, UITableView tableView)
			{
				tableViewController.DidLoadSearchResults (controller, tableView);
			}
			
			public override void WillUnloadSearchResults (UISearchDisplayController controller, UITableView tableView)
			{
				tableViewController.WillUnloadSearchResults (controller, tableView);
			}
			
			public override void WillShowSearchResults (UISearchDisplayController controller, UITableView tableView)
			{
				tableViewController.WillShowSearchResults (controller, tableView);
			}
			
			public override void DidShowSearchResults (UISearchDisplayController controller, UITableView tableView)
			{
				tableViewController.DidShowSearchResults (controller, tableView);
			}
			
			public override void WillHideSearchResults (UISearchDisplayController controller, UITableView tableView)
			{
				tableViewController.WillHideSearchResults (controller, tableView);
			}
			
			public override void DidHideSearchResults (UISearchDisplayController controller, UITableView tableView)
			{
				tableViewController.DidHideSearchResults (controller, tableView);
			}
			
			public override bool ShouldReloadForSearchScope (UISearchDisplayController controller, int scope)
			{
				return tableViewController.ShouldReloadForSearchScope (controller, scope);
			}
			
			public override bool ShouldReloadForSearchString (UISearchDisplayController controller, string search)
			{
				return tableViewController.ShouldReloadForSearchString (controller, search);
			}
		}
		
		protected static float DefaultSearchBarHeight = 44.0f;
		
		UISearchDisplayController searchDisplayController;
		AllInOneSearchDisplayDelegate searchDisplayDelegate;
		AllInOneTableViewDataSource tableViewDataSource;
		AllInOneTableViewDelegate tableViewDelegate;
		UISearchBar searchBar;
		
		public AllInOneTableViewController (UITableViewStyle style) : base (style)
		{
			searchDisplayDelegate = new AllInOneSearchDisplayDelegate (this);
			tableViewDataSource = new AllInOneTableViewDataSource (this);
			tableViewDelegate = new AllInOneTableViewDelegate (this);
			ClearsSelectionOnViewWillAppear = false;
		}
		
		public bool AutoHideSearch {
			get; set;
		}
		
		public bool AutoRotate {
			get; set;
		}
		
		public override bool ClearsSelectionOnViewWillAppear {
			get; set;
		}
		
		public override UISearchDisplayController SearchDisplayController {
			get { return searchDisplayController; }
		}
		
		public string SearchPlaceholder {
			get; set;
		}
		
		protected virtual UISearchBar CreateSearchBar ()
		{
			return new UISearchBar (new RectangleF (0, 0, TableView.Bounds.Width, DefaultSearchBarHeight));
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			searchBar = CreateSearchBar ();
			if (SearchPlaceholder != null)
				searchBar.Placeholder = SearchPlaceholder;
			
			searchDisplayController = new UISearchDisplayController (searchBar, this);
			SearchDisplayController.SearchResultsDataSource = tableViewDataSource;
			SearchDisplayController.SearchResultsDelegate = tableViewDelegate;
			SearchDisplayController.Delegate = searchDisplayDelegate;
			TableView.TableHeaderView = searchBar;
			
			TableView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			TableView.AutosizesSubviews = true;
			
			TableView.DataSource = tableViewDataSource;
			TableView.Delegate = tableViewDelegate;
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			if (searchDisplayController != null) {
				searchDisplayController.Dispose ();
				searchDisplayController = null;
			}
			
			if (searchDisplayDelegate != null) {
				searchDisplayDelegate.Dispose ();
				searchDisplayDelegate = null;
			}
			
			if (tableViewDataSource != null) {
				tableViewDataSource.Dispose ();
				tableViewDataSource = null;
			}
			
			if (tableViewDelegate != null) {
				tableViewDelegate.Dispose ();
				tableViewDelegate = null;
			}
			
			if (searchBar != null) {
				searchBar.Dispose ();
				searchBar = null;
			}
		}
		
		public void HideSearchBar ()
		{
			if (TableView != null && TableView.ContentOffset.Y < searchBar.Frame.Height)
				TableView.ContentOffset = new PointF (0, searchBar.Frame.Height);
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			if (AutoHideSearch)
				HideSearchBar ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AutoRotate;
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
		}
		
		#region UITableVIewDataSource
		protected abstract int NumberOfSections (UITableView tableView);
		protected abstract int RowsInSection (UITableView tableView, int section);
		protected abstract UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath);
		
		protected virtual string TitleForHeader (UITableView tableView, int section)
		{
			return null;
		}
		
		protected virtual string TitleForFooter (UITableView tableView, int section)
		{
			return null;
		}
		
		protected virtual bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return false;
		}
		
		protected virtual bool CanMoveRow (UITableView tableView, NSIndexPath idnexPath)
		{
			return false;
		}
		
		protected virtual void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
		}
		
		protected virtual void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
		{
		}
		#endregion
		
		#region UITableViewDelegate
		protected virtual void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
		{
		}
		
		protected virtual bool ShouldIndentWhileEditing (UITableView tableView, NSIndexPath indexPath)
		{
			return false;
		}
		
		protected virtual int IndentationLevel (UITableView tableView, NSIndexPath indexPath)
		{
			return 0;
		}
		
		protected virtual UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.None;
		}
		
		protected virtual void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
		}
		
		protected virtual void WillBeginEditing (UITableView tableView, NSIndexPath indexPath)
		{
		}
		
		protected virtual void DidEndEditing (UITableView tableView, NSIndexPath indexPath)
		{
		}
		
		protected virtual UIView GetViewForHeader (UITableView tableView, int section)
		{
			return null;
		}
		
		protected virtual UIView GetViewForFooter (UITableView tableView, int section)
		{
			return null;
		}
		
		protected virtual float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return tableView.RowHeight;
		}
		
		protected virtual float GetHeightForHeader (UITableView tableView, int section)
		{
			return tableView.SectionHeaderHeight;
		}
		
		protected virtual float GetHeightForFooter (UITableView tableView, int section)
		{
			return tableView.SectionFooterHeight;
		}
		
		protected virtual NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
		{
			return indexPath;
		}
		
		protected virtual void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
		}
		
		protected virtual void WillDeselectRow (UITableView tableView, NSIndexPath indexPath)
		{
		}
		
		protected virtual void RowDeselected (UITableView tableView, NSIndexPath indexPath)
		{
		}
		
		protected virtual string TitleForDeleteConfirmation (UITableView tableView, NSIndexPath indexPath)
		{
			return "Delete";
		}
		#endregion
		
		#region UISearchDisplayDelegate
		protected virtual void WillBeginSearch (UISearchDisplayController controller)
		{
		}
		
		protected virtual void DidBeginSearch (UISearchDisplayController controller)
		{
		}
		
		protected virtual void WillEndSearch (UISearchDisplayController controller)
		{
		}
		
		protected virtual void DidEndSearch (UISearchDisplayController controller)
		{
		}
		
		protected virtual void DidLoadSearchResults (UISearchDisplayController controller, UITableView tableView)
		{
		}
		
		protected virtual void WillUnloadSearchResults (UISearchDisplayController controller, UITableView tableView)
		{
		}
		
		protected virtual void WillShowSearchResults (UISearchDisplayController controller, UITableView tableView)
		{
		}
		
		protected virtual void DidShowSearchResults (UISearchDisplayController controller, UITableView tableView)
		{
		}
		
		protected virtual void WillHideSearchResults (UISearchDisplayController controller, UITableView tableView)
		{
		}
		
		protected virtual void DidHideSearchResults (UISearchDisplayController controller, UITableView tableView)
		{
		}
		
		protected virtual bool ShouldReloadForSearchScope (UISearchDisplayController controller, int scope)
		{
			return true;
		}
		
		protected virtual bool ShouldReloadForSearchString (UISearchDisplayController controller, string search)
		{
			return true;
		}
		#endregion
	}
}

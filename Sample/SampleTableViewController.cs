// 
// SampleTableViewController.cs
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

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.SQLite;

namespace Sample {
	public class SampleItem {
		[PrimaryKey][AutoIncrement]
		public int ItemId { get; set; }
		
		public string Title { get; set; }
		public string Details { get; set; }
	}
	
	public class SampleTableViewController : SQLiteTableViewController<SampleItem>
	{
		static NSString key = new NSString ("SampleItemCellKey");
		
		public SampleTableViewController (SQLiteConnection sqlitedb)
			: base (sqlitedb, 16)
		{
		}
		
		// Note: This is the only method, other than the .ctor, that we actually need to implement in
		// order to have a functional SQLiteTableViewController capable of displaying data from our
		// SQLite table.
		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath, SampleItem item)
		{
			var cell = tableView.DequeueReusableCell (key);
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Value1, key);
			
			cell.TextLabel.Text = item.Title;
			cell.DetailTextLabel.Text = item.Details;
			
			return cell;
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
	}
}

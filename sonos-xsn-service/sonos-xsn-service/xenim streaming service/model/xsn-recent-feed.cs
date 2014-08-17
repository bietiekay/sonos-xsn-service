using System;
using System.Collections.Generic;

namespace sonosxsnservice
{
	public class xsn_recent_feed_item
	{
		public string website { get; set; }
		public string begin { get; set; }
		public string pubdate { get; set; }
		public string author_name { get; set; }
		public string link { get; set; }
		public string unique_id { get; set; }
		public string icon { get; set; }
		public string end { get; set; }
		public string description { get; set; }
		public string title { get; set; }
	}

	public class xsn_recent_feed
	{
		public string description { get; set; }
		public string feed_url { get; set; }
		public string title { get; set; }
		public List<xsn_recent_feed_item> items { get; set; }
		public string language { get; set; }
		public string link { get; set; }
		public string id { get; set; }
	}
}
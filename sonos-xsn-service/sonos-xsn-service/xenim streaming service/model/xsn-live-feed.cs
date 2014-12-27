using System;
using System.Collections.Generic;

namespace sonosxsnservice
{
	public class xsn_live_feed_item
	{
		public string begin { get; set; }
		public string description { get; set; }
		public string pubdate { get; set; }
		public string title { get; set; }
		public string author_name { get; set; }
		public string listener { get; set; }
		public string link { get; set; }
		public string unique_id { get; set; }
		public List<string> streams { get; set; }
		public string channel { get; set; }
		public string icon { get; set; }
		public string website { get; set; }
		public string end { get; set; }
		public string current_song { get; set; }

		public xsn_live_feed_item()
		{
			icon = "http://media.streams.xenim.de/show-icons/logo_1.png";
		}
	}

	public class xsn_live_feed
	{
		public string description { get; set; }
		public string feed_url { get; set; }
		public string title { get; set; }
		public List<xsn_live_feed_item> items { get; set; }
		public string language { get; set; }
		public string link { get; set; }
		public string id { get; set; }
	}
}
using System;
using System.Collections.Generic;

namespace relivebot
{
	public class Author
	{
		public string name { get; set; }
		public string web { get; set; }
		public string cover { get; set; }
	}

	public class Upcoming
	{
		public string id { get; set; }
		public string channel { get; set; }
		public string episode { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public Author author { get; set; }
		public string start { get; set; }
		public string end { get; set; }
		public string link { get; set; }
		public string flattr { get; set; }
		public string flattr_user { get; set; }
		public string url { get; set; }
		public string shownotes { get; set; }
	}

	public class Author2
	{
		public string name { get; set; }
		public string web { get; set; }
		public string cover { get; set; }
	}

	public class Stream
	{
		public string hls { get; set; }
		public string mp3 { get; set; }
	}

	public class Recent
	{
		public string id { get; set; }
		public string channel { get; set; }
		public string episode { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public Author2 author { get; set; }
		public string start { get; set; }
		public string end { get; set; }
		public string link { get; set; }
		public string flattr { get; set; }
		public string flattr_user { get; set; }
		public string url { get; set; }
		public string shownotes { get; set; }
		public Stream stream { get; set; }
	}

	public class Feeds
	{
		public List<object> live { get; set; }
		public List<Upcoming> upcoming { get; set; }
		public List<Recent> recent { get; set; }
	}

	public class relivebotFeeds
	{
		public Feeds feeds { get; set; }
	}

}


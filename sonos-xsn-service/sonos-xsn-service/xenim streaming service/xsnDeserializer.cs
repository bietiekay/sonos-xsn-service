using System;
using Newtonsoft.Json;
using System.Net;
using relivebot;

namespace sonosxsnservice
{
	public static class xsnDeserializer
	{
		#region Updaters

		#region Live Feed
		public static xsn_live_feed UpdateLiveFeed(String LiveFeedURL)
		{
			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;
			client.Headers.Add ("user-agent", "sonos-xsn-service; https://github.com/bietiekay/sonos-xsn-service");

			try
			{
				return DeserializeLiveFeed(client.DownloadString(LiveFeedURL));
			}
			catch(Exception e)
			{
				Console.WriteLine("UpdateLiveFeed Exception: "+e.Message);
				return null;
			}
		}
		#endregion

		#region Upcoming Feed
		public static xsn_upcoming_feed UpdateUpcomingFeed(String UpcomingFeedURL)
		{
			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;
			client.Headers.Add ("user-agent", "sonos-xsn-service; https://github.com/bietiekay/sonos-xsn-service");

			try
			{
				return DeserializeUpcomingFeed(client.DownloadString(UpcomingFeedURL));
			}
			catch(Exception e)
			{
				Console.WriteLine("UpdateUpcomingFeed Exception: "+e.Message);
				return null;
			}
		}
		#endregion

		#region Xenim Recent Feed
		public static xsn_recent_feed UpdateRecentFeed(String RecentFeedURL)
		{
			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;
			client.Headers.Add ("user-agent", "sonos-xsn-service; https://github.com/bietiekay/sonos-xsn-service");

			try
			{
				return DeserializeRecentFeed(client.DownloadString(RecentFeedURL));
			}
			catch(Exception e)
			{
				Console.WriteLine("UpdateRecentFeed Exception: "+e.Message);
				return null;
			}
		}
		#endregion

		#region reLiveBot Recent Feed
		public static relivebotFeeds UpdateRecentReLiveBotFeed(String RecentFeedURL)
		{
			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;
			client.Headers.Add ("user-agent", "sonos-xsn-service; https://github.com/bietiekay/sonos-xsn-service");

			try
			{
				return DeserializeRecentReLiveBotFeed(client.DownloadString(RecentFeedURL));
			}
			catch(Exception e)
			{
				Console.WriteLine("UpdateRecentFeed Exception: "+e.Message);
				return null;
			}
		}
		#endregion


		#endregion	

		#region Deserializers
		public static xsn_live_feed DeserializeLiveFeed(String Input)
		{
			return JsonConvert.DeserializeObject<xsn_live_feed> (Input);
		}

		public static xsn_recent_feed DeserializeRecentFeed(String Input)
		{
			return JsonConvert.DeserializeObject<xsn_recent_feed> (Input);
		}

		public static relivebotFeeds DeserializeRecentReLiveBotFeed(String Input)
		{
			return JsonConvert.DeserializeObject<relivebotFeeds> (Input);
		}

		public static xsn_upcoming_feed DeserializeUpcomingFeed(String Input)
		{
			return JsonConvert.DeserializeObject<xsn_upcoming_feed> (Input);
		}
		#endregion
	}
}


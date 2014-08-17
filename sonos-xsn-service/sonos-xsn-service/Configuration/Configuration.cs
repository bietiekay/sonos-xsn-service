using System;
using System.IO;
using Newtonsoft.Json;

namespace sonosxsnservice
{
	public class xsnServiceConfiguration
	{
		public int poll_xsn_interval_seconds { get; set; }
		public string xsn_upcoming_feed_url { get; set; }
		public string xsn_recent_feed_url { get; set; }
		public string xsn_live_feed_url { get; set; }
	}

	public class Configuration
	{
		private xsnServiceConfiguration myConfiguration;

		public Configuration (String ConfigurationFileName)
		{
			if (File.Exists (ConfigurationFileName)) {

				String input = File.ReadAllText (ConfigurationFileName);
				myConfiguration = JsonConvert.DeserializeObject<xsnServiceConfiguration>(input);			
			}
			else
			{
				Console.WriteLine ("Error: " + ConfigurationFileName + " not found!");
				throw new Exception("Configuration file not found");
			}
		}

		#region Configuration Access
		public Int32 GetPollingInterval()
		{
			return myConfiguration.poll_xsn_interval_seconds;
		}

		public String GetUpcomingFeedURL()
		{
			return myConfiguration.xsn_upcoming_feed_url;
		}

		public String GetRecentFeedURL()
		{
			return myConfiguration.xsn_recent_feed_url;
		}

		public String GetLiveFeedURL()
		{
			return myConfiguration.xsn_live_feed_url;
		}
		#endregion
	}
}


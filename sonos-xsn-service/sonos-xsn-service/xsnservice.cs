using System;
using System.Collections.Generic;
using System.Threading;

namespace sonosxsnservice
{
	public class xsnservice
	{
		private Configuration myConfiguration;
		private bool shutdown;
		private xsn_live_feed CurrentLiveFeed;
		private xsn_recent_feed CurrentRecentFeed;
		private xsn_upcoming_feed CurrentUpcomingFeed;

		public xsnservice (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
			shutdown = false;
			CurrentLiveFeed = null;
			CurrentRecentFeed = null;
			CurrentUpcomingFeed = null;
		}

		public void Shutdown()
		{
			shutdown = true;
		}
			
		#region Thread Method
		public void Run()
		{
			while (!shutdown) {

				try{
					// update and act...

					// Live Feed
					CurrentLiveFeed = xsnDeserializer.UpdateLiveFeed(myConfiguration.GetLiveFeedURL());
					if (CurrentLiveFeed != null)
						Console.WriteLine("Updated xsn Live Feed - "+CurrentLiveFeed.items.Count+" streams online");

					// Upcoming Feed
					CurrentUpcomingFeed = xsnDeserializer.UpdateUpcomingFeed(myConfiguration.GetUpcomingFeedURL());
					if (CurrentUpcomingFeed != null)
						Console.WriteLine("Updated xsn Upcoming Feed - "+CurrentUpcomingFeed.items.Count+" streams upcoming");

					// Recent Feed
					CurrentRecentFeed = xsnDeserializer.UpdateRecentFeed(myConfiguration.GetRecentFeedURL());
					if (CurrentRecentFeed != null)
						Console.WriteLine("Updated xsn Recent Feed - "+CurrentRecentFeed.items.Count+" streams done");
				}
				catch(Exception e)
				{
					// pokemon handling, catching them all, because we want this to run "unlimitedly"
					Console.WriteLine ("Exception: "+e.Message);
				}
				Thread.Sleep (myConfiguration.GetPollingInterval()*1000);
			}
		}
		#endregion
	}
}
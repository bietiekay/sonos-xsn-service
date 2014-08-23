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
		public object locker;	// used to lock read/writes to the data objects above

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

					#region Live Feed
					xsn_live_feed UpdatedCurrentLiveFeed = xsnDeserializer.UpdateLiveFeed(myConfiguration.GetLiveFeedURL());
					if (UpdatedCurrentLiveFeed != null)
					{
						//Console.WriteLine("Updated xsn Live Feed - "+CurrentLiveFeed.items.Count+" streams online");
						lock(locker)
						{
							CurrentLiveFeed = UpdatedCurrentLiveFeed;
						}
					}
					#endregion

					#region Upcoming Feed
					xsn_upcoming_feed UpdatedCurrentUpcomingFeed = xsnDeserializer.UpdateUpcomingFeed(myConfiguration.GetUpcomingFeedURL());
					if (UpdatedCurrentUpcomingFeed != null)
					{
						//Console.WriteLine("Updated xsn Upcoming Feed - "+CurrentUpcomingFeed.items.Count+" streams upcoming");
						lock(locker)
						{
							CurrentUpcomingFeed = UpdatedCurrentUpcomingFeed;
						}
					}
					#endregion

					#region Recent Feed
					xsn_recent_feed UpdatedCurrentRecentFeed = xsnDeserializer.UpdateRecentFeed(myConfiguration.GetRecentFeedURL());
					if (UpdatedCurrentRecentFeed != null)
					{
						//Console.WriteLine("Updated xsn Recent Feed - "+CurrentRecentFeed.items.Count+" streams done");
						lock(locker)
						{
							CurrentRecentFeed = UpdatedCurrentRecentFeed;
						}
					}
					#endregion
				}
				catch(Exception)
				{
					// pokemon handling, catching them all, because we want this to run "unlimitedly"
					//ConsoleOutputLogger.WriteLine ("SMAPIMethods-Exception: "+e.Message);
				}
				Thread.Sleep (myConfiguration.GetPollingInterval()*1000);
			}
		}
		#endregion

		#region Read Methods
		public xsn_live_feed GetCurrentLiveFeed()
		{
			return CurrentLiveFeed;
		}

		public xsn_recent_feed GetCurrentRecentFeed()
		{
			return CurrentRecentFeed;
		}

		public xsn_upcoming_feed GetCurrentUpcomingFeed()
		{
			return CurrentUpcomingFeed;
		}
		#endregion
	}
}
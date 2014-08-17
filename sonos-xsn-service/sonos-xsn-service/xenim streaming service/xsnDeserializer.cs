using System;
using Newtonsoft.Json;
using System.Net;

namespace sonosxsnservice
{
	public static class xsnDeserializer
	{
		public static xsn_live_feed UpdateLiveFeed(String LiveFeedURL)
		{
			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;

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
			
		public static xsn_live_feed DeserializeLiveFeed(String Input)
		{
			return JsonConvert.DeserializeObject<xsn_live_feed> (Input);
		}
	}
}


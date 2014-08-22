using System;
using System.Text;

namespace sonosxsnservice
{
	/// <summary>
	/// this class contains all Sonos Music API methods to be called. 
	/// Each method parses the PostInputData for further information about the actual request by the client and
	/// then outputs through the return value the response XML.
	/// Note: I am not using any fancy schmancy XML serialization here on purpose. It's overly complicated for the 
	/// simple task of outputting well defined xml
	/// </summary>
	public static class SMAPI
	{
		public static String GetLastUpdate (xsnservice xsnService, String PostInputData)
		{
			// this will always output the same response / nothing interesting to see here.
			String Response = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getLastUpdateResponse><ns1:getLastUpdateResult><ns1:catalog>" + DateTime.Now.Minute+"</ns1:catalog><ns1:favorites>0</ns1:favorites><ns1:pollInterval>30</ns1:pollInterval></ns1:getLastUpdateResult></ns1:getLastUpdateResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>";
			return Response;
		}
			
		#region getMetadata
		public static String getMetadata (xsnservice xsnService, String PostInputData)
		{
			// this represents the following directory structure
			// root
			//  +-- Live
			//  +-- Recent
			//  +-- Upcoming

			// Step 1: Filter out which directoy level should be outputted
			/* Example root level request:
			 * 
			 * <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
			 * 	<s:Header>
			 * 		<credentials xmlns="http://www.sonos.com/Services/1.1">
			 * 			<deviceId>xx-xx-xx-xx-xx-xx:x</deviceId>
			 * 			<deviceProvider>Sonos</deviceProvider>
			 * 		</credentials>
			 * 	</s:Header>
			 * 	<s:Body>
			 * 		<getMetadata xmlns="http://www.sonos.com/Services/1.1">
			 * 			<id>root</id>
			 * 			<index>0</index>
			 * 			<count>100</count>
			 * 		</getMetadata>
			 * 	</s:Body>
			 * </s:Envelope>
			 */

			// get the Directory
			String Directory = PostInputData.Remove (0, PostInputData.IndexOf ("<id>") + 4);
			Directory = Directory.Substring(0,Directory.IndexOf ("</id>"));

			String sIndex = PostInputData.Remove (0, PostInputData.IndexOf ("<index>") + 7);
			sIndex = sIndex.Substring (0, sIndex.IndexOf ("</index>"));

			String sCount = PostInputData.Remove (0, PostInputData.IndexOf ("<count>") + 7);
			sCount = sCount.Substring (0, sCount.IndexOf ("</count>"));

			Int32 Index = Int32.Parse (sIndex);
			Int32 Count = Int32.Parse (sCount);

			// this is where the actual output of this call is going to be stored in...
			String Output = "";

			if (Directory.ToUpper ().StartsWith ("LIVESTREAM")) {
				ConsoleOutputLogger.WriteLine ("Livestream Directory");
				Output = generateLivestreamDirectory (xsnService, Directory);
			
			} else {
				switch (Directory.ToUpper ()) {
				case "ROOT":
					ConsoleOutputLogger.WriteLine ("Root Directory");
					Output = generateRootDirectory (xsnService);
				//Output = TestGenerator ();
					break;
				case "LIVE":
					ConsoleOutputLogger.WriteLine ("Live Directory");
					Output = generateLiveDirectory (xsnService);
					break;
				case "RECENT":
					ConsoleOutputLogger.WriteLine ("Recent Directory");
					Output = generateRecentDirectory (xsnService);
					break;
				case "UPCOMING":
					ConsoleOutputLogger.WriteLine ("Upcoming Directory");
					Output = generateUpcomingDirectory (xsnService);
					break;
				default:
					break;
				}
			}
			return Output;
		}

		#region Directory Methods

		/// <summary>
		/// this will generate the root directory listing
		/// </summary>
		/// <returns>The root directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateRootDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();

			Output.Append (@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""http://www.sonos.com/Services/1.1""><soap:Body><ns1:getMetadataResponse><ns1:getMetadataResult><ns1:index>0</ns1:index><ns1:count>3</ns1:count><ns1:total>3</ns1:total>");

			// Live Directory
			Output.Append (@"<ns1:mediaCollection><ns1:id>LIVE</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
			Output.Append ("Live Shows (" + xsnService.GetCurrentLiveFeed ().items.Count + ")");
			Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

			// Recent Directory
			Output.Append (@"<ns1:mediaCollection><ns1:id>RECENT</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
			Output.Append ("Recent Shows (" + xsnService.GetCurrentRecentFeed ().items.Count + ")");
			Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

			// Upcoming Directory
			Output.Append (@"<ns1:mediaCollection><ns1:id>UPCOMING</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
			Output.Append ("Upcoming Shows (" + xsnService.GetCurrentUpcomingFeed ().items.Count + ")");
			Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

			Output.Append (@"</ns1:getMetadataResult></ns1:getMetadataResponse></soap:Body></soap:Envelope>");

			return Output.ToString();
		}

		/// <summary>
		/// this will generate the live directory listing
		/// </summary>
		/// <returns>The live directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateLivestreamDirectory(xsnservice xsnService, String Livestream)
		{
			StringBuilder Output = new StringBuilder ();
			StringBuilder Elements = new StringBuilder ();
			Int32 Counter = 0;
			Int32 StreamCounter = 0;

			if (Livestream.IndexOf (':') > 0) {
				Livestream = Livestream.Remove (0, Livestream.IndexOf (':') + 1);
			} else
				return null;

			foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items)
			{
				if (Item.unique_id == Livestream) {
					if (Item.title != null) {
						foreach (String stream in Item.streams) {
							Counter++;

							Elements.Append ("<ns1:mediaMetadata><ns1:id>LIVE:" + Item.unique_id + ":"+StreamCounter+"</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + stream + "</ns1:title>");
							Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + Item.author_name + "</ns1:artistId><ns1:artist />");
							Elements.Append ("<ns1:albumId>ALBUM:" + Item.unique_id + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
							Elements.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata>");
							Elements.Append ("</ns1:mediaMetadata>");
							StreamCounter++;
						}
					}
				}
			}

			Output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMetadataResponse><ns1:getMetadataResult>");
			Output.Append ("<ns1:index>0</ns1:index>");
			Output.Append ("<ns1:count>" + Counter+ "</ns1:count>");
			Output.Append ("<ns1:total>" + Counter + "</ns1:total>");

			Output.Append (Elements.ToString ());

			Output.Append ("</ns1:getMetadataResult></ns1:getMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");

			return Output.ToString();
		}

		/// <summary>
		/// this will generate the directory listing with all live streams
		/// </summary>
		/// <returns>The live directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateLiveDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();
			StringBuilder Elements = new StringBuilder ();
			Int32 Counter = 0;

			foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items)
			{
				if (Item.title != null) {
					Counter++;
//					Elements.Append ("<ns1:mediaMetadata><ns1:id>LIVE:" + Item.unique_id + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + Item.title + "</ns1:title>");
//					Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + Item.author_name + "</ns1:artistId><ns1:artist />");
//					Elements.Append ("<ns1:albumId>ALBUM:" + Item.unique_id + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
//					Elements.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata>");
//					Elements.Append ("</ns1:mediaMetadata>");

					Elements.Append (@"<ns1:mediaCollection><ns1:id>LIVESTREAM:"+Item.unique_id+"</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
					Elements.Append (Item.title);
					Elements.Append (@"</ns1:title>");
					//Elements.Append (@"<ns1:albumartUri>" + Item.icon + "</ns1:albumartUri");
					Elements.Append ("<ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");
				}
			}

			Output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMetadataResponse><ns1:getMetadataResult>");
			Output.Append ("<ns1:index>0</ns1:index>");
			Output.Append ("<ns1:count>" + Counter+ "</ns1:count>");
			Output.Append ("<ns1:total>" + Counter + "</ns1:total>");

			Output.Append (Elements.ToString ());

			Output.Append ("</ns1:getMetadataResult></ns1:getMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");

			Console.WriteLine (Output.ToString ());
			return Output.ToString();
		}

		/// <summary>
		/// this will generate the recent directory listing
		/// </summary>
		/// <returns>The recent directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateRecentDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();
			StringBuilder Elements = new StringBuilder ();
			Int32 Counter = 0;

			foreach (xsn_recent_feed_item Item in xsnService.GetCurrentRecentFeed().items)
			{
				if (Item.title != null) {
					Counter++;
					Elements.Append ("<ns1:mediaMetadata><ns1:id>RECENT:" + Item.unique_id + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + Item.title + "</ns1:title>");
					Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + Item.author_name + "</ns1:artistId><ns1:artist />");
					Elements.Append ("<ns1:albumId>ALBUM:" + Item.unique_id + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
					Elements.Append ("<ns1:canPlay>true</ns1:canPlay></ns1:trackMetadata>");
					Elements.Append ("</ns1:mediaMetadata>");
				}
			}

			Output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMetadataResponse><ns1:getMetadataResult>");
			Output.Append ("<ns1:index>0</ns1:index>");
			Output.Append ("<ns1:count>" + Counter+ "</ns1:count>");
			Output.Append ("<ns1:total>" + Counter + "</ns1:total>");

			Output.Append (Elements.ToString ());

			Output.Append ("</ns1:getMetadataResult></ns1:getMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");

			return Output.ToString();
		}

		/// <summary>
		/// this will generate the upcoming directory listing
		/// </summary>
		/// <returns>The upcoming directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateUpcomingDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();
			StringBuilder Elements = new StringBuilder ();
			Int32 Counter = 0;

			foreach (xsn_upcoming_feed_item Item in xsnService.GetCurrentUpcomingFeed().items)
			{
				if (Item.title != null) {
					Counter++;
					Elements.Append ("<ns1:mediaMetadata><ns1:id>UPCOMING:" + Item.unique_id + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + Item.title + "</ns1:title>");
					Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + Item.author_name + "</ns1:artistId><ns1:artist />");
					Elements.Append ("<ns1:albumId>ALBUM:" + Item.unique_id + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
					Elements.Append ("<ns1:canPlay>true</ns1:canPlay></ns1:trackMetadata>");
					Elements.Append ("</ns1:mediaMetadata>");
				}
			}

			Output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMetadataResponse><ns1:getMetadataResult>");
			Output.Append ("<ns1:index>0</ns1:index>");
			Output.Append ("<ns1:count>" + Counter+ "</ns1:count>");
			Output.Append ("<ns1:total>" + Counter + "</ns1:total>");

			Output.Append (Elements.ToString ());

			Output.Append ("</ns1:getMetadataResult></ns1:getMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");

			return Output.ToString();
		}
		#endregion

		#endregion

		public static String getMediaMetadata (xsnservice xsnService, String PostInputData)
		{
			// debugging
			//ConsoleOutputLogger.WriteLine ("getMediaMetadata PostInput: " + PostInputData);

			StringBuilder Output = new StringBuilder ();
			// get the Directory
			String ItemID = PostInputData.Remove (0, PostInputData.IndexOf ("<id>") + 4);
			ItemID = ItemID.Substring(0,ItemID.IndexOf ("</id>"));
			String ItemType;
			String StreamNumber = "0";
			if (ItemID.IndexOf (':') > 0) {
				ItemType = ItemID.Remove (ItemID.IndexOf (':'));
				ItemID = ItemID.Remove (0, ItemID.IndexOf (':') + 1);

				// now get the stream number...
				if (ItemID.IndexOf (':') > 0) {
					ItemID = ItemID.Remove (ItemID.IndexOf (':'));
					StreamNumber = ItemID.Remove (0, ItemID.IndexOf (':') + 1);
				}
			} else
				return null;

			// find that item

			if (ItemType == "LIVE") {

				// this is for the live feed mediafiles
				foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items)
				{
					if (Item.unique_id == ItemID)
					{
						// found it, now find the right stream...
						Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body>");
						Output.Append ("<ns1:getMediaMetadataResponse><ns1:getMediaMetadataResult>");

						Output.Append ("<ns1:id>LIVE:" + Item.unique_id + ":" + StreamNumber + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + Item.title + "</ns1:title><ns1:mimeType>audio/mpeg3</ns1:mimeType>");
						Output.Append ("<ns1:trackMetadata><ns1:artistId>" + Item.author_name + "</ns1:artistId><ns1:artist /><ns1:albumId>" + Item.channel + "</ns1:albumId>");
						Output.Append ("<ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
						Output.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata><ns1:dynamic><ns1:property><ns1:name>isStarred</ns1:name><ns1:value>5</ns1:value>");
						Output.Append ("</ns1:property><ns1:property><ns1:name>isRead</ns1:name><ns1:value>true</ns1:value></ns1:property></ns1:dynamic></ns1:getMediaMetadataResult></ns1:getMediaMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
					}
				}
				return Output.ToString();
			}

//			if (ItemType == "RECENT") {
//
//				// this is for the live feed mediafiles
//				foreach (xsn_recent_feed_item Item in xsnService.GetCurrentRecentFeed().items)
//				{
//					if (Item.unique_id == ItemID)
//					{
//						// found it!
//
//						Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body>");
//						Output.Append ("<ns1:getMediaMetadataResponse><ns1:getMediaMetadataResult>");
//
//						Output.Append ("<ns1:id>RECENT:" + Item.unique_id + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + Item.title + "</ns1:title><ns1:mimeType>audio/mpeg3</ns1:mimeType>");
//						Output.Append ("<ns1:trackMetadata><ns1:artistId>" + Item.author_name + "</ns1:artistId><ns1:artist /><ns1:albumId>" +  Item.unique_id + "</ns1:albumId>");
//						Output.Append ("<ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
//						Output.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata><ns1:dynamic><ns1:property><ns1:name>isStarred</ns1:name><ns1:value>5</ns1:value>");
//						Output.Append ("</ns1:property><ns1:property><ns1:name>isRead</ns1:name><ns1:value>true</ns1:value></ns1:property></ns1:dynamic></ns1:getMediaMetadataResult></ns1:getMediaMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
//					}
//				}
//				return Output.ToString();
//			}

			return "";
		}

		public static String getMediaURI (xsnservice xsnService, String PostInputData)
		{
			StringBuilder Output = new StringBuilder ();
			// get the Directory
			String ItemID = PostInputData.Remove (0, PostInputData.IndexOf ("<id>") + 4);
			ItemID = ItemID.Substring(0,ItemID.IndexOf ("</id>"));

			String ItemType = ItemID.Remove (ItemID.IndexOf (':'));
			ItemID = ItemID.Remove (0, ItemID.IndexOf (':')+1);
			// find that item
			Int32 StreamNumber = 0;

			// now get the stream number...
			if (ItemID.IndexOf (':') > 0) {
				StreamNumber = Convert.ToInt32(ItemID.Remove (0, ItemID.IndexOf (':') + 1));
				ItemID = ItemID.Remove (ItemID.IndexOf (':'));
			}

			if (ItemType == "LIVE") {
				foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items) {
					if (Item.unique_id == ItemID) {
						// found it!

						Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMediaURIResponse><ns1:getMediaURIResult>" + Item.streams [StreamNumber] + "</ns1:getMediaURIResult></ns1:getMediaURIResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
					}
				}

				return Output.ToString ();
			}

//			if (ItemType == "RECENT") {
//				foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items) {
//					if (Item.unique_id == ItemID) {
//						// found it!
//
//						Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMediaURIResponse><ns1:getMediaURIResult>" + Item.streams [0] + "</ns1:getMediaURIResult></ns1:getMediaURIResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
//					}
//				}
//
//				return Output.ToString ();
//			}
//

			return "";
		}
	}
}


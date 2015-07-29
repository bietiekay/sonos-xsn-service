using System;
using System.Text;
using System.Xml;
using relivebot;

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
		#region Helper
		private static String ShortenURLTotheEssence(String URL)
		{
			Uri baseUri = new Uri(URL);

			return baseUri.AbsolutePath;
		}
		private static String EncodeXMLString(string value)
		{
			System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings 
			{
				ConformanceLevel = System.Xml.ConformanceLevel.Fragment
			};

			StringBuilder builder = new StringBuilder();

			using (var writer = System.Xml.XmlWriter.Create(builder, settings))
			{
				writer.WriteString(value);
			}

			return builder.ToString();
		}
		#endregion


		#region GetLastUpdate
		public static String GetLastUpdate (xsnservice xsnService, String PostInputData)
		{
			// this will always output the same response / nothing interesting to see here.
			String Response = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getLastUpdateResponse><ns1:getLastUpdateResult><ns1:catalog>" + DateTime.Now.Minute+"</ns1:catalog><ns1:favorites>0</ns1:favorites><ns1:pollInterval>30</ns1:pollInterval></ns1:getLastUpdateResult></ns1:getLastUpdateResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>";
			return Response;
		}
		#endregion

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

			// this is where the actual output of this call is going to be stored in...
			String Output = "";

			if (Directory.ToUpper ().StartsWith ("LIVESTREAM")) {
				//ConsoleOutputLogger.WriteLine ("Livestream Directory");
				Output = generateLivestreamDirectory (xsnService, Directory);
			
			} else {
				switch (Directory.ToUpper ()) {
				case "ROOT":
					//ConsoleOutputLogger.WriteLine ("Root Directory");
					Output = generateRootDirectory (xsnService);
					break;
				case "LIVE":
					//ConsoleOutputLogger.WriteLine ("Live Directory");
					Output = generateLiveDirectory (xsnService);
					break;
				case "RECENT":
					//ConsoleOutputLogger.WriteLine ("Recent Directory");
					Output = generateRecentDirectory (xsnService);
					break;
				case "UPCOMING":
					//ConsoleOutputLogger.WriteLine ("Upcoming Directory");
					Output = generateUpcomingDirectory (xsnService);
					break;
				default:
					break;
				}
			}
			return Output;
		}
		#endregion

		#region Directory Methods used by getMetaData

		#region generateRootDirectory
		/// <summary>
		/// this will generate the root directory listing with Live, Recent and Upcoming Folders
		/// </summary>
		/// <returns>The root directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateRootDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();

			Output.Append (@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""http://www.sonos.com/Services/1.1""><soap:Body><ns1:getMetadataResponse><ns1:getMetadataResult><ns1:index>0</ns1:index><ns1:count>3</ns1:count><ns1:total>3</ns1:total>");

			lock(xsnService.locker)	{

				// Live Directory
				Output.Append (@"<ns1:mediaCollection><ns1:id>LIVE</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
				Output.Append ("Live Shows (" + xsnService.GetCurrentLiveFeed ().items.Count + ")");
				Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

				// Recent Directory
				Output.Append (@"<ns1:mediaCollection><ns1:id>RECENT</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
				Output.Append ("Recent Shows (" + xsnService.GetCurrentRecentReLiveBotFeed ().feeds.recent.Count + ")");
				Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

				// Upcoming Directory
				Output.Append (@"<ns1:mediaCollection><ns1:id>UPCOMING</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
				Output.Append ("Upcoming Shows (" + xsnService.GetCurrentUpcomingFeed ().items.Count + ")");
				Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

				Output.Append (@"</ns1:getMetadataResult></ns1:getMetadataResponse></soap:Body></soap:Envelope>");
			}
			return Output.ToString();
		}
		#endregion

		#region generateLiveDirectory
		/// <summary>
		/// this will generate the directory listing with all live stream folders, which contain in the next step the actual streams available
		/// </summary>
		/// <returns>The live directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		private static String generateLiveDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();
			StringBuilder Elements = new StringBuilder ();
			Int32 Counter = 0;

			lock (xsnService.locker) {

				foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items) {
					// TODO: add here the LIVE stream directly when there is just one stream available for a live-show

					if (Item.streams != null)
					{
						if (Item.streams.Count == 1)
						{
							// this is when the live show item only has one stream
							Elements.Append ("<ns1:mediaMetadata><ns1:id>LIVE:" + EncodeXMLString(Item.unique_id) + ":0</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(Item.title) + "</ns1:title>");
							Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author_name) + "</ns1:artistId><ns1:artist />");
							Elements.Append ("<ns1:albumId>ALBUM:" + EncodeXMLString(Item.unique_id) + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
							Elements.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata>");
							Elements.Append ("</ns1:mediaMetadata>");
							Counter++;
						}
						else
						{
							// this is when the live show item has more than one stream...
							if (Item.title != null) {
								Counter++;

								Elements.Append (@"<ns1:mediaCollection><ns1:id>LIVESTREAM:" + EncodeXMLString(Item.unique_id) + "</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
								Elements.Append (EncodeXMLString(Item.title));
								Elements.Append (@"</ns1:title>");
								Elements.Append ("<ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");
							}
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
		#endregion

		#region generateLivestreamDirectory
		/// <summary>
		/// this will generate the directory listing with the list of all available streams for a certain livestream
		/// </summary>
		/// <returns>The live directory xml</returns>
		/// <param name="xsnService">Xsn service.</param>
		/// <param name="Livestream">this contains the livestream unique id</param>
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

			lock(xsnService.locker){

				foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items)
				{
					if (Item.unique_id == Livestream) {
						if (Item.title != null) {
							foreach (String stream in Item.streams) {
								Counter++;
								//Uri baseUri = new Uri(stream);
								Elements.Append ("<ns1:mediaMetadata><ns1:id>LIVE:" + EncodeXMLString(Item.unique_id) + ":"+StreamCounter+"</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(ShortenURLTotheEssence(stream)) + "</ns1:title>");
								Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author_name) + "</ns1:artistId><ns1:artist />");
								Elements.Append ("<ns1:albumId>ALBUM:" + EncodeXMLString (Item.unique_id) + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
								Elements.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata>");
								Elements.Append ("</ns1:mediaMetadata>");
								StreamCounter++;
							}
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
		#endregion

		#region generateRecentDirectory
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

			lock(xsnService.locker) {

				foreach (relivebot.Recent Item in xsnService.GetCurrentRecentReLiveBotFeed ().feeds.recent)
				{
					if (Item.stream != null) {
						Counter++;
						Elements.Append ("<ns1:mediaMetadata><ns1:id>RECENT:" + EncodeXMLString(Item.id) + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(Item.title) + "</ns1:title>");
						Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author.name) + "</ns1:artistId><ns1:artist />");
						Elements.Append ("<ns1:albumId>ALBUM:" + EncodeXMLString(Item.id) + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.author.cover + "</ns1:albumArtURI>");
						Elements.Append ("<ns1:canPlay>true</ns1:canPlay></ns1:trackMetadata>");
						Elements.Append ("</ns1:mediaMetadata>");
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

		/// here comes the xenim recent directory generator
		/*private static String generateRecentDirectory(xsnservice xsnService)
		{
			StringBuilder Output = new StringBuilder ();
			StringBuilder Elements = new StringBuilder ();
			Int32 Counter = 0;

			lock(xsnService.locker) {

				foreach (xsn_recent_feed_item Item in xsnService.GetCurrentRecentFeed().items)
				{
					if (Item.title != null) {
						Counter++;
						Elements.Append ("<ns1:mediaMetadata><ns1:id>RECENT:" + EncodeXMLString(Item.unique_id) + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(Item.title) + "</ns1:title>");
						Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author_name) + "</ns1:artistId><ns1:artist />");
						Elements.Append ("<ns1:albumId>ALBUM:" + EncodeXMLString(Item.unique_id) + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
						Elements.Append ("<ns1:canPlay>true</ns1:canPlay></ns1:trackMetadata>");
						Elements.Append ("</ns1:mediaMetadata>");
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
		}*/
		#endregion

		#region generateUpcomingDirectory
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

			lock(xsnService.locker) {
	
				foreach (xsn_upcoming_feed_item Item in xsnService.GetCurrentUpcomingFeed().items)
				{
					if (Item.title != null) {
						Counter++;
						Elements.Append ("<ns1:mediaMetadata><ns1:id>UPCOMING:" + EncodeXMLString(Item.unique_id) + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(Item.title) + "</ns1:title>");
						Elements.Append ("<ns1:mimeType>audio/mpeg3</ns1:mimeType><ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author_name) + "</ns1:artistId><ns1:artist />");
						Elements.Append ("<ns1:albumId>ALBUM:" + EncodeXMLString(Item.unique_id) + "</ns1:albumId><ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
						Elements.Append ("<ns1:canPlay>true</ns1:canPlay></ns1:trackMetadata>");
						Elements.Append ("</ns1:mediaMetadata>");
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
		#endregion

		#endregion

		#region getMediaMetadata
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

				lock(xsnService.locker) {

					// this is for the live feed mediafiles
					foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items)
					{
						if (Item.unique_id == ItemID)
						{
							// found it, now find the right stream...
							Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body>");
							Output.Append ("<ns1:getMediaMetadataResponse><ns1:getMediaMetadataResult>");

							Output.Append ("<ns1:id>LIVE:" + EncodeXMLString(Item.unique_id) + ":" + StreamNumber + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(Item.title) + "</ns1:title><ns1:mimeType>audio/mpeg3</ns1:mimeType>");
							Output.Append ("<ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author_name) + "</ns1:artistId><ns1:artist /><ns1:albumId>" + EncodeXMLString(Item.channel) + "</ns1:albumId>");
							Output.Append ("<ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.icon + "</ns1:albumArtURI>");
							Output.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata><ns1:dynamic><ns1:property><ns1:name>isStarred</ns1:name><ns1:value>5</ns1:value>");
							Output.Append ("</ns1:property><ns1:property><ns1:name>isRead</ns1:name><ns1:value>true</ns1:value></ns1:property></ns1:dynamic></ns1:getMediaMetadataResult></ns1:getMediaMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
						}
					}
				}
				return Output.ToString();
			}

			if (ItemType == "RECENT") {

				lock(xsnService.locker) {

					// this is for the live feed mediafiles
					foreach (relivebot.Recent Item in xsnService.GetCurrentRecentReLiveBotFeed ().feeds.recent)
					{
						if (Item.id == ItemID)
						{
							// found it, now find the right stream...
							Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body>");
							Output.Append ("<ns1:getMediaMetadataResponse><ns1:getMediaMetadataResult>");

							Output.Append ("<ns1:id>RECENT:" + EncodeXMLString(Item.id) + ":" + StreamNumber + "</ns1:id><ns1:itemType>stream</ns1:itemType><ns1:title>" + EncodeXMLString(Item.title) + "</ns1:title><ns1:mimeType>audio/mpeg3</ns1:mimeType>");
							Output.Append ("<ns1:trackMetadata><ns1:artistId>" + EncodeXMLString(Item.author.name) + "</ns1:artistId><ns1:artist /><ns1:albumId>" + EncodeXMLString(Item.channel) + "</ns1:albumId>");
							Output.Append ("<ns1:album></ns1:album><ns1:duration>0</ns1:duration><ns1:rating>5</ns1:rating><ns1:albumArtURI>" + Item.author.cover + "</ns1:albumArtURI>");
							Output.Append ("<ns1:canPlay>true</ns1:canPlay><ns1:canSkip>true</ns1:canSkip></ns1:trackMetadata><ns1:dynamic><ns1:property><ns1:name>isStarred</ns1:name><ns1:value>5</ns1:value>");
							Output.Append ("</ns1:property><ns1:property><ns1:name>isRead</ns1:name><ns1:value>true</ns1:value></ns1:property></ns1:dynamic></ns1:getMediaMetadataResult></ns1:getMediaMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
						}
					}
				}
				return Output.ToString();
			}

			return "";
		}
		#endregion

		#region getMediaURI
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

			lock(xsnService.locker) {

				if (ItemType == "LIVE") {
					foreach (xsn_live_feed_item Item in xsnService.GetCurrentLiveFeed().items) {
						if (Item.unique_id == ItemID) {
							// found it!

							Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMediaURIResponse><ns1:getMediaURIResult>" + Item.streams [StreamNumber] + "</ns1:getMediaURIResult></ns1:getMediaURIResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
						}
					}

					return Output.ToString ();
				}

				if (ItemType == "RECENT") {
					foreach (relivebot.Recent Item in xsnService.GetCurrentRecentReLiveBotFeed ().feeds.recent) 
					{
						if (Item.id == ItemID) {
							// found it!

							Output.Append ("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getMediaURIResponse><ns1:getMediaURIResult>" + Item.stream.mp3 + "</ns1:getMediaURIResult></ns1:getMediaURIResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
						}
					}

					return Output.ToString ();
				}

			}

			return "";
		}
		#endregion
	}
}
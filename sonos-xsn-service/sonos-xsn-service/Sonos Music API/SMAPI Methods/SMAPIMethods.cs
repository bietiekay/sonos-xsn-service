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
			String Response = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ns1=\"http://www.sonos.com/Services/1.1\"><SOAP-ENV:Body><ns1:getLastUpdateResponse><ns1:getLastUpdateResult><ns1:catalog>0</ns1:catalog><ns1:favorites>0</ns1:favorites><ns1:pollInterval>30</ns1:pollInterval></ns1:getLastUpdateResult></ns1:getLastUpdateResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>";
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

			switch (Directory.ToUpper())
			{
			case "ROOT":
				Console.WriteLine ("Root Directory");
				Output = generateRootDirectory (xsnService);
				break;
			case "LIVE":
				Console.WriteLine("Live Directory");
				break;
			case "RECENT":
				Console.WriteLine ("Recent Directory");
				break;
			case "UPCOMING":
				Console.WriteLine ("Upcoming Directory");
				break;
			default:
				break;
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

			Output.Append (@"<?xml version=""1.0"" encoding=""UTF-8""?><SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""http://www.sonos.com/Services/1.1""><SOAP-ENV:Body><ns1:getMetadataResponse><ns1:getMetadataResult><ns1:index>0</ns1:index><ns1:count>3</ns1:count><ns1:total>3</ns1:total>");

			// Live Directory
			Output.Append (@"<ns1:mediaCollection><ns1:id>LIVE</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
			Output.Append ("Live Shows (" + xsnService.GetCurrentLiveFeed ().items.Count + ")");
			Output.Append (xsnService.GetCurrentLiveFeed ().items.Count);
			Output.Append (@")""</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

			// Recent Directory
			Output.Append (@"<ns1:mediaCollection><ns1:id>RECENT</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
			Output.Append ("Recent Shows (" + xsnService.GetCurrentRecentFeed ().items.Count + ")");
			Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

			// Upcoming Directory
			Output.Append (@"<ns1:mediaCollection><ns1:id>UPCOMING</ns1:id><ns1:itemType>collection</ns1:itemType><ns1:title>");
			Output.Append ("Upcoming Shows (" + xsnService.GetCurrentUpcomingFeed ().items.Count + ")");
			Output.Append (@"</ns1:title><ns1:canPlay>false</ns1:canPlay></ns1:mediaCollection>");

			Output.Append (@"</ns1:getMetadataResult></ns1:getMetadataResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");

			return Output.ToString();
		}

		#endregion

		#endregion

		public static String getMediaMetadata (xsnservice xsnService, String PostInputData)
		{
			throw new NotImplementedException ();
		}

		public static String getMediaURI (xsnservice xsnService, String PostInputData)
		{
			throw new NotImplementedException ();
		}
	}
}


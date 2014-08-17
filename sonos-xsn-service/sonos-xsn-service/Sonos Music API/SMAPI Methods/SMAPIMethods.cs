using System;

namespace sonosxsnservice
{
	/// <summary>
	/// this class contains all Sonos Music API methods to be called. 
	/// Each method parses the PostInputData for further information about the actual request by the client and
	/// then outputs through the return value the response XML.
	/// </summary>
	public static class SMAPI
	{
		public static String GetLastUpdate (xsnservice xsnService, String PostInputData)
		{
			return "....";
		}

		public static String getMetadata (xsnservice xsnService, String PostInputData)
		{
			throw new NotImplementedException ();

		}

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


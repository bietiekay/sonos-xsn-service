using System;
using Newtonsoft.Json;

namespace sonosxsnservice
{
	public static class xsnDeserializer
	{
		public static xsn_live_feed Deserialize(String Input)
		{
			return JsonConvert.DeserializeObject<xsn_live_feed> (Input);
		}
	}
}


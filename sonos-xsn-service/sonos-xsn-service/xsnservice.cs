using System;
using System.Collections.Generic;
using System.Threading;

namespace sonosxsnservice
{
	public class xsnservice
	{
		private Configuration myConfiguration;
		private bool shutdown;

		public xsnservice (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
			shutdown = false;
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
				}
				catch(Exception e)
				{
					// pokemon handling, catching them all, because we want this to run "unlimitedly"
					Console.WriteLine ("Pokemon: "+e.Message);
				}
				Thread.Sleep (myConfiguration.GetPollingInterval()*1000);
			}
		}
		#endregion
	}
}
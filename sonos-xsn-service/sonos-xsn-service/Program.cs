using System;
using System.Threading;
using sonosxsnservice.HTTP;

namespace sonosxsnservice
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("SONOS xenim streaming service integration");
			Console.WriteLine ("(C) Daniel Kirstenpfad 2014 - http://www.technology-ninja.com");
			Console.WriteLine ();

			Configuration myConfiguration = new Configuration ("configuration.json");

			#region Start-Up Main-Event Loop

			#region xenim streaming network updater thread 
			Console.WriteLine("Starting xsn updater...");

			xsnservice _Thread = new xsnservice(myConfiguration);
			Thread xsnServiceThread = new Thread(new ThreadStart(_Thread.Run));

			xsnServiceThread.Start();
			#endregion 

			#region built-in HTTP server
			Console.WriteLine("Starting http server...");
			HttpServer httpServer;

			httpServer = new SonosSMAPIServer(myConfiguration.GetHTTPListeningIP(),myConfiguration.GetHTTPListeningPort(), _Thread);
			Thread thread = new Thread(new ThreadStart(httpServer.listen));
			thread.Start();

			#endregion

			#endregion
		}
	}
}

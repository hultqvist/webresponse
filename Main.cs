using System;
using System.Text.RegularExpressions;
//using System.Globalization;
using System.Collections.Generic;

namespace webresponse
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			string host;
			if(args.Length == 1)
				host = args[0]+".80";
			else if(args.Length == 2)
				host = args[0]+"."+args[1];
			else
			{
				Console.Error.WriteLine(
				    "WebResponse parser, extract web server delays from a tcpdump log.\n"+
					"Copyright Peter Hultqvist 2010, phq@endnode.se, released under the GPLv3 license - free to copy, run, modify and more.\n"+
					"\n"+
					"Usage: mono webresponse.exe [host] <port>\n"+
					"	stdin: log from tcpdump\n"+
					"	stdout: delay in format: timestamp(unixtime) delay(ms)\n"+
					"\n"+
					"Example: Record to log and parse afterwards\n"+
					"	$ tcpdump -tt -S -n \"port 80 and tcp[13] & 8 != 0\" > web.log\n"+
					"	$ cat web.log | mono webresponse.exe 123.123.123.123 > delay.log\n"+
					"\n"+
					"Example: Live output\n"+
					"	$ tcpdump -tt -S -n \"port 80 and tcp[13] & 8 != 0\" | mono webresponse.exe 123.123.123.123\n"+
					"");	
				
				return;
			}
			Console.Error.WriteLine("Matching for host: {0}", host);
			
			string line;
			Regex linetest = new Regex(@"^([^ ]+) IP ([^ ]+) > ([^ ]+): ", RegexOptions.Compiled);

			// 20:07:25.511138 IP 79.102.242.2.47957 > 91.123.195.179.80: P 3784270264:3784270690(426) ack 3623097293 win 267 <nop,nop,timestamp 10893157 169074960>
			
			
			Dictionary<string, DateTime> state = new Dictionary<string, DateTime>();
			
			while((line = Console.ReadLine()) != null)
			{
				try
				{
						
					Match match = linetest.Match(line);
					if( ! match.Success)
					{
						if(line.Trim() == "")
							continue;
						
						Console.Error.WriteLine("Line error: {0}", line);
						continue;
					}
					
					//DateTime time = DateTime.ParseExact(match.Groups[1].Value, "HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
					DateTime time = UnixTime.UnixTimeToDateTime(match.Groups[1].Value);
					string from = match.Groups[2].Value;
					string to = match.Groups[3].Value;
					
					if(to == host)	//New incoming request
						state[from] = time;
					else if(from == host)	//Reply, record time difference
					{
						TimeSpan diff = time - state[to];
						Console.WriteLine("{0} {1}", match.Groups[1].Value, diff.TotalMilliseconds.ToString());
						state.Remove(to);
					}
					else
						Console.Error.WriteLine("Host error: neither from nor to");
				}
				catch(FormatException)
				{
					Console.Error.WriteLine("Line error: {0}", line);
				}
				catch(KeyNotFoundException)
				{
					continue;
				}
			}
			Console.Error.WriteLine("");
		}
	}
}

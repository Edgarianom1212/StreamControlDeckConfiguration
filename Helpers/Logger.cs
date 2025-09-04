using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Helpers
{
	public static class Logger
	{
		private static readonly string LogFile = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory, "app.log");

		public static void Log(string message)
		{
			//dont log for now
			//string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
			//File.AppendAllText(LogFile, line + Environment.NewLine);
		}
	}
}

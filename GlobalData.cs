using Avalonia.Controls;
using StreamDeckConfiguration.Controls;
using StreamDeckConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration
{
	public static class GlobalData
	{

		static GlobalData()
		{
			KeyActionList = new List<KeyAction>()
			{
				new KeyAction("HTTP Request", "mdi-web", new HttpRequest()),
			};

		}

		public static async void ExecuteAction(object keyActionUserControl)
		{
			switch (keyActionUserControl)
			{
				case HttpRequest httpRequest:
					SendHttpRequest(httpRequest.RequestString, httpRequest.RequestBody);
					break;
			}
		}

		private static async void SendHttpRequest(string url, string body)
		{
			var client = new HttpClient();
			var content = new StringContent(body, Encoding.UTF8, "application/json");

			var response = await client.PutAsync(url, content);
			string result = await response.Content.ReadAsStringAsync();
			Debug.WriteLine(result);
		}

		public static List<KeyAction> KeyActionList { get; set; }

	}
}

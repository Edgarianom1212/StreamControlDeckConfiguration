using StreamDeckConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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


		public static List<KeyAction> KeyActionList { get; set; }

	}
}

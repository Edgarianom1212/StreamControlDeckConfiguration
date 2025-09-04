using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Helpers
{
	public class PortManager
	{
		public static SerialPort StreamDeckPort { get; set; }

		public static void ClosePort()
		{
			if (StreamDeckPort != null)
			{
				try
				{
					if (StreamDeckPort.IsOpen)
					{
						StreamDeckPort.Close();
					}
					StreamDeckPort.Dispose();
				}
				catch (Exception ex)
				{
					Logger.Log("Fehler beim Schließen: " + ex.Message);
				}
				finally
				{
					StreamDeckPort = null;
				}
			}
		}

	}
}

using Avalonia.Controls;
using StreamDeckConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Helpers
{
	public static class ActionMapper
	{
		// Control -> persistierbare Daten
		public static ActionData? ToData(Control ctrl) => ctrl switch
		{
			HttpRequest v => new HttpRequestData(v.RequestString, v.RequestBody),
			OpenWebsite v => new OpenWebsiteData(v.URL),
			OpenApplication v => new OpenApplicationData(v.FilePath, v.FileName),
			CloseApplication v => new CloseApplicationData(v.FilePath, v.FileName),
			Text v => new TextPasteData(v.PasteText),
			HotKey v => new HotKeyData(v.ShortcutDisplay),
			_ => null
		};

		// (Optional) Daten -> Control (falls du Controls aus Config wiederherstellen willst)
		public static Control? ToControl(ActionData data) => data switch
		{
			HttpRequestData d => new HttpRequest { RequestString = d.RequestString, RequestBody = d.Body },
			OpenWebsiteData d => new OpenWebsite { URL = d.Url },
			OpenApplicationData d => new OpenApplication { FilePath = d.FilePath, FileName = d.FileName },
			CloseApplicationData d => new CloseApplication { FilePath = d.FilePath, FileName = d.FileName },
			TextPasteData d => new Text { PasteText = d.PasteText },
			HotKeyData d => new HotKey { ShortcutDisplay = d.ShortcutDisplay },
			_ => null
		};
	}
}

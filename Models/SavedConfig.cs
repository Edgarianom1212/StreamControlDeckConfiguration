using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Models
{
	public record KeyConfig
	{
		public int KeyIndex { get; init; }
		public string? Label { get; set; }
		public string? IconName { get; set; }
		public ActionData? Action { get; set; }
	}

	public record AppConfig
	{
		public List<KeyConfig> Keys { get; init; } = new();

		public static AppConfig CreateDefault() => new()
			{
				Keys = Enumerable.Range(0, 12)
					.Select(i => new KeyConfig { KeyIndex = i, Label = $"Key {i + 1}" })
					.ToList()
			};
	}
}

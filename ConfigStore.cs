using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StreamDeckConfiguration
{
	/// <summary>
	/// Generischer JSON-Config-Speicher, der plattformgerecht unter dem Benutzer-App-Datenpfad speichert.
	/// - Windows:   %AppData%\{Company}\{App}\{fileName}
	/// - Linux:     ~/.config/{Company}/{App}/{fileName}
	/// - macOS:     ~/Library/Application Support/{Company}/{App}/{fileName}
	/// </summary>
	public sealed class ConfigStore<T>
		where T : class
	{
		private readonly SemaphoreSlim _mutex = new(1, 1);

		public string CompanyName { get; } = "PrinterBrothers";
		public string AppName { get; } = "StreamControlDeck";
		public string FileName { get; } = "config.json";

		/// <summary>Kompletter Pfad zur Datei (z. B. .../config.json).</summary>
		public string FilePath { get; }
		/// <summary>Ordner, in dem die Config liegt.</summary>
		public string DirectoryPath { get; }

		private readonly JsonSerializerOptions _json;

		public ConfigStore(JsonSerializerOptions? jsonOptions = null)
		{
			DirectoryPath = GetDefaultConfigDirectory(CompanyName, AppName);
			FilePath = Path.Combine(DirectoryPath, FileName);

			_json = jsonOptions ?? CreateDefaultJsonOptions();
		}

		/// <summary>
		/// Lädt die Config. Falls die Datei fehlt oder ungültig ist, wird der Default erzeugt und sofort gespeichert.
		/// </summary>
		public async Task<T> LoadAsync(Func<T> defaultFactory, CancellationToken ct = default)
		{
			ArgumentNullException.ThrowIfNull(defaultFactory);

			await _mutex.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				Directory.CreateDirectory(DirectoryPath);

				if (!File.Exists(FilePath))
				{
					var fresh = defaultFactory();
					await SaveInternalAsync(fresh, ct).ConfigureAwait(false);
					return fresh;
				}

				using var fs = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				var loaded = await JsonSerializer.DeserializeAsync<T>(fs, _json, ct).ConfigureAwait(false);

				if (loaded is null)
				{
					var fallback = defaultFactory();
					await SaveInternalAsync(fallback, ct).ConfigureAwait(false);
					return fallback;
				}

				return loaded;
			}
			catch
			{
				// Bei jedem Fehler auf sicheren Default zurückfallen, damit die App weiterläuft
				var fallback = defaultFactory();
				try { await SaveInternalAsync(fallback, ct).ConfigureAwait(false); } catch { /* ignore */ }
				return fallback;
			}
			finally
			{
				_mutex.Release();
			}
		}

		/// <summary>
		/// Speichert die Config (atomar via Temp-Datei + Replace).
		/// </summary>
		public async Task SaveAsync(T value, CancellationToken ct = default)
		{
			ArgumentNullException.ThrowIfNull(value);

			await _mutex.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				Directory.CreateDirectory(DirectoryPath);
				await SaveInternalAsync(value, ct).ConfigureAwait(false);
			}
			finally
			{
				_mutex.Release();
			}
		}

		/// <summary>
		/// Erstellt eine Datums-gestempelte Sicherungskopie der aktuellen Datei (falls vorhanden).
		/// </summary>
		public async Task<string?> BackupAsync(CancellationToken ct = default)
		{
			await _mutex.WaitAsync(ct).ConfigureAwait(false);
			try
			{
				if (!File.Exists(FilePath)) return null;

				var stamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss");
				var backupName = Path.GetFileNameWithoutExtension(FileName) + $"_{stamp}" + Path.GetExtension(FileName);
				var backupPath = Path.Combine(DirectoryPath, "backups");
				Directory.CreateDirectory(backupPath);
				var fullBackup = Path.Combine(backupPath, backupName);

				File.Copy(FilePath, fullBackup, overwrite: false);
				return fullBackup;
			}
			finally
			{
				_mutex.Release();
			}
		}

		private async Task SaveInternalAsync(T value, CancellationToken ct)
		{
			var tmp = FilePath + ".tmp";

			// In Temp-Datei schreiben
			await using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
			{
				await JsonSerializer.SerializeAsync(fs, value, _json, ct).ConfigureAwait(false);
				await fs.FlushAsync(ct).ConfigureAwait(false);
			}

			// Atomar ersetzen (auf Windows ReplaceFile wäre ideal; Move reicht hier praktikabel)
			if (File.Exists(FilePath))
			{
				// Kleine zusätzliche Sicherheit: alte Datei noch als .bak behalten
				var bak = FilePath + ".bak";
				try
				{
					if (File.Exists(bak)) File.Delete(bak);
					File.Move(FilePath, bak);
				}
				catch { /* optional */ }
			}

			// Temp -> Ziel verschieben
			File.Move(tmp, FilePath, overwrite: true);
		}

		private static JsonSerializerOptions CreateDefaultJsonOptions() =>
			new()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true,
				Converters =
				{
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
				},
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

		private static string GetDefaultConfigDirectory(string company, string app)
		{
			// Basis: ApplicationData – ergibt:
			// Windows: %AppData%
			// Linux:   ~/.config
			// macOS:   ~/Library/Application Support
			var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			// Einige Distros/Setups liefern leer → Fallback auf HOME
			if (string.IsNullOrWhiteSpace(basePath))
			{
				var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				basePath = string.IsNullOrWhiteSpace(home) ? "." : home;

				// Linux-typischer Fallback
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
					!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					basePath = Path.Combine(basePath, ".config");
				}
			}

			return Path.Combine(basePath, company, app);
		}
	}
}

/* ========================== Verwendung (Beispiel) ==========================
 * Angenommen, du hast ein Modell für 12 Tasten:
 *
 * public record KeyConfig
 * {
 *     public int KeyIndex { get; init; }
 *     public string? Label { get; init; }
 *     public string? IconName { get; init; }
 *     public string? ActionType { get; init; } // z.B. "Hotkey", "AppStart", "MQTT", ...
 *     public string? ActionPayload { get; init; }
 * }
 *
 * public record AppConfig
 * {
 *     public List<KeyConfig> Keys { get; init; } = Enumerable.Range(0, 12)
 *         .Select(i => new KeyConfig { KeyIndex = i, Label = $"Key {i+1}" }).ToList();
 * }
 *
 * Initialisieren (z. B. in App.Startup o. ä.):
 *
 * var store = new ConfigStore<AppConfig>("Edgariano", "StreamDeckConfiguration");
 * AppConfig cfg = await store.LoadAsync(() => new AppConfig());
 *
 * // ... Änderungen in der UI ...
 * await store.SaveAsync(cfg);
 *
 * // Optional: Backup anlegen
 * var backupPath = await store.BackupAsync();
 * ========================================================================== */

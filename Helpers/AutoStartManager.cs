// File: Platform/Windows/AutoStartManager.cs
// Target Framework: net8.0-windows
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace StreamDeckConfiguration.Helpers;

/// <summary>
/// Helper class to manage Windows autostart (Run registry key).
/// Creates, removes and checks an entry under
/// HKCU\Software\Microsoft\Windows\CurrentVersion\Run
/// so that the application starts automatically when the user logs in.
/// </summary>
public static class AutoStartManager
{
	private const string RunValueName = "StreamDeckConfiguration";

	private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

	public static void Enable(string? args = null)
	{
		var exePath = GetExecutablePath();

		var command = $"\"{exePath}\"{(string.IsNullOrWhiteSpace(args) ? "" : " " + args)}";

		using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
					  ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
					  ?? throw new InvalidOperationException("Cannot open or create Run key.");

		key.SetValue(RunValueName, command);
	}

	public static void Disable()
	{
		using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
		key?.DeleteValue(RunValueName, throwOnMissingValue: false);
	}

	public static bool IsEnabled(string? args = null)
	{
		using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
		var value = key?.GetValue(RunValueName) as string;
		if (string.IsNullOrWhiteSpace(value)) return false;

		var exePath = GetExecutablePath();
		var expected = $"\"{exePath}\"{(string.IsNullOrWhiteSpace(args) ? "" : " " + args)}";

		return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
	}

	private static string GetExecutablePath()
	{
		var path = Environment.ProcessPath
				   ?? Process.GetCurrentProcess().MainModule?.FileName
				   ?? throw new InvalidOperationException("Cannot determine executable path.");

		return Path.GetFullPath(path);
	}
}

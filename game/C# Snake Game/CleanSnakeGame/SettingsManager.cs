using System;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace CleanSnakeGame
{
    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CleanSnakeGame",
            "settings.json"
        );

        public static GameSettings Settings { get; private set; } = new GameSettings();

        static SettingsManager()
        {
            LoadSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                // Handle save error silently
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    Settings = JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
                }
                else
                {
                    Settings = new GameSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                // Handle load error silently, use defaults
                Settings = new GameSettings();
                Console.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }
    }

    public class GameSettings
    {
        public string Difficulty { get; set; } = "Medium";
        public string PlayerName { get; set; } = "Player";
        public int SnakeColorIndex { get; set; } = 0;
        public bool PowerupsEnabled { get; set; } = true;
        public bool ObstaclesEnabled { get; set; } = false;
        public bool SoundEnabled { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
        public bool Fullscreen { get; set; } = false;
        public int BestScore { get; set; } = 0;

        // Helper methods
        public Color GetSnakeColor()
        {
            var colors = new Color[] { Color.Lime, Color.Red, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan };
            return colors[Math.Max(0, Math.Min(SnakeColorIndex, colors.Length - 1))];
        }

        public int GetGameSpeed()
        {
            return Difficulty switch
            {
                "Easy" => 200,
                "Medium" => 150,
                "Hard" => 100,
                _ => 150
            };
        }
    }
}

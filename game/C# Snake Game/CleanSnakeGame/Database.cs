using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace CleanSnakeGame
{
    public static class Database
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CleanSnakeGame");

        private static readonly string DbPath = Path.Combine(AppFolder, "snake.db");
        private static readonly string ConnectionString = $"Data Source={""}".Length == 0 ? "" : $"Data Source={DbPath}";

        public static void Initialize()
        {
            Directory.CreateDirectory(AppFolder);

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS HighScores (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerName TEXT NOT NULL,
                    Score INTEGER NOT NULL,
                    Level INTEGER NOT NULL,
                    PlayedAt TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        public static void AddScore(string playerName, int score, int level, DateTime playedAt)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO HighScores (PlayerName, Score, Level, PlayedAt)
                VALUES ($name, $score, $level, $playedAt);
            ";
            command.Parameters.AddWithValue("$name", playerName ?? "Player");
            command.Parameters.AddWithValue("$score", score);
            command.Parameters.AddWithValue("$level", level);
            command.Parameters.AddWithValue("$playedAt", playedAt.ToString("o")); // ISO 8601
            command.ExecuteNonQuery();
        }

        public static List<HighScore> GetTopScores(int limit = 20)
        {
            var results = new List<HighScore>();

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PlayerName, Score, Level, PlayedAt
                FROM HighScores
                ORDER BY Score DESC, PlayedAt ASC
                LIMIT $limit;
            ";
            command.Parameters.AddWithValue("$limit", Math.Max(1, limit));

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new HighScore
                {
                    PlayerName = reader.GetString(0),
                    Score = reader.GetInt32(1),
                    Level = reader.GetInt32(2),
                    PlayedAt = DateTime.Parse(reader.GetString(3))
                });
            }

            return results;
        }
    }

    public class HighScore
    {
        public string PlayerName { get; set; } = "Player";
        public int Score { get; set; }
        public int Level { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramToDiscordBot
{
    public static class KeywordRepository
    {
        private const string ConnectionString = "Data Source=keywords.db";

        // Метод для добавления ключевого слова
        public static void AddKeyword(string word)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "INSERT INTO Keywords (Word) VALUES (@word)";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@word", word);
            command.ExecuteNonQuery();
        }

        // Метод для удаления ключевого слова
        public static void RemoveKeyword(string word)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "DELETE FROM Keywords WHERE Word = @word";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@word", word);
            command.ExecuteNonQuery();
        }

        // Метод для получения списка всех ключевых слов
        public static List<string> GetKeywords()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "SELECT Word FROM Keywords";
            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            var keywords = new List<string>();
            while (reader.Read())
            {
                keywords.Add(reader.GetString(0));
            }

            return keywords;
        }
    }
}

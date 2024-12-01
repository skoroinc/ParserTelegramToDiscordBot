using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramToDiscordBot
{
    public static class DatabaseInitializer
    {
        private const string ConnectionString = "Data Source=keywords.db"; // Укажите путь к файлу базы данных

        public static void Initialize()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Запрос на создание таблицы для хранения ключевых слов
            string createTableQuery = @"
        CREATE TABLE IF NOT EXISTS Keywords (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Word TEXT NOT NULL UNIQUE
        );";

            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery(); // Выполнение запроса на создание таблицы
        }
    }
}

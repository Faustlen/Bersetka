using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Bersetka.helpers
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = @"Data Source=.\DataBase\Bersetka.db;Version=3;";

        public static DataTable GetPlayers(string searchQuery = "")
        {
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Name, Score, Wins, Losses, Draws FROM Players";
                    if (!string.IsNullOrWhiteSpace(searchQuery))
                        query += " WHERE Name LIKE @SearchQuery";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchQuery))
                            command.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                }
            }
            return dataTable;
        }

        public static List<string> GetPlayerNames()
        {
            List<string> players = new List<string>();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Name FROM Players ORDER BY Name ASC";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                            players.Add(reader["Name"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки списка игроков: {ex.Message}");
                }
            }
            return players;
        }

        public static void AddUserToDatabase(string name)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Players (Name, Score, Wins, Losses, Draws) VALUES (@Name, 800, 0, 0, 0)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
                }
            }
        }

        public static void UpdateUserInDatabase(string oldName, string newName, int score, int wins, int losses, int draws)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Players SET Name = @NewName, Score = @Score, Wins = @Wins, Losses = @Losses, Draws = @Draws WHERE Name = @OldName";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NewName", newName);
                        command.Parameters.AddWithValue("@Score", score);
                        command.Parameters.AddWithValue("@Wins", wins);
                        command.Parameters.AddWithValue("@Losses", losses);
                        command.Parameters.AddWithValue("@Draws", draws);
                        command.Parameters.AddWithValue("@OldName", oldName);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении участника: {ex.Message}");
                }
            }
        }

        public static void DeleteUserFromDatabase(string name)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Players WHERE Name = @Name";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
                }
            }
        }

    }
}

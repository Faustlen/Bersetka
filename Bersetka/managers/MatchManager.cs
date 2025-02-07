using System;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Windows.Media;

namespace Bersetka.managers
{
    public static class MatchManager
    {
        private static readonly string connectionString = @"Data Source=.\DataBase\Bersetka.db;Version=3;";

        public static void RegisterMatch(string leftPlayer, string rightPlayer, string result, TextBlock resultMessage)
        {
            if (string.IsNullOrEmpty(leftPlayer) || string.IsNullOrEmpty(rightPlayer))
            {
                ShowMessage(resultMessage, "Выберите обоих участников!", false);
                return;
            }
            if (leftPlayer == rightPlayer)
            {
                ShowMessage(resultMessage, "Нельзя выбрать одного и того же игрока!", false);
                return;
            }

            int leftRating, rightRating;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT Score FROM Players WHERE Name = @Name", connection))
                {
                    command.Parameters.AddWithValue("@Name", leftPlayer);
                    leftRating = Convert.ToInt32(command.ExecuteScalar());

                    command.Parameters["@Name"].Value = rightPlayer;
                    rightRating = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            int ratingChangeLeft = 0, ratingChangeRight = 0;

            switch (result)
            {
                case "left":
                    ratingChangeLeft = CalculateWinRating(leftRating, rightRating);
                    ratingChangeRight = -ratingChangeLeft;
                    UpdateMatchStats(leftPlayer, rightPlayer, ratingChangeLeft, ratingChangeRight, "left");
                    break;

                case "right":
                    ratingChangeRight = CalculateWinRating(rightRating, leftRating);
                    ratingChangeLeft = -ratingChangeRight;
                    UpdateMatchStats(leftPlayer, rightPlayer, ratingChangeLeft, ratingChangeRight, "right");
                    break;

                case "draw":
                    ratingChangeLeft = CalculateDrawRating(leftRating, rightRating);
                    ratingChangeRight = -ratingChangeLeft;
                    UpdateMatchStats(leftPlayer, rightPlayer, ratingChangeLeft, ratingChangeRight, "draw");
                    break;
            }

            ShowMessage(resultMessage, $"✅ Результат: {leftPlayer} ({ratingChangeLeft:+#;-#;0}) vs {rightPlayer} ({ratingChangeRight:+#;-#;0})", true);
        }

        private static int CalculateWinRating(int winnerRating, int loserRating)
        {
            int baseRating = 20;
            int ratingDifference = loserRating - winnerRating; // Теперь смотрим разницу правильно

            // Если победитель был слабее, он получает БОЛЬШЕ
            int bonus = ratingDifference / 25;
            return baseRating + Math.Max(bonus, 0); // Убедимся, что не уменьшаем рейтинг за победу
        }

        private static int CalculateDrawRating(int rating1, int rating2)
        {
            int ratingDifference = rating1 - rating2;

            // Слабый игрок получает больше за ничью
            int adjustment = ratingDifference / 25;
            return -adjustment; // Уменьшаем у сильного, увеличиваем у слабого
        }


        private static void UpdateMatchStats(string leftPlayer, string rightPlayer, int leftRatingChange, int rightRatingChange, string result)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = result switch
                {
                    "left" => "UPDATE Players SET Wins = Wins + 1, Score = MAX(0, Score + @LeftRatingChange) WHERE Name = @LeftPlayer; " +
                              "UPDATE Players SET Losses = Losses + 1, Score = MAX(0, Score + @RightRatingChange) WHERE Name = @RightPlayer;",
                    "right" => "UPDATE Players SET Wins = Wins + 1, Score = MAX(0, Score + @RightRatingChange) WHERE Name = @RightPlayer; " +
                               "UPDATE Players SET Losses = Losses + 1, Score = MAX(0, Score + @LeftRatingChange) WHERE Name = @LeftPlayer;",
                    "draw" => "UPDATE Players SET Draws = Draws + 1, Score = MAX(0, Score + @LeftRatingChange) WHERE Name = @LeftPlayer; " +
                              "UPDATE Players SET Draws = Draws + 1, Score = MAX(0, Score + @RightRatingChange) WHERE Name = @RightPlayer;",
                    _ => throw new ArgumentException("Неверный результат матча")
                };

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LeftPlayer", leftPlayer);
                    command.Parameters.AddWithValue("@RightPlayer", rightPlayer);
                    command.Parameters.AddWithValue("@LeftRatingChange", leftRatingChange);
                    command.Parameters.AddWithValue("@RightRatingChange", rightRatingChange);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void ShowMessage(TextBlock textBlock, string message, bool success)
        {
            textBlock.Text = message;
            textBlock.Foreground = success ? Brushes.LimeGreen : Brushes.Red;
        }
    }
}

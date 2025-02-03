using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Bersetka.windows.main.addUser;
using Bersetka.windows.main.editUser;

namespace Bersetka.windows.main
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=.\DataBase\Bersetka.db;Version=3;";
        private List<string> playersList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            SetupContextMenu();
            LoadPlayers();

            LoadWindowSettings();
        }

        private void LoadWindowSettings()
        {
            if (Properties.Settings.Default.WindowWidth > 0)
            {
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Height = Properties.Settings.Default.WindowHeight;
                this.Left = Properties.Settings.Default.WindowLeft;
                this.Top = Properties.Settings.Default.WindowTop;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Сохраняем текущее положение и размер окна
            Properties.Settings.Default.WindowWidth = this.Width;
            Properties.Settings.Default.WindowHeight = this.Height;
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.WindowTop = this.Top;
            Properties.Settings.Default.Save();
        }

    private void LoadData()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Name, Score, Wins, Losses, Draws FROM Players";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridPlayers.ItemsSource = dataTable.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();

            var addUserMenuItem = new MenuItem
            {
                Header = "Добавить участника"
            };
            addUserMenuItem.Click += AddUserMenuItem_Click;

            var editUserMenuItem = new MenuItem
            {
                Header = "Редактировать участника"
            };
            editUserMenuItem.Click += EditUserMenuItem_Click;

            var deleteUserMenuItem = new MenuItem
            {
                Header = "Удалить участника"
            };
            deleteUserMenuItem.Click += DeleteUserMenuItem_Click;

            contextMenu.Items.Add(addUserMenuItem);
            contextMenu.Items.Add(editUserMenuItem);
            contextMenu.Items.Add(deleteUserMenuItem);

            dataGridPlayers.ContextMenu = contextMenu;
        }


        private void AddUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow
            {
                Owner = this, // Устанавливаем владельца
                WindowStartupLocation = WindowStartupLocation.CenterOwner // Центрируем относительно владельца
            };

            if (addUserWindow.ShowDialog() == true)
            {
                AddUserToDatabase(addUserWindow.UserName);
                LoadData(); // Обновляем таблицу после добавления
            }
        }

        private void DeleteUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridPlayers.SelectedItem is DataRowView selectedRow)
            {
                string userName = selectedRow["Name"].ToString();
                MessageBoxResult result = MessageBox.Show(
                    $"Вы точно хотите удалить участника \"{userName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteUserFromDatabase(userName);
                    LoadData(); // Обновляем таблицу после удаления
                }
            }
            else
            {
                MessageBox.Show("Выберите участника для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridPlayers.SelectedItem is DataRowView selectedRow)
            {
                string currentName = selectedRow["Name"].ToString();
                int score = Convert.ToInt32(selectedRow["Score"]);
                int wins = Convert.ToInt32(selectedRow["Wins"]);
                int losses = Convert.ToInt32(selectedRow["Losses"]);
                int draws = Convert.ToInt32(selectedRow["Draws"]);

                var editUserWindow = new EditUserWindow(currentName, score, wins, losses, draws)
                {
                    Owner = this
                };

                if (editUserWindow.ShowDialog() == true)
                {
                    UpdateUserInDatabase(currentName, editUserWindow.UserName, editUserWindow.Score, editUserWindow.Wins, editUserWindow.Losses, editUserWindow.Draws);
                    LoadData(); // Обновляем таблицу после редактирования
                }
            }
            else
            {
                MessageBox.Show("Выберите участника для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddUserToDatabase(string name)
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
                    MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteUserFromDatabase(string name)
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
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateUserInDatabase(string oldName, string newName, int score, int wins, int losses, int draws)
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
                    MessageBox.Show($"Ошибка при обновлении данных участника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(txtSearch.Text);
        }

        private void LoadData(string searchQuery = "")
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Name, Score, Wins, Losses, Draws FROM Players";

                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        query += " WHERE Name LIKE @SearchQuery";
                    }

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchQuery))
                        {
                            command.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");
                        }

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridPlayers.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === 1. ЗАГРУЗКА ИГРОКОВ В СПИСКИ ===
        private void LoadPlayers()
        {
            playersList.Clear();
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
                        {
                            playersList.Add(reader["Name"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки участников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Обновляем списки игроков
            UpdatePlayerLists();
        }

        private void UpdatePlayerLists()
        {
            listBoxLeft.ItemsSource = playersList.ToList(); // Копия списка
            listBoxRight.ItemsSource = playersList.ToList();
        }

        // === 2. ФИЛЬТРАЦИЯ ПОИСКА ===
        private void txtSearchLeft_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearchLeft.Text.ToLower();
            listBoxLeft.ItemsSource = playersList.Where(p => p.ToLower().Contains(search)).ToList();
        }

        private void txtSearchRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearchRight.Text.ToLower();
            listBoxRight.ItemsSource = playersList.Where(p => p.ToLower().Contains(search)).ToList();
        }

        // === 3. ЛОГИКА КНОПОК "ПОБЕДА/НИЧЬЯ" ===
        private void btnWinLeft_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSelection(out string leftPlayer, out string rightPlayer))
            {
                UpdateMatchResult(leftPlayer, rightPlayer, "left");
            }
        }

        private void btnWinRight_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSelection(out string leftPlayer, out string rightPlayer))
            {
                UpdateMatchResult(leftPlayer, rightPlayer, "right");
            }
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSelection(out string leftPlayer, out string rightPlayer))
            {
                UpdateMatchResult(leftPlayer, rightPlayer, "draw");
            }
        }

        // === 4. ВАЛИДАЦИЯ ВЫБОРА ===
        private bool ValidateSelection(out string leftPlayer, out string rightPlayer)
        {
            leftPlayer = listBoxLeft.SelectedItem as string;
            rightPlayer = listBoxRight.SelectedItem as string;

            if (leftPlayer == null || rightPlayer == null)
            {
                ShowResultMessage("Выберите обоих участников!", success: false);
                return false;
            }

            if (leftPlayer == rightPlayer)
            {
                ShowResultMessage("Нельзя выбрать одного и того же игрока!", success: false);
                return false;
            }

            return true;
        }

        // === 5. ОБНОВЛЕНИЕ РЕЗУЛЬТАТОВ В БАЗЕ ===
        private void UpdateMatchResult(string leftPlayer, string rightPlayer, string result)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "";

                    switch (result)
                    {
                        case "left":
                            query = "UPDATE Players SET Wins = Wins + 1 WHERE Name = @LeftPlayer; " +
                                    "UPDATE Players SET Losses = Losses + 1 WHERE Name = @RightPlayer;";
                            break;
                        case "right":
                            query = "UPDATE Players SET Wins = Wins + 1 WHERE Name = @RightPlayer; " +
                                    "UPDATE Players SET Losses = Losses + 1 WHERE Name = @LeftPlayer;";
                            break;
                        case "draw":
                            query = "UPDATE Players SET Draws = Draws + 1 WHERE Name = @LeftPlayer OR Name = @RightPlayer;";
                            break;
                    }

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LeftPlayer", leftPlayer);
                        command.Parameters.AddWithValue("@RightPlayer", rightPlayer);
                        command.ExecuteNonQuery();
                    }

                    ShowResultMessage($"Результат записан: {leftPlayer} vs {rightPlayer}", success: true);
                }
                catch (Exception ex)
                {
                    ShowResultMessage($"Ошибка: {ex.Message}", success: false);
                }
            }

            LoadPlayers(); // Обновляем списки
        }

        // Метод для отображения уведомления
        private void ShowResultMessage(string message, bool success)
        {
            lblResultMessage.Text = message;
            lblResultMessage.Foreground = success ? Brushes.Orange : Brushes.Red;
        }
    }
}
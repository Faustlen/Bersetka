using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            SetupContextMenu();

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

    }
}
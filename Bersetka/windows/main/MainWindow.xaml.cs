using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Bersetka.helpers;
using Bersetka.managers;
using Bersetka.windows.main.addUser;
using Bersetka.windows.main.editUser;

namespace Bersetka.windows.main
{
    public partial class MainWindow : Window
    {
        private List<string> playersList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadPlayers();
            LoadData();
            SetupContextMenu(); // Устанавливаем контекстное меню при запуске
        }


        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();

            var addUserMenuItem = new MenuItem { Header = "Добавить участника" };
            addUserMenuItem.Click += AddUserMenuItem_Click;

            var editUserMenuItem = new MenuItem { Header = "Редактировать участника" };
            editUserMenuItem.Click += EditUserMenuItem_Click;

            var deleteUserMenuItem = new MenuItem { Header = "Удалить участника" };
            deleteUserMenuItem.Click += DeleteUserMenuItem_Click;

            contextMenu.Items.Add(addUserMenuItem);
            contextMenu.Items.Add(editUserMenuItem);
            contextMenu.Items.Add(deleteUserMenuItem);

            dataGridPlayers.ContextMenu = contextMenu;
        }

        private void AddUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };

            if (addUserWindow.ShowDialog() == true)
            {
                DatabaseHelper.AddUserToDatabase(addUserWindow.UserName);
                LoadData(); // Обновляем таблицу после добавления
                LoadPlayers();
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

                var editUserWindow = new EditUserWindow(currentName, score, wins, losses, draws) { Owner = this };

                if (editUserWindow.ShowDialog() == true)
                {
                    DatabaseHelper.UpdateUserInDatabase(currentName, editUserWindow.UserName, editUserWindow.Score, editUserWindow.Wins, editUserWindow.Losses, editUserWindow.Draws);
                    LoadData(); // Обновляем таблицу после редактирования
                    LoadPlayers();
                }
            }
            else
            {
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
                    DatabaseHelper.DeleteUserFromDatabase(userName);
                    LoadData(); // Обновляем таблицу после удаления
                    LoadPlayers();
                }
            }
            else
            {
            }
        }


        private void LoadData()
        {
            UIManager.UpdateDataGrid(dataGridPlayers, DatabaseHelper.GetPlayers());
            SetupContextMenu(); // Добавляем контекстное меню после загрузки данных
        }


        private void LoadPlayers()
        {
            playersList = DatabaseHelper.GetPlayerNames();
            UIManager.UpdatePlayerList(listBoxLeft, playersList);
            UIManager.UpdatePlayerList(listBoxRight, playersList);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) =>
            UIManager.UpdateDataGrid(dataGridPlayers, DatabaseHelper.GetPlayers(txtSearch.Text));

        private void txtSearchLeft_TextChanged(object sender, TextChangedEventArgs e) =>
            PlayerSearchHelper.FilterPlayers(txtSearchLeft, listBoxLeft, playersList);

        private void txtSearchRight_TextChanged(object sender, TextChangedEventArgs e) =>
            PlayerSearchHelper.FilterPlayers(txtSearchRight, listBoxRight, playersList);

        private void btnWinLeft_Click(object sender, RoutedEventArgs e)
        {
            MatchManager.RegisterMatch(listBoxLeft.SelectedItem as string, listBoxRight.SelectedItem as string, "left", lblResultMessage);
            LoadData();
        }

        private void btnWinRight_Click(object sender, RoutedEventArgs e)
        {
            MatchManager.RegisterMatch(listBoxLeft.SelectedItem as string, listBoxRight.SelectedItem as string, "right", lblResultMessage);
            LoadData();
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            MatchManager.RegisterMatch(listBoxLeft.SelectedItem as string, listBoxRight.SelectedItem as string, "draw", lblResultMessage);
            LoadData();
        }
    }
}

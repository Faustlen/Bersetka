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
        private string selectedLeftPlayer;
        private string selectedRightPlayer;
        private string winner; // "left", "right", null (ничья)

        public MainWindow()
        {
            InitializeComponent();
            LoadPlayers();
            LoadData();
            SetupContextMenu();
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
                LoadData();
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
                    LoadData();
                    LoadPlayers();
                }
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
                    LoadData();
                    LoadPlayers();
                }
            }
        }

        private void LoadData()
        {
            UIManager.UpdateDataGrid(dataGridPlayers, DatabaseHelper.GetPlayers());
            SetupContextMenu();
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

        private void listBoxLeft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedLeftPlayer = listBoxLeft.SelectedItem as string;
            btnWinLeft.Content = selectedLeftPlayer ?? "Выберите участника";
        }

        private void listBoxRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRightPlayer = listBoxRight.SelectedItem as string;
            btnWinRight.Content = selectedRightPlayer ?? "Выберите участника";
        }

        private void btnWinLeft_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSelection())
            {
                winner = "left";
                lblLeftCrown.Text = "👑";
                lblRightCrown.Text = "";
            }
        }

        private void btnWinRight_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSelection())
            {
                winner = "right";
                lblRightCrown.Text = "👑";
                lblLeftCrown.Text = "";
            }
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            winner = null;
            lblLeftCrown.Text = "";
            lblRightCrown.Text = "";
        }

        private void btnSubmitMatch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection()) return;

            string matchResult = winner switch
            {
                "left" => "left",
                "right" => "right",
                _ => "draw"
            };

            MatchManager.RegisterMatch(selectedLeftPlayer, selectedRightPlayer, matchResult, lblResultMessage);

            ResetMatchSelection();
        }

        private bool ValidateSelection()
        {
            if (string.IsNullOrEmpty(selectedLeftPlayer) || string.IsNullOrEmpty(selectedRightPlayer))
            {
                ShowResultMessage("Выберите обоих участников!", false);
                return false;
            }

            return true;
        }

        private void ResetMatchSelection()
        {
            listBoxLeft.SelectedItem = null;
            listBoxRight.SelectedItem = null;
            selectedLeftPlayer = null;
            selectedRightPlayer = null;
            btnWinLeft.Content = "Выберите участника";
            btnWinRight.Content = "Выберите участника";
            lblLeftCrown.Text = "";
            lblRightCrown.Text = "";
            txtSearchLeft.Text = "";
            txtSearchRight.Text = "";
            winner = null;
        }

        private void ShowResultMessage(string message, bool success)
        {
            lblResultMessage.Text = message;
            lblResultMessage.Foreground = success ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;
        }
    }
}

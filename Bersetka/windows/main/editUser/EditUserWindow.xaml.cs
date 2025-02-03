using System;
using System.Windows;

namespace Bersetka.windows.main.editUser
{
    public partial class EditUserWindow : Window
    {
        public string UserName { get; private set; }
        public int Score { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int Draws { get; private set; }

        public EditUserWindow(string currentName, int score, int wins, int losses, int draws)
        {
            InitializeComponent();

            // Заполняем поля текущими данными
            txtUserName.Text = currentName;
            txtScore.Text = score.ToString();
            txtWins.Text = wins.ToString();
            txtLosses.Text = losses.ToString();
            txtDraws.Text = draws.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Имя пользователя не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtScore.Text, out int score) ||
                !int.TryParse(txtWins.Text, out int wins) ||
                !int.TryParse(txtLosses.Text, out int losses) ||
                !int.TryParse(txtDraws.Text, out int draws))
            {
                MessageBox.Show("Очки, победы, поражения и ничьи должны быть числовыми значениями!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UserName = txtUserName.Text;
            Score = score;
            Wins = wins;
            Losses = losses;
            Draws = draws;

            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

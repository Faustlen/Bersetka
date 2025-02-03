using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Bersetka.helpers
{
    public static class PlayerSearchHelper
    {
        public static void FilterPlayers(TextBox searchBox, ListBox listBox, List<string> playersList)
        {
            string search = searchBox.Text.ToLower();
            listBox.ItemsSource = playersList.Where(p => p.ToLower().Contains(search)).ToList();
        }
    }
}

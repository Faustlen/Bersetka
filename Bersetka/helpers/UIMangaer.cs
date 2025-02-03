using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace Bersetka.helpers
{
    public static class UIManager
    {
        public static void UpdatePlayerList(ListBox listBox, List<string> players)
        {
            listBox.ItemsSource = players.ToList();
        }

        public static void UpdateDataGrid(DataGrid dataGrid, DataTable data)
        {
            dataGrid.ItemsSource = data.DefaultView;
        }
    }
}

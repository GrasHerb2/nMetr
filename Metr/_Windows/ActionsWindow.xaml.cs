using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Metr
{
    /// <summary>
    /// Логика взаимодействия для ActionsWindow.xaml
    /// </summary>
    public partial class ActionsWindow : Window
    {
        List<Actions> actionsList;
        MetrBaseEntities context = MetrBaseEntities.GetContext();
        public int userID { get; set; }
        public ActionsWindow(int uID = 0)
        {
            InitializeComponent();
            searchTBFN.ItemsSource = context.User.Where(a=>a.Actions.Count!=0).Select(a=>a.FullName).ToList();
            userID = uID;
            try
            {
                searchTBFN.Text = userID != 0 ? context.User.Where(a => a.User_ID == userID).FirstOrDefault().FullName : "";
            }
            catch { }
            Update();
        }

        void Update()
        {
            actionsList = context.Actions.ToList();
            if (searchTBFN.Text != "")
                actionsList = actionsList.Where(a => a.User.FullName == searchTBFN.Text).ToList();

            if (DateStart.SelectedDate != null)
                actionsList = actionsList.Where(a => DateTime.Compare(DateStart.SelectedDate.Value, a.ActionDate) <= 0).ToList();
            if (DateEnd.SelectedDate != null)
                actionsList = actionsList.Where(a => DateTime.Compare(DateEnd.SelectedDate.Value, a.ActionDate) >= 0).ToList();

            actionsList = actionsList.OrderByDescending(a=>a.ActionDate).ToList();

            CountLbl.Content = "Записи:"+actionsList.Count+" из "+context.Actions.Count();

            mainGrid.ItemsSource = null;
            mainGrid.ItemsSource = actionsList;
        }

        private void mainGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Update();
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void restoreBtn_Click(object sender, RoutedEventArgs e)
        {
            searchTBFN.Text = "";
            DateStart.SelectedDate = null;
            DateEnd.SelectedDate = null;
            Update();
        }
    }
}

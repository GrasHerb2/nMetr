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
        List<Operation> operationList;
        MetrBaseEn context = MetrBaseEn.GetContext();
        public int userID { get; set; }

        int preCount = 0;
        public ActionsWindow(int uID = 0)
        {
            InitializeComponent();
            searchTBFN.ItemsSource = context.User.Where(a=>a.Operation.Count!=0).Select(a=>a.FullName).ToList();
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
            operationList = context.Operation.ToList();
            preCount = operationList.Count();
            if (searchTBFN.Text != "")
                operationList = operationList.Where(a => a.User.FullName == searchTBFN.Text).ToList();

            if (DateStart.SelectedDate != null)
                operationList = operationList.Where(a => DateTime.Compare(DateStart.SelectedDate.Value, a.OperationDate) <= 0).ToList();
            if (DateEnd.SelectedDate != null)
                operationList = operationList.Where(a => DateTime.Compare(DateEnd.SelectedDate.Value, a.OperationDate) >= 0).ToList();

            operationList = operationList.OrderByDescending(a=>a.OperationDate).ToList();

            CountLbl.Content = "Записи:"+operationList.Count+" из "+context.Operation.Count();

            mainGrid.ItemsSource = null;
            mainGrid.ItemsSource = operationList;
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

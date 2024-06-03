using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Metr._Windows
{
    /// <summary>
    /// Логика взаимодействия для HistWindow.xaml
    /// </summary>
    /// 

    public partial class HistWindow : Window
    {
        int devId = 0;
        int uId = 0;
        int preCount = 0;
        List<Operation> operations = new List<Operation>();
        DateTime temp = new DateTime();
        public HistWindow(int devId = 0, int uId = 0)
        {
            InitializeComponent();
            this.devId = devId;
            this.uId = uId;

            if (uId != 0)
            {
                Title = "История операций с прибором: " + MetrBaseEn.GetContext().Device.Where(d => d.Device_ID == devId).FirstOrDefault().Name;
                Update(1);
            }

            if (devId != 0)
            {
                Title = "История операций с прибором: " + MetrBaseEn.GetContext().Device.Where(d => d.Device_ID == devId).FirstOrDefault().Name;
                Update(2);
            }
            
            if(devId == 0 && uId == 0)
            {
                Update();
            }

            searchCBType.ItemsSource = null;
            searchCBType.ItemsSource = operations.Select(o => o.OperationType.Title).Distinct().ToList();

            searchCBUser.ItemsSource = null;
            searchCBUser.ItemsSource = operations.Select(o => o.User.FullName).Distinct().ToList();
        }
        void Update(int t = 0)
        {
            switch (t)
            {
                default:
                    operations = MetrBaseEn.GetContext().Operation.ToList();
                    break;
                case 1:
                    operations = MetrBaseEn.GetContext(true).User.Where(d => d.User_ID == uId).FirstOrDefault().Operation.ToList();
                    break;
                case 2:
                    operations = MetrBaseEn.GetContext(true).Device.Where(d => d.Device_ID == devId).FirstOrDefault().Operation.ToList();                    
                    break;
            }
            preCount = operations.Count();
            mainGrid.ItemsSource = null;
            mainGrid.ItemsSource = operations;
        }
        void Search()
        {
            Update();

            if (DateStart.SelectedDate>DateEnd.SelectedDate && DateStart.SelectedDate!=null && DateEnd.SelectedDate!=null)
            {
                temp = DateEnd.SelectedDate.Value;
                DateEnd.SelectedDate = DateStart.SelectedDate;
                DateStart.SelectedDate = temp;
            }


            operations = DateStart.SelectedDate!=null? operations.Where(o=> o.OperationDate>=DateStart.SelectedDate.Value).ToList() : operations;

            operations = DateEnd.SelectedDate!=null? operations.Where(o=> o.OperationDate<=DateEnd.SelectedDate.Value).ToList() : operations;

            operations = searchCBType.Text != "" ? operations.Where(o=> o.OperationType.Title == searchCBType.Text).ToList() : operations;

            operations = searchCBUser.Text != "" ? operations.Where(o => o.User.FullName == searchCBUser.Text).ToList() : operations;

            operations = searchTxtDesc.Text != "" ? operations.Where(o => o.OperationText.Contains(searchTxtDesc.Text)).ToList() : operations;

            CountLbl.Content = "Записи:" + operations.Count + " из " + preCount;

            operations = operations.OrderByDescending(o=>o.OperationDate).ToList();

            mainGrid.ItemsSource = null;
            mainGrid.ItemsSource = operations;
        }
        private void restoreBtn_Click(object sender, RoutedEventArgs e)
        {
            searchCBType.Text = "";
            DateStart.SelectedDate = null;
            DateEnd.SelectedDate = null;
            searchCBUser.Text = "";
            searchTxtDesc.Text = "";
            Search();
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void mainGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Search();
        }
    }
}

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

namespace Metr
{
    /// <summary>
    /// Логика взаимодействия для UserActivate.xaml
    /// </summary>
    public partial class UserActivate : Window
    {
        public int selectedRole { get; set; }
        public UserActivate(string Info)
        {
            InitializeComponent();
            infoText.Text = Info;
            roleCmb.Text = "Выберите доступ";
            roleCmb.ItemsSource = MetrBaseEn.GetContext().Role.Select(r=>r.Title).ToList();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (roleCmb.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите доступ пользователя.","Активация", MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            else
            {
                selectedRole = roleCmb.SelectedIndex +1;
                DialogResult = true;
            }
        }
    }
}

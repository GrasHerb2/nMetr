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
    /// Логика взаимодействия для TextWin.xaml
    /// </summary>
    public partial class TextWin : Window
    {
        public string textOut { get; set; }
        int type = 0;
        public TextWin(string Message, string Header, int type = 0)
        {
            InitializeComponent();

            this.mainLbl.Content = Message;
            this.Title = Header;
            this.type = type;
        }

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(inputTxt.Text)||type == 1)
            {
                textOut = inputTxt.Text;
                DialogResult = true;
            }
            else MessageBox.Show("Введите название объекта", Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

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
        public TextWin(string Message, string Header)
        {
            InitializeComponent();
            this.mainLbl.Content = Message;
            this.Title = Header;
        }

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            textOut = inputTxt.Text;
            DialogResult = true;
        }
    }
}

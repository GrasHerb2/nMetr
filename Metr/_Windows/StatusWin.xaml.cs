using Metr.Classes;
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
    /// Логика взаимодействия для StatusWin.xaml
    /// </summary>
    public partial class StatusWin : Window
    {
        public StatusWin()
        {
            InitializeComponent();
            statusTb.DataContext = new { stString = SettingsClass.status };
        }
        void statusChange(object sender, EventArgs e)
        {
            if(statusTb.Text == "☼") this.Close();
        }

    }
}

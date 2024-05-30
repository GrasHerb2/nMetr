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

namespace Metr
{
    /// <summary>
    /// Логика взаимодействия для RegWindow.xaml
    /// </summary>
    public partial class RegWindow : Window
    {
        MetrBaseEn context = MetrBaseEn.GetContext();
        public RegWindow()
        {
            InitializeComponent();
        }

        private void cBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(lTxt.Text) || string.IsNullOrEmpty(pTxt.Text) || string.IsNullOrEmpty(fNameTxt.Text))
            {
                MessageBox.Show("Все поля, кроме почты, обязательны","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var a = UControl.newEmployee(lTxt.Text,pTxt.Text,fNameTxt.Text,mTxt.Text!=""?mTxt.Text:"-");
            switch (a.resultid)
            {
                case 1:
                    MessageBox.Show("Логин занят, необходимо использовать иной", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;
                case 0:
                    {
                        switch (MessageBox.Show("Вы уверены, что хотите отправить заявку на создание учётной записи?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                context.User.Add(a.User);
                                context.SaveChanges();
                                a.Action.UserID = context.User.Where(u => u.ULogin == a.User.ULogin).FirstOrDefault().User_ID;
                                context.Actions.Add(a.Action);
                                context.SaveChanges();
                                MessageBox.Show("Заявка отправлена и ждёт подтверждения, вы можете воспользоваться режимом просмотра","Сообщение",MessageBoxButton.OK,MessageBoxImage.Information);
                                this.DialogResult = true;
                                return;
                            case MessageBoxResult.No:
                                MessageBox.Show("Заявка отменена", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            case MessageBoxResult.Cancel:
                                break;
                        }

                    }
                    break;
                case -1:
                    MessageBox.Show(a.Action.ActionText, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}

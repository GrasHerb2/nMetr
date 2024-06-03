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
        int windowType = 1;
        UControl.tResult result = null;
        public RegWindow(int type = 1)
        {
            InitializeComponent();
            windowType = type;
            switch (windowType)
            {
                case 1:
                    Title = "Запрос на добавление учётной записи";
                    break;
                case 2:
                    Title = "Добавление учётной записи";
                    break;
                case 3:
                    Title = "Изменение учётной записи";

                    fNameTxt.Text = CurrentUser.user.FullName;
                    lTxt.Text = CurrentUser.user.ULogin;
                    pTxt.Text = CurrentUser.user.UPass;
                    mTxt.Text = CurrentUser.user.Email;

                    break;
            }
        }

        private void cBtn_Click(object sender, RoutedEventArgs e)
        {            
            if (string.IsNullOrEmpty(lTxt.Text) || string.IsNullOrEmpty(pTxt.Text) || string.IsNullOrEmpty(fNameTxt.Text))
            {
                MessageBox.Show("Все поля, кроме почты, обязательны", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            switch (windowType)
            {
                case 1:

                    result = UControl.newEmployee(lTxt.Text, pTxt.Text, fNameTxt.Text, mTxt.Text != "" ? mTxt.Text : "-");
                    if (result.resultid != 0)
                    {
                        MessageBox.Show(result.errorText, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        switch (MessageBox.Show("Вы уверены, что хотите отправить заявку на создание учётной записи?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                context.User.Add(result.User);
                                context.SaveChanges();
                                result.Operation.UserID = context.User.Where(u => u.ULogin == result.User.ULogin).FirstOrDefault().User_ID;
                                context.Operation.Add(result.Operation);
                                context.SaveChanges();
                                MessageBox.Show("Заявка отправлена и ждёт подтверждения, вы можете воспользоваться режимом просмотра", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
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
                case 2:

                    

                    break;
                case 3:

                    result = UControl.redactEmployee(lTxt.Text, pTxt.Text, fNameTxt.Text, mTxt.Text != "" ? mTxt.Text : "-");
                    if (result.resultid != 0)
                    {
                        MessageBox.Show(result.errorText, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        switch (MessageBox.Show("Вы уверены, что хотите сохранить изменения учётной записи?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:

                                User chUser = context.User.Where(u => u.User_ID == CurrentUser.user.User_ID).FirstOrDefault();

                                chUser.FullName = result.User.FullName;
                                chUser.ULogin = result.User.ULogin;
                                chUser.UPass = result.User.UPass;
                                chUser.Email = result.User.Email;

                                context.SaveChanges();
                                result.Operation.UserID = CurrentUser.user.User_ID;
                                context.Operation.Add(result.Operation);
                                context.SaveChanges();
                                MessageBox.Show("Учётная запись изменена", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                                this.DialogResult = true;
                                return;
                            case MessageBoxResult.No:
                                MessageBox.Show("Изменения отменены", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            case MessageBoxResult.Cancel:
                                break;
                        }

                    }

                    break;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (windowType == 3) MessageBox.Show("Изменения отменены", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}


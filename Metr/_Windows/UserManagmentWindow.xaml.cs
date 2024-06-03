using Metr._Windows;
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
using static Metr.Classes.UControl;

namespace Metr
{
    /// <summary>
    /// Логика взаимодействия для UserManagmentWindow.xaml
    /// </summary>
    public partial class UserManagmentWindow : Window
    {
        MetrBaseEn context;
        List<Operation> register = new List<Operation>();
        public UserManagmentWindow()
        {
            InitializeComponent();            
            context = MetrBaseEn.GetContext();
            UpdateTabs();
        }
        void UpdateTabs() 
        {
            UControl.UpdateUsers(context);

            register.Clear();

            foreach (UControl u in UControl.UserDataRegister)
            {
                register.Add(context.Operation.Where(a => a.UserID == u.userID && a.ID_Type == 1 && a.ID_Status==2).First());
            }

            mainGrid.ItemsSource = null;
            mainGrid.ItemsSource = UControl.UserData;
            regGrid.ItemsSource = null;
            regGrid.ItemsSource = register;
            regUsersTab.Header = "Регистрация " + register.Count();
            deaGrid.ItemsSource = UControl.UserDataDeactive;
            regUsersTab.Visibility = UControl.UserDataRegister.Count() != 0 ? Visibility.Visible : Visibility.Collapsed;
            deaUsersTab.Visibility = UControl.UserDataDeactive.Count() != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        
        void uActionsShow()
        {

            if (mainGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите учётную запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UControl user = mainGrid.SelectedItem as UControl;
            HistWindow histWindow = new HistWindow(0,user.userID);
            histWindow.ShowDialog();
        }
        private void userActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            uActionsShow();
        }

        private void cActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            uActionsShow();
        }

        private void cDeactiveBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mainGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите учётную запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UControl c = mainGrid.SelectedItem as UControl;

            if (c.userID == CurrentUser.user.User_ID || (c.roleID == 3 && UControl.UserData.Where(u=>u.roleID==3).Count() == 1))
            {
                MessageBox.Show("Невозможно деактивировать запись на которой вы в данный момент работаете!","Предупреждение",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show("Вы уверены, что хотите деактивировать учётную запись\n"+c.fullName+"?","Деактивация",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var result = UControl.deactiveEmp(c.userID, CurrentUser.user.User_ID);
                switch (result.resultid)
                {
                    case 0:
                        MessageBox.Show("Учётная запись была деактивирована.","Деактивация",MessageBoxButton.OK,MessageBoxImage.Information);
                        User a = context.User.Where(u => u.User_ID == c.userID).FirstOrDefault();
                        a = result.User;
                        context.Operation.Add(result.Operation);
                        context.SaveChanges();
                        UpdateTabs();
                        break;
                    default: 
                        MessageBox.Show(result.errorText, "Деактивация", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private void userRecover_Click(object sender, RoutedEventArgs e)
        {

            if (deaGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите учётную запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UControl c = deaGrid.SelectedItem as UControl;
            if (MessageBox.Show("Вы уверены, что хотите восстановить учётную запись\n" + c.fullName + "?", "Восстановление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UControl.tResult result = UControl.recoverEmp(c.userID, CurrentUser.user.User_ID);
                switch (result.resultid)
                {
                    case 0:
                        MessageBox.Show("Учётная запись была восстановлена.", "Восстановление", MessageBoxButton.OK, MessageBoxImage.Information);
                        User a = context.User.Where(u => u.User_ID == c.userID).FirstOrDefault();
                        a = result.User;
                        context.Operation.Add(result.Operation);
                        context.SaveChanges();
                        UpdateTabs();
                        break;
                    default:
                        MessageBox.Show(result.errorText, "Восстановление", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }
        void deaActionsShow()
        {
            if (deaGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите учётную запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UControl user = deaGrid.SelectedItem as UControl;
            HistWindow histWindow = new HistWindow(0,user.userID);
            histWindow.ShowDialog();
        }
        private void deaActions_Click(object sender, RoutedEventArgs e)
        {
            deaActionsShow();
        }

        private void deaActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            deaActionsShow();
        }

        void userRegister()
        {
            if (regGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите учётную запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                User adm = context.User.Where(u => u.User_ID == CurrentUser.user.User_ID).FirstOrDefault();
                int id = (regGrid.SelectedItem as Operation).UserID;
                User user = context.User.Where(u => u.User_ID == id).FirstOrDefault();
                UserActivate userActivate = new UserActivate("Пользователю будет открыт доступ к системе\nГость - только просмотр данных\nПользователь - редактирование журнала\nАдминистратор - редактирование журнала и управление учётными записями");
                userActivate.ShowDialog();
                if (userActivate.DialogResult == true)
                {
                    var a = UControl.activateEmp(id, CurrentUser.user.User_ID, userActivate.selectedRole);
                    if (a.resultid != 0)
                    {
                        MessageBox.Show(a.errorText, "Активация", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    user = a.User;
                    string roletxt = context.Role.Where(r => r.Role_ID == userActivate.selectedRole).FirstOrDefault().Title;
                    Operation op = context.Operation.Where(o => o.UserID == id).FirstOrDefault();
                    op.ID_Status = 1;
                    op.OperationText += "\nАктивировал " + adm.FullName;
                    context.SaveChanges();
                    MessageBox.Show("Активация учётной записи произведена", "Активация", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateTabs();
                }
            }
        }

        private void uRegBtn_Click(object sender, RoutedEventArgs e)
        {
            userRegister();
        }

        private void regUserBtn_Click(object sender, RoutedEventArgs e)
        {
            userRegister();
        }

        private void uCnlBtn_Click(object sender, RoutedEventArgs e)
        {
            if (regGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите учётную запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                User adm = context.User.Where(u => u.User_ID == CurrentUser.user.User_ID).FirstOrDefault();
                Operation regOp = regGrid.SelectedItem as Operation;
                User user = context.User.Where(u => u.User_ID == regOp.UserID).FirstOrDefault();
                if (MessageBox.Show("Регистрация будет отменена без возможности отмены! Продолжить?", "Отмена регистрации", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Operation op = context.Operation.Where(o => o.UserID == regOp.UserID).FirstOrDefault();
                        op.ID_Status = 3;
                        op.OperationText += "\nОтменил " + adm.FullName;
                        op.UserID = adm.User_ID;
                        context.SaveChanges();
                        context.User.Remove(user);
                        context.SaveChanges();
                        MessageBox.Show("Регистрация отменена", "Отмена регистрации", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.InnerException.Message.ToString(), "Отмена регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            UpdateTabs();
        }

        private void addUserBtn_Click(object sender, RoutedEventArgs e)
        {
            RegWindow regWindow = new RegWindow(2);
            regWindow.ShowDialog();
            UpdateTabs();
        }
    }
}

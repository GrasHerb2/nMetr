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
    /// Логика взаимодействия для UserManagmentWindow.xaml
    /// </summary>
    public partial class UserManagmentWindow : Window
    {
        public int User { get; set; }
        MetrBaseEn context;
        List<Actions> register = new List<Actions>();
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
                register.Add(context.Actions.Where(a => a.UserID == u.userID).First());
            }

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
            ActionsWindow actionsWindow = new ActionsWindow(user.userID);
            actionsWindow.ShowDialog();
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

            if (c.userID == User || (c.roleID == 3 && UControl.UserData.Where(u=>u.roleID==3).Count() == 1))
            {
                MessageBox.Show("Невозможно деактивировать запись на которой вы в данный момент работаете!","Предупреждение",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show("Вы уверены, что хотите деактивировать учётную запись\n"+c.fullName+"?","Деактивация",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UControl.tResult result = UControl.deactiveEmp(c.userID, User);
                switch (result.resultid)
                {
                    case 0:
                        MessageBox.Show("Учётная запись была деактивирована.","Деактивация",MessageBoxButton.OK,MessageBoxImage.Information);
                        User a = context.User.Where(u => u.User_ID == c.userID).FirstOrDefault();
                        a = result.User;
                        context.Actions.Add(result.Action);
                        context.SaveChanges();
                        UpdateTabs();
                        break;
                    default: break;
                }
            }
            UpdateTabs();
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
                UControl.tResult result = UControl.recoverEmp(c.userID, User);
                switch (result.resultid)
                {
                    case 0:
                        MessageBox.Show("Учётная запись была восстановлена.", "Восстановление", MessageBoxButton.OK, MessageBoxImage.Information);
                        User a = context.User.Where(u => u.User_ID == c.userID).FirstOrDefault();
                        a = result.User;
                        context.Actions.Add(result.Action);
                        context.SaveChanges();
                        UpdateTabs();
                        break;
                    default: break;
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
            ActionsWindow actionsWindow = new ActionsWindow(user.userID);
            actionsWindow.ShowDialog();
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
                MessageBox.Show("Выберите учётную запись","Ошибка",MessageBoxButton.OK,MessageBoxImage.Information);
                return;
            }
            else
            {
                User adm = context.User.Where(u => u.User_ID == User).FirstOrDefault();
                int id = (regGrid.SelectedItem as Actions).UserID;
                User user = context.User.Where(u=>u.User_ID==id).FirstOrDefault();
                UserActivate userActivate = new UserActivate("Пользователю будет открыт доступ к системе\nГость - только просмотр данных\nПользователь - редактирование журнала\nАдминистратор - редактирование журнала и управление учётными записями");
                userActivate.ShowDialog();
                if (userActivate.DialogResult == true)
                {

                    var a = UControl.activateEmp(id, User , userActivate.selectedRole);
                    user = a.User;
                    string roletxt = context.Role.Where(r=>r.Role_ID==userActivate.selectedRole).FirstOrDefault().Title;
                    context.Actions.Add(new Actions()
                    {
                        ActionDate = DateTime.Now,
                        ActionText = adm.FullName + " открыл доступ с уровнем \"" + roletxt + "\" " + user.FullName,
                        UserID = User,
                        ComputerName = Environment.MachineName
                    });
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
                User adm = context.User.Where(u => u.User_ID == User).FirstOrDefault();
                int id = (regGrid.SelectedItem as Actions).UserID;
                User user = context.User.Where(u => u.User_ID == id).FirstOrDefault();
                UserActivate userActivate = new UserActivate("Пользователю будет открыт доступ к системе\nГость - только просмотр данных\nПользователь - редактирование журнала\nАдминистратор - редактирование журнала и управление учётными записями");
                userActivate.ShowDialog();
                if (userActivate.DialogResult == true)
                {

                    var a = UControl.activateEmp(id, User, userActivate.selectedRole);
                    user = a.User;
                    string roletxt = context.Role.Where(r => r.Role_ID == userActivate.selectedRole).FirstOrDefault().Title;
                    context.Actions.Add(new Actions()
                    {
                        ActionDate = DateTime.Now,
                        ActionText = adm.FullName + " открыл доступ с уровнем \"" + roletxt + "\" " + user.FullName,
                        UserID = User,
                        ComputerName = Environment.MachineName
                    });
                    context.SaveChanges();
                    MessageBox.Show("Активация учётной записи произведена", "Активация", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateTabs();
                }
            }
        }
    }
}

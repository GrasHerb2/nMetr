using Metr._Windows;
using Metr.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Metr
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MetrBaseEn context = MetrBaseEn.GetContext();

        List<string> objNames = new List<string>();
        List<string> dSearch = new List<string>();
        List<string> objects = new List<string>();


        ExportWindow exWin;
        HistWindow histWindow;
        UserManagmentWindow umw;

        DateTime? searchStart = null;
        DateTime? searchEnd = null;
        string tempText;
        bool searchHide;
        bool searchDel;
        bool pprDate;
        bool Exp;


        string objTemp = "";
        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;


            try
            {
                SettingsClass.LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            logIn();
            Thread thread = new Thread(UpdateTabs) { IsBackground = true };
            thread.Start();
            EClass.UpdPresets();
        }
        //Метод запроса авторизации
        void logIn()
        {
            AuthWindow authWin = new AuthWindow();
            authWin.ShowDialog();

            switch (authWin.DialogResult)
            {
                case true:
                    if (CurrentUser.user.User_ID != 1)
                    {
                        MessageBox.Show("Вы вошли как " + CurrentUser.user.FullName + "\nДобро пожаловать!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("В режиме просмотра вам не доступны действия связанные с редактированием", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    break;

                case false:
                    Application.Current.Shutdown();
                    break;
            }
        }
        //Метод обновления вкладок
        void UpdateTabs()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    objNames = MetrBaseEn.GetContext().Object.Select(o => o.Name).ToList();
                    pBar.Visibility = Visibility.Visible;
                }));

                DeviceData.dataUpdate();
                startSearch();

                Dispatcher.Invoke(new Action(() =>
                {
                    BottomUpdate();

                    deviceGrid.ItemsSource = null;
                    deviceGrid.ItemsSource = DeviceData.deviceListMain;

                    pprGrid.ItemsSource = null;
                    pprGrid.ItemsSource = DeviceData.deviceListPPR;

                    excGrid.ItemsSource = null;
                    excGrid.ItemsSource = DeviceData.deviceListExc;

                    pBar.Visibility = Visibility.Collapsed;
                    searchTBObj.ItemsSource = (context.Object.OrderBy(d => d.Name).Select(d => d.Name).ToList());
                }));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message.ToString() + "\n" + ex.InnerException.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    pBar.Value = 0;
                    pBar.Visibility = Visibility.Collapsed;
                }));
            }
        }
        //Метод поиска
        void startSearch()
        {
            try
            {
                DeviceData.dataUpdate();
                Dispatcher.Invoke(new Action(() =>
                {
                    searchDel = delCheck.IsChecked.Value;
                    pprDate = dateChB.IsChecked.Value;
                    Exp = expChB.IsChecked.Value;
                    searchStart = expDateStart.SelectedDate != null ? expDateStart.SelectedDate : DateTime.MinValue;
                    searchEnd = expDateEnd.SelectedDate != null ? expDateEnd.SelectedDate : DateTime.MaxValue;
                    dSearch = new List<string>() { searchTBNum.Text.ToLower(), searchTBName.Text.ToLower() };
                    searchHide = hideCheck.IsChecked.Value;
                }));

                DeviceData.Search(dSearch, objects, searchStart.Value, searchEnd.Value, searchHide, searchDel, pprDate, Exp);

                Dispatcher.Invoke(new Action(() =>
                {
                    BottomUpdate();

                    deviceGrid.ItemsSource = null;
                    deviceGrid.ItemsSource = DeviceData.deviceListMain;

                    pprGrid.ItemsSource = null;
                    pprGrid.ItemsSource = DeviceData.deviceListPPR;

                    excGrid.ItemsSource = null;
                    excGrid.ItemsSource = DeviceData.deviceListExc;

                    if (DeviceData.deviceListMain.Count == 0)
                        MessageBox.Show("Не было найдено приборов по заданым критериям", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                }));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
        }



        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(startSearch);
            thread.Start();
        }
        private void cHide_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    List<Device> devicesHide = new List<Device>();
                    List<Device> devicesUnHide = new List<Device>();
                    foreach (DeviceData d in deviceGrid.SelectedItems)
                    {
                        if (d.Hidden)
                            devicesUnHide.Add(context.Device.Where(dev => dev.Device_ID == d.ID).FirstOrDefault());
                        else
                            devicesHide.Add(context.Device.Where(dev => dev.Device_ID == d.ID).FirstOrDefault());
                    }
                    if (devicesHide.Count() > 0) DeviceData.deviceHide(devicesHide, context, CurrentUser.user.User_ID);
                    if (devicesUnHide.Count() > 0) DeviceData.deviceUnHide(devicesUnHide, context, CurrentUser.user.User_ID);
                }
                else MessageBox.Show("Для изменения видимости необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    List<Device> devicesDel = new List<Device>();
                    List<Device> devicesRec = new List<Device>();
                    foreach (DeviceData d in deviceGrid.SelectedItems)
                    {
                        if (!d.Delete)
                            devicesDel.Add(context.Device.Where(dev => dev.Device_ID == d.ID).FirstOrDefault());
                        else
                            devicesRec.Add(context.Device.Where(dev => dev.Device_ID == d.ID).FirstOrDefault());
                    }
                    if (devicesDel.Count() > 0) DeviceData.deviceDel(devicesDel, context, CurrentUser.user.User_ID);
                    if (devicesRec.Count() > 0) DeviceData.deviceRec(devicesRec, context, CurrentUser.user.User_ID);
                }
                else MessageBox.Show("Для удаления необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    DeviceWin newDevice = new DeviceWin(true);
                    newDevice.ShowDialog();
                    switch (newDevice.DialogResult)
                    {
                        case true:
                            MessageBox.Show("Сохранено!", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case false:
                            MessageBox.Show("Добавление отменено", "Добавление", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        default:

                            break;
                    }
                }
                else MessageBox.Show("Для добавления необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(startSearch) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void restoreBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                delCheck.IsChecked = false;
                expDateStart.SelectedDate = null;
                expDateEnd.SelectedDate = null;
                searchTBNum.Text = "";
                searchTBName.Text = "";
                searchTBObj.Text = "";
                objects.Clear();
                ObjListView.ItemsSource = null;
                ObjListView.ItemsSource = objects;
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void redacting()
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    int index = ((DeviceData)deviceGrid.SelectedItems[0]).ID;
                    DeviceWin newDevice = new DeviceWin(false, index);
                    newDevice.ShowDialog();
                    switch (newDevice.DialogResult)
                    {
                        case false:
                            MessageBox.Show("Изменение отменено", "Изменение", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        default:

                            break;
                    }
                }
                else MessageBox.Show("Для редактирования необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void contextEdit_Click(object sender, RoutedEventArgs e)
        {
            redacting();
        }

        private void redactBtn_Click(object sender, RoutedEventArgs e)
        {
            redacting();
        }

        private void deviceGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    DeviceData device = e.Row.Item as DeviceData;
                    Device dev = context.Device.Where(d => d.Device_ID == device.ID).FirstOrDefault();
                    DeviceData.DeviceEdit(dev, device.Name, device.ObjectName, device.FNum, device.Param, device.MetrData, device.ExpDate, int.Parse(device.Period), device.Note, CurrentUser.user.User_ID);
                }
                else MessageBox.Show("Для редактирования необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void BottomUpdate()
        {
            try
            {
                switch (mainTab.SelectedIndex)
                {
                    case 0:
                        itemCountLbl.Content = DeviceData.infoMain;
                        hideCheck.IsEnabled = true;
                        expChB.IsEnabled = true;
                        DatePickers.IsEnabled = true;
                        dateChB.IsEnabled = false;
                        break;
                    case 1:
                        itemCountLbl.Content = DeviceData.infoPPR;
                        hideCheck.IsEnabled = false;
                        dateChB.IsEnabled = true;
                        expChB.IsEnabled = false;
                        DatePickers.IsEnabled = !dateChB.IsChecked.Value;
                        addBtn.IsEnabled = false;
                        break;
                    case 2:
                        itemCountLbl.Content = DeviceData.infoExc;
                        hideCheck.IsEnabled = false;
                        dateChB.IsEnabled = false;
                        expChB.IsEnabled = false;
                        DatePickers.IsEnabled = false;
                        addBtn.IsEnabled = false;
                        break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void mainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BottomUpdate();
        }

        private void dateChB_Checked(object sender, RoutedEventArgs e)
        {
            DatePickers.IsEnabled = !dateChB.IsChecked.Value;
        }

        private void userBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID == 3)
                {
                    umw = new UserManagmentWindow();
                    umw.ShowDialog();
                }
                else MessageBox.Show("Для данной функции необходимо иметь доступ 'Администратор'");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void journalBtn_Click(object sender, RoutedEventArgs e)
        {
            histWindow = new HistWindow();
            histWindow.ShowDialog();
        }

        private void infoBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("nMetr - программа предназначенная для ведения журнала приборов, отслеживания их срока годности и составления журнала ППР.\nРазработчик: Асонов Г.С.\nДанная находится на стадии разработки и не отображает коечный результат.", "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void signOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы хотите выйти из текущей учётной записи?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) logIn();
        }

        private void cRec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    List<Device> devices = new List<Device>();
                    foreach (DeviceData d in excGrid.SelectedItems)
                    {
                        devices.Add(context.Device.Where(dev => dev.Device_ID == d.ID).FirstOrDefault());
                    }
                    DeviceData.deviceRec(devices, context, CurrentUser.user.User_ID);
                }
                else MessageBox.Show("Для восстановления необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void pprGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    DeviceData device = e.Row.Item as DeviceData;
                    Device dev = context.Device.Where(d => d.Device_ID == device.ID).FirstOrDefault();
                    DeviceData.DeviceEdit(dev, device.Name, device.ObjectName, device.FNum, device.Param, device.MetrData, device.ExpDate, int.Parse(device.Period), device.Note, CurrentUser.user.User_ID);
                }
                else MessageBox.Show("Для редактирования необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void excGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                if (CurrentUser.user.RoleID >= 2)
                {
                    DeviceData device = e.Row.Item as DeviceData;
                    Device dev = context.Device.Where(d => d.Device_ID == device.ID).FirstOrDefault();
                    DeviceData.DeviceEdit(dev, device.Name, device.ObjectName, device.FNum, device.Param, device.MetrData, device.ExpDate, int.Parse(device.Period), device.Note, CurrentUser.user.User_ID);
                }
                else MessageBox.Show("Для редактирования необходимо иметь роль 'Пользователь' или выше");
                Thread thread = new Thread(UpdateTabs) { IsBackground = true };
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddObjBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(searchTBObj.Text) && !objects.Contains(searchTBObj.Text))
                objects.Add(searchTBObj.Text);

            ObjListView.ItemsSource = null;
            ObjListView.ItemsSource = objects;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                objects.RemoveAt(ObjListView.SelectedIndex);

                ObjListView.ItemsSource = null;
                ObjListView.ItemsSource = objects;
            }
            catch { }
        }


        private void searchTBObjItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            objectsUpdate((sender as TextBlock).Text);
            e.Handled = true;
        }

        void objectsUpdate(string ObjName)
        {
            try
            {
                objTemp = ObjName;
                if (objTemp != "" && !objects.Contains(objTemp) && objNames.Contains(objTemp))
                {
                    objects.Add(objNames.FirstOrDefault(o => o == objTemp));

                    objTemp = "";
                    ObjListView.ItemsSource = null;
                    ObjListView.ItemsSource = objects;

                }
            }
            catch
            {

            }
        }


        private void expBtn_Click(object sender, RoutedEventArgs e)
        {
            exWin = new ExportWindow();
            exWin.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (searchTBObj.Text != "")
                {
                    objectsUpdate(searchTBObj.Text);
                    searchTBObj.Text = "";
                    e.Handled = true;
                }

                else if (mainTab.IsFocused)
                {
                    Thread thread = new Thread(startSearch);
                    thread.Start();
                }

                Control s = e.Source as Control;
                if (s != null)
                {
                    s.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }

            }
        }

        private void cExp_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceData> list = new List<DeviceData>();
            foreach (DeviceData d in deviceGrid.SelectedItems)
            {
                list.Add(d);
            }
            EClass.Export(EClass.Presets[2], list);
        }

        private void cHist_Click(object sender, RoutedEventArgs e)
        {
            HistWindow histWindow = new HistWindow((deviceGrid.SelectedItems.Count > 0 ? deviceGrid.SelectedItems[0] as DeviceData : excGrid.SelectedItems[0] as DeviceData).ID);
            histWindow.Show();
        }

        private void chUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentUser.user.User_ID == 1)
            {
                MessageBox.Show("Нельзя редактировать учётную запись 'Гость'","Предупреждение",MessageBoxButton.OK,MessageBoxImage.Stop);
                return;
            }
            RegWindow regWindow = new RegWindow(3);
            regWindow.ShowDialog();
        }
    }
}

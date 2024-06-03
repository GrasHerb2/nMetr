using DocumentFormat.OpenXml.Wordprocessing;
using Metr.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Metr._Windows
{
    /// <summary>
    /// Логика взаимодействия для ExcelExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        List<Classes.Column> Columns = new List<Classes.Column>();
        EClass localSettings = new EClass();
        public ExportWindow()
        {
            InitializeComponent();
            EClass.UpdPresets();

            UpdateSettings(EClass.Presets[0]);
            TableTypeCmB.SelectedIndex = 0;
        }
        void UpdateSettings(EClass a)
        {
            TableTypeCmB.ItemsSource = EClass.Presets.Select(p => p.Name).ToList();

            SearchUseChB.IsChecked = a.Settings[0] == 1 ? true : false;
            OriginTableCmB.SelectedIndex = a.Settings[1];
            ObjSortChB.IsChecked = a.Settings[2] == 1 ? true : false;

            ColumnsDG.ItemsSource = null;
            Columns.Clear();
            for (int i = 0; i < a.CHeader.Count; i++)
                Columns.Add(new Classes.Column() { Header = a.CHeader[i], Field = a.Field[i] });
            ColumnsDG.ItemsSource = Columns;
        }

        private void TableTypeCmB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSettings(EClass.Presets[TableTypeCmB.SelectedIndex]);
        }

        void exporting()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                loadingProgress.Visibility = Visibility.Visible;
                this.IsEnabled = false;
                localSettings = EClass.Presets[TableTypeCmB.SelectedIndex];
                localSettings.CHeader = Columns.Select(p => p.Header).ToList();
                localSettings.Field = Columns.Select(p => p.Field).ToList();
                localSettings.Settings = new List<int> { SearchUseChB.IsChecked.Value ? 1 : 0, OriginTableCmB.SelectedIndex, ObjSortChB.IsChecked.Value ? 1 : 0 };
            }));
            EClass.Export(localSettings);
            Dispatcher.Invoke(new Action(() =>
            {
                this.IsEnabled = true;
                loadingProgress.Visibility = Visibility.Collapsed;
            }));
        }

        private void expBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Thread thread = new Thread(exporting);
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            TextWin textWin = new TextWin("Название набора настроек", "Экспорт");
            textWin.ShowDialog();
            if (!textWin.DialogResult.Value)
            {
                MessageBox.Show("Сохранение отменено", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            localSettings.Name = textWin.textOut;
            localSettings.CHeader = Columns.Select(p => p.Header).ToList();
            localSettings.Field = Columns.Select(p => p.Field).ToList();
            localSettings.Settings = new List<int> { SearchUseChB.IsChecked.Value ? 1 : 0, OriginTableCmB.SelectedIndex, ObjSortChB.IsChecked.Value ? 1 : 0 };

            SettingsClass.SavePreset(localSettings);
            UpdateSettings(localSettings);
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TableTypeCmB.SelectedIndex > 4)
            {
                int a = TableTypeCmB.SelectedIndex;
                SettingsClass.DelPreset(EClass.Presets[a].Name);

                TableTypeCmB.SelectedIndex = 0;
            }
            else
                e.Handled = true;
        }
    }

    public class ComboBoxSelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                return EClass.converted[(int)value < EClass.converted.Count ? (int)value : 0];
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

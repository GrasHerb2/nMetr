using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Metr.Classes
{
    class JsonObject
    {
        public string Name { get; set; }
        public List<DeviceData> devices1 { get; set; }
    }
    class Column
    {
        public string Header { get; set; }
        public int Field { get; set; }
    }
    public class EClass
    {
        public static List<string> converted { get; } = new List<string>()
        {
            "Пусто",
            "Порядковый номер",
            "Название прибора",
            "Заводской номер",
            "Объект",
            "Измеряемый параметр",
            "Единицы измерения",
            "МП (Срок годности)",
            "ППР (Только текущий месяц)",
            "МП/ППР 1",
            "ППР 1",
            "ППР 2",
            "ППР 3",
            "ППР 4",
            "Период ППР",
            "Примечания"
        };


        public string Name { get; set; }
        public List<string> CHeader { get; set; }
        public List<int> Field { get; set; }
        public List<int> Settings { get; set; }

        public static List<EClass> Presets { get; set; }

        public static void UpdPresets()
        {
            Presets = new List<EClass>();
            Presets.AddRange(new List<EClass>()
            {
            new EClass()
            {
                Name = "ППР на год",
                CHeader = new List<string> { "Объект", "Название", "Метрологические данные", "Заводской номер", "Измеряемый параметр", "МП/ППР1", "ППР2", "ППР3", "ППР4" },
                Field = new List<int> { 4, 2, 6, 3, 5, 9, 11, 12, 13 },
                Settings = new List<int> { 0, 2, 1 }
            },

            new EClass()
            {
                Name = "ППР на месяц",
                CHeader = new List<string> { "Объект", "Название", "Метрологические данные", "Заводской номер", "Измеряемый параметр", "Дата" },
                Field = new List<int> { 4, 2, 6, 3, 5, 8 },
                Settings = new List<int> { 0, 2, 0 }
            },

            new EClass()
            {
                Name = "График поверки/калибровки",
                CHeader = new List<string> { "Название", "Метрологические данные", "Заводской номер", "Годен до", "Примечание", "Измеряемый параметр" },
                Field = new List<int> { 2, 6, 3, 7, 15, 5 },
                Settings = new List<int> { 0, 1, 0 }
            },

            new EClass()
            {
                Name = "Журнал сдачи приборов",
                CHeader = new List<string> { "Объект", "Название", "Измеряемый параметр", "Метрологические данные", "Заводской номер", "Годен до", "Дата отправки", "Годен до" },
                Field = new List<int> { 4, 2, 5, 6, 3, 7, 0, 0 },
                Settings = new List<int> { 0, 1, 0 }
            },
            new EClass()
            {
                Name = "Иной",
                CHeader = new List<string> { "Введите заголовок" },
                Field = new List<int> { 0 },
                Settings = new List<int> { 0, 1, 0 }
            }
            });
            try
            {
                Presets.AddRange(SettingsClass.EPresets);
            }
            catch
            {

            }
        }
        public static void Export(EClass settings, List<DeviceData> pData = null)
        {


            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel (*.xlsx)|*.xlsx|Txt|*.txt|Json|*.json";



            List<DeviceData> devs = new List<DeviceData>();
            if (pData != null)
            {
                devs = pData;
            }
            else
            {

                if (settings.Settings[0] == 0)
                {
                    DeviceData.Search(new List<string>() { "", "" }, new List<string>() { }, DateTime.MinValue, DateTime.MaxValue, false, false, settings.Field.Contains(8));
                }

                switch (settings.Settings[1])
                {
                    case 0:
                        devs.AddRange(DeviceData.deviceListMain);
                        devs.AddRange(DeviceData.deviceListPPR);
                        devs.AddRange(DeviceData.deviceListExc);
                        devs = devs.Distinct().ToList();
                        break;
                    case 1: devs = DeviceData.deviceListMain; break;
                    case 2: devs = DeviceData.deviceListPPR; break;
                    case 3: devs = DeviceData.deviceListExc; break;
                }
            }

            if (!settings.Field.Contains(8) ||
                !settings.Field.Contains(9) ||
                !settings.Field.Contains(10) ||
                !settings.Field.Contains(11) ||
                !settings.Field.Contains(12) ||
                !settings.Field.Contains(13))
            {
                devs = devs.Where(d => d.FNum != "Н/Д").ToList();
            }

            devs = devs.OrderByDescending(x => x.ObjectName).ThenBy(x => x.Name).ThenBy(x => x.ExpDate).ToList();
            if (settings.Field.Contains(8)) devs = devs.OrderBy(x => x.pprMonthDate).ToList();

            string save = "";

            if (saveFileDialog.ShowDialog().Value)
            {
                if (saveFileDialog.FileName.Contains(".txt"))
                {


                    if (settings.Settings[2] == 1)
                    {
                        foreach (List<DeviceData> devices in devs.GroupBy(d => d.ObjectName).Select(grp => grp.ToList()))
                        {

                            save = Path.GetDirectoryName(saveFileDialog.FileName) + "\\import\\";
                            Directory.CreateDirectory(save);

                            string fName = devices[0].ObjectName;

                            string content = "";

                            for (int h = 0; h < settings.CHeader.Count; h++)
                            {
                                content = settings.CHeader[h];
                            }

                            content += "\n";

                            for (int i = 0; i < devices.Count; i++)
                            {
                                content += DeviceData.DevString(settings.Field, devices[i]);
                                content += "\n";
                            }

                            File.WriteAllText(save + fName + ".txt", content);
                        }
                    }
                    else
                    {


                        string content = "";
                        for (int h = 0; h < settings.CHeader.Count; h++)
                        {
                            content = settings.CHeader[h];
                        }

                        for (int i = 0; i < devs.Count; i++)
                        {
                            content += DeviceData.DevString(settings.Field, devs[i]);
                            content += "\n";
                        }
                        File.WriteAllText(saveFileDialog.FileName, content);
                    }
                }

                if (saveFileDialog.FileName.Contains(".json"))
                {
                    string text = "";
                    if (settings.Settings[2] == 1)
                    {
                        foreach (List<DeviceData> devices in devs.GroupBy(d => d.ObjectName).Select(grp => grp.ToList()))
                        {
                            text = JsonSerializer.Serialize(new JsonObject() { Name = devices[0].ObjectName, devices1 = devices });
                        }
                    }
                    else
                    {
                        text = JsonSerializer.Serialize(devs);
                    }


                    File.WriteAllText(saveFileDialog.FileName, text);
                }

                if (saveFileDialog.FileName.Contains(".xlsx"))
                {
                    try
                    {
                        SLDocument sl = new SLDocument();
                        SLStyle style = new SLStyle();

                        string exportString = "";

                        if (settings.Settings[2] == 1)
                        {
                            foreach (List<DeviceData> devices in devs.GroupBy(d => d.ObjectName).Select(grp => grp.ToList()))
                            {

                                sl.AddWorksheet(devices[0].ObjectName.Length > 30 ? devices[0].ObjectName.Remove(30) : devices[0].ObjectName);
                                style = sl.GetCellStyle(1, 1);
                                style.Alignment.Horizontal = HorizontalAlignmentValues.Center;
                                style.Alignment.Vertical = VerticalAlignmentValues.Center;

                                sl.SetCellValue(1, 1, devices[0].ObjectName);
                                sl.MergeWorksheetCells(1, 1, 1, settings.Field.Count());

                                sl.SetCellStyle(1, 1, style);

                                for (int h = 0; h < settings.CHeader.Count; h++)
                                {
                                    sl.SetCellValue(2, h + 1, settings.CHeader[h]);

                                    style = sl.GetCellStyle(2, h + 1);
                                    style.Border.Outline = true;

                                    style.Border.SetLeftBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                    style.Border.SetRightBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                    style.Border.SetTopBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                    style.Border.SetBottomBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);

                                    sl.SetCellStyle(2, h + 1, style);
                                }

                                for (int i = 0; i < devices.Count; i++)
                                {
                                    exportString = DeviceData.DevString(settings.Field, devices[i]);
                                    for (int j = 0; j < exportString.Split('\t').Count() - 1; j++)
                                    {
                                        sl.SetCellValue(i + 3, j + 1, exportString.Split('\t')[j]);

                                        style = sl.GetCellStyle(i + 3, j + 1);
                                        style.Border.Outline = true;

                                        style.Border.SetLeftBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                        style.Border.SetRightBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                        style.Border.SetTopBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                        style.Border.SetBottomBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);

                                        sl.SetCellStyle(i + 3, j + 1, style);
                                    }
                                }
                            }

                            sl.DeleteWorksheet(sl.GetWorksheetNames()[0]);
                        }
                        else
                        {
                            sl.RenameWorksheet(sl.GetWorksheetNames()[0], "Экспорт");
                            for (int h = 0; h < settings.CHeader.Count; h++)
                            {
                                sl.SetCellValue(2, h + 1, settings.CHeader[h]);

                                style = sl.GetCellStyle(2, h + 1);
                                style.Border.Outline = true;

                                style.Border.SetLeftBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                style.Border.SetRightBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                style.Border.SetTopBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                style.Border.SetBottomBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);

                                sl.SetCellStyle(2, h + 1, style);
                            }

                            for (int i = 0; i < devs.Count; i++)
                            {
                                exportString = DeviceData.DevString(settings.Field, devs[i]);
                                for (int j = 0; j < exportString.Split('\t').Count(); j++)
                                {
                                    sl.SetCellValue(i + 3, j + 1, exportString.Split('\t')[j]);

                                    style = sl.GetCellStyle(i + 3, j + 1);
                                    style.Border.Outline = true;

                                    style.Border.SetLeftBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                    style.Border.SetRightBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                    style.Border.SetTopBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);
                                    style.Border.SetBottomBorder(BorderStyleValues.Thin, System.Drawing.Color.Black);

                                    style.SetWrapText(true);
                                    sl.SetCellStyle(i + 3, j + 1, style);
                                }

                                sl.AutoFitColumn(1, i + 3, 20);
                            }
                        }
                        sl.SaveAs(saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {

                    }

                }

            }
        }
    }
}

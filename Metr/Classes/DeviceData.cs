using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Metr.Classes
{
    public class DeviceData
    {        
        public int ID { get; set; }
        public string FNum { get; set; }
        public string Name { get; set; }
        public string MetrData { get; set; }
        public int ObjectID { get; set; }
        public string ObjectName { get; set; }
        public string Param { get; set; }
        public string Note { get; set; }
        public string updateDate { get; set; }
        public DateTime? ExpDate { get; set; }
        public DateTime? pprDate1 { get; set; }
        public DateTime? pprDate2 { get; set; }
        public DateTime? pprDate3 { get; set; }
        public DateTime? pprDate4 { get; set; }
        public DateTime? pprMonthDate { get; set; }
        public bool Hidden { get; set; }
        public bool Delete { get; set; }
        public bool PPR { get; set; }
        public string Stroke { get; set; }
        public string Period { get; set; }

        //Далее общие параметры и методы для класса DeviceData
        /// <summary>
        /// Содержит все приборы в БД
        /// </summary>
        public static List<DeviceData> deviceList = new List<DeviceData>();

        /// <summary>
        /// Содержит все не исключённые приборы в БД
        /// </summary>
        public static List<DeviceData> deviceListMain = new List<DeviceData>();

        /// <summary>
        /// Содержит приборы для ППР
        /// </summary>
        public static List<DeviceData> deviceListPPR = new List<DeviceData>();

        /// <summary>
        /// Содержит все исключённые приборы в БД
        /// </summary>
        public static List<DeviceData> deviceListExc = new List<DeviceData>();


        /// <summary>
        /// Строка результата главной таблицы
        /// </summary>
        public static string infoMain { get; set; }
        /// <summary>
        /// Строка результата таблицы ППР
        /// </summary>
        public static string infoPPR { get; set; }
        /// <summary>
        /// Строка результата таблицы исключённых приборов
        /// </summary>
        public static string infoExc { get; set; }


        /// <summary>
        /// Список действий для записи в БД последнего использовавшегося метода
        /// </summary>
        public static List<Operation> operations = new List<Operation>();

        /// <summary>
        /// Список для класса Device в БД
        /// </summary>
        public static List<Device> devices = new List<Device>();

        //Далее методы для взаимодействия со списками DeviceData

        /// <summary>
        /// Метод для обновления набора данных.
        /// </summary>
        /// <param name="pprYear">Если 0, приравнивается к текущему году. По умолчанию = 0 (выбирает год текущей даты)</param>
        /// <returns>Возвращает deviceList содержащий все приборы в БД</returns>
        public static List<DeviceData> dataUpdate(int pprYear = 0)
        {
            devices =  MetrBaseEn.GetContext().Device.ToList();
            deviceList.Clear();
            string note = "";
            foreach (Device d in devices)
            {
                note += d.NoteText;
                deviceList.Add(new DeviceData()
                {
                    ID = d.Device_ID,
                    ExpDate = d.ExpDate,
                    Period = d.PPR_Period.ToString(),
                    Stroke = "None",
                    MetrData = d.MetrData,
                    FNum = d.FNum,
                    Name = d.Name,
                    ObjectID = d.Object.Object_ID,
                    ObjectName = d.Object.Name,
                    Param = d.Param,
                    Note = d.NoteText,

                    Hidden = d.Hidden.Value,
                    Delete = d.Removed.Value,
                    PPR = !d.PPR_Removed.Value
                });
                //При ExpDate не null рассчитывает месяца ППР
                if (deviceList.Last().ExpDate != null)
                {
                    deviceList.Last().pprDate1 = new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month, 1);
                    deviceList.Last().pprDate2 = (d.ExpDate.Value.Month + 3 <= 12 ? new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month + 3, 1) : new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month + 3 - 12, 1));
                    deviceList.Last().pprDate3 = (d.ExpDate.Value.Month + 6 <= 12 ? new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month + 6, 1) : new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month + 6 - 12, 1));
                    deviceList.Last().pprDate4 = (d.ExpDate.Value.Month + 9 <= 12 ? new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month + 9, 1) : new DateTime(DateTime.Now.Year, d.ExpDate.Value.Month + 9 - 12, 1));
                }
                note = "";
            }
            infoMain = "Приборы:" + deviceList.Count + " из " + devices.Count();

            //Здесь для заполнения листов выводящихся в таблицы
            Search(new List<string>() { "", "" }, new List<string>() { }, DateTime.MinValue, DateTime.MaxValue, false, false);

            return deviceList;
        }
        /// <summary>
        /// Метод поиска приборов в БД
        /// </summary>
        /// <param name="dSearch">Номер, название</param>
        /// <param name="objects">Объекты</param>
        /// <param name="searchStart">Дата начального срока годности</param>
        /// <param name="searchEnd">Дата крайнего срока годности</param>
        /// <param name="Hid">Отображение приборов скрытых из основного списка</param>
        /// <param name="Del">Отображение исключённых приборов</param>
        /// <param name="pprDate">(Для ППР) Отображение только с ППР текущего месяца</param>
        /// <param name="Exp">(Для Основного) Выделение даты просроченых приборов</param>
        /// <returns>Возвращает deviceList</returns>
        public static List<DeviceData> Search(List<string> dSearch, List<string> objects, DateTime searchStart, DateTime searchEnd, bool Hid, bool Del, bool pprDate = false, bool Exp = false)
        {
            deviceListMain = deviceList;
            int total = deviceList.Count;

            deviceListExc = deviceListMain.Where(d =>
                d.Delete
                ).ToList();

            infoExc = " из " + deviceListExc.Count();

            deviceListPPR = deviceListMain.Where(d => d.PPR && !d.Hidden && !d.Delete && d.ExpDate != null).ToList();

            infoPPR = " из " + deviceListPPR.Count();

            if (dSearch[0] != "")
            {
                deviceListMain = deviceListMain.Where(d =>
                    d.FNum.ToLower().Contains(dSearch[0])
                    ).ToList();

                deviceListPPR = deviceListPPR.Where(d =>
                    d.FNum.ToLower().Contains(dSearch[0])
                    ).ToList();

                deviceListExc = deviceListExc.Where(d =>
                    d.FNum.ToLower().Contains(dSearch[0])
                    ).ToList();
            }


            if (objects.Count != 0)
            {
                deviceListMain = deviceListMain.Where(d =>
                objects.Any(a => a == d.ObjectName)
                ).ToList();

                deviceListPPR = deviceListPPR.Where(d =>
                objects.Any(a => a == d.ObjectName)
                ).ToList();

                deviceListExc = deviceListExc.Where(d =>
                objects.Any(a => a == d.ObjectName)
                ).ToList();
            }

            string expInfo = "";
            int expCount = 0;
            if (Exp)
            {
                foreach (DeviceData a in deviceListMain)
                {
                    a.Stroke = a.ExpDate != null ? (DateTime.Compare(a.ExpDate.Value, DateTime.Now) < 0 ? "Underline" : "None") : "None";
                    expCount += a.ExpDate != null ? (DateTime.Compare(a.ExpDate.Value, DateTime.Now) < 0 ? 1 : 0) : 0;
                    expInfo = expCount + " просроченных приборов";
                }
            }

            deviceListMain = deviceListMain.Where(d =>
            d.Name.ToLower().Contains(dSearch[1])
            ).ToList();

            deviceListPPR = deviceListPPR.Where(d =>
            d.Name.ToLower().Contains(dSearch[1])
            ).ToList();

            deviceListExc = deviceListExc.Where(d =>
            d.Name.ToLower().Contains(dSearch[1])
            ).ToList();
            //Конец поиска исключённых








            if (searchStart != new DateTime())
            {
                deviceListMain = deviceListMain.Where(d =>
                    d.ExpDate == null ||
                    DateTime.Compare(d.ExpDate.Value, searchStart) >= 0
                    ).ToList();
                deviceListPPR = !pprDate ? deviceListPPR.Where(d =>
                    DateTime.Compare(d.pprDate1.Value, searchStart) >= 0 ||
                    DateTime.Compare(d.pprDate2.Value, searchStart) >= 0 ||
                    DateTime.Compare(d.pprDate3.Value, searchStart) >= 0 ||
                    DateTime.Compare(d.pprDate4.Value, searchStart) >= 0
                    ).ToList() : deviceListPPR;
            }

            if (searchEnd != new DateTime())
            {
                deviceListMain = deviceListMain.Where(d =>
                    d.ExpDate == null ||
                    DateTime.Compare(d.ExpDate.Value, searchEnd) <= 0
                    ).ToList();
                deviceListPPR = !pprDate ? deviceListPPR.Where(d =>
                    DateTime.Compare(d.pprDate1.Value, searchEnd) <= 0 ||
                    DateTime.Compare(d.pprDate2.Value, searchEnd) <= 0 ||
                    DateTime.Compare(d.pprDate3.Value, searchEnd) <= 0 ||
                    DateTime.Compare(d.pprDate4.Value, searchEnd) <= 0
                    ).ToList() : deviceListPPR;
            }

            deviceListPPR.OrderBy(d => d.ExpDate);

            if (pprDate)
            {
                deviceListPPR = deviceListPPR.Where(d =>
                (d.pprDate1.Value.Month == DateTime.Now.Month) ||
                (d.pprDate2.Value.Month == DateTime.Now.Month) ||
                (d.pprDate3.Value.Month == DateTime.Now.Month) ||
                (d.pprDate4.Value.Month == DateTime.Now.Month)
                ).ToList();

                List<int> WorkingDays = new List<int>();
                List<int> DeviceDays = new List<int>();

                int NowDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

                DayOfWeek dayOfWeek;

                for (int i = 1; i <= NowDaysInMonth; i++)
                {
                    dayOfWeek = new DateTime(DateTime.Now.Year, DateTime.Now.Month, i).DayOfWeek;
                    if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
                    {
                        WorkingDays.Add(i);
                    }
                }

                int DayInterval = (int)Math.Ceiling((double)deviceListPPR.Count / (double)WorkingDays.Count);
                foreach (int d in WorkingDays)
                    for (int i = 0; i < DayInterval; i++) DeviceDays.Add(d);

                for (int i = 0; i < deviceListPPR.Count; i++)
                {
                    deviceListPPR[i].pprMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DeviceDays[i]);
                }

                deviceListPPR.OrderBy(d => d.ObjectName);
            }

            if (!Hid)
                deviceListMain = deviceListMain.Where(d =>
                !d.Hidden
                ).ToList();

            if (Del)
                deviceListMain = deviceListMain.Where(d =>
                d.Delete
                ).ToList();
            else
                deviceListMain = deviceListMain.Where(d =>
                !d.Delete
                ).ToList();

            deviceListMain = deviceListMain.OrderBy(d => d.ExpDate).ToList();
            infoMain = "Приборы:" + deviceListMain.Count + " из " + total + (Exp ? "\n" + expInfo : "");
            infoPPR = "Приборы: " + deviceListPPR.Count() + infoPPR;
            infoExc = "Приборы: " + deviceListExc.Count() + infoExc;


            deviceListMain = Exp ? deviceListMain.OrderByDescending(d => d.Stroke).ToList() : deviceListMain;

            return deviceListMain;

        }
        /// <summary>
        /// Метод добавления нового прибора
        /// </summary>
        /// <param name="Name">Название</param>
        /// <param name="ObjectName">Название объекта</param>
        /// <param name="FNum">Заводской номер</param>
        /// <param name="Param">Измеряемый параметр </param>
        /// <param name="MetrData">Единицы измерения </param>
        /// <param name="ExpDate">Срок годности </param>
        /// <param name="Period">Период прохождения ППР </param>
        /// <param name="NoteText">Примечания </param>
        /// <param name="noPPR">Отслеживание ППР </param>
        /// <param name="user">Пользоваетль добавляющий прибор</param>
        /// <returns>Возвращает MessageBoxResult где: Yes - добавление подтверждено, No - добавление отмененно пользователем, Cancel - добавление отменено по иным причинам, None - добавление отменено системой</returns>
        public static MessageBoxResult NewDevice(string Name, string ObjectName, string FNum, string Param, string MetrData, DateTime? ExpDate, int Period, string NoteText, bool noPPR, int user)
        {
            MetrBaseEn context = MetrBaseEn.GetContext();

            operations.Clear();

            int tempId = 0;

            string log = "";

            int objId = -1;

            if (context.Object.Where(o => o.Name.ToLower() == ObjectName.ToLower()).Count() != 0)
                objId = context.Object.Where(o => o.Name.ToLower() == ObjectName.ToLower()).FirstOrDefault().Object_ID;

            if (objId == -1)
                if (MessageBox.Show("В базе данных не был найден объект \"" + ObjectName + "\".\n Добавить новый объект в базу данных?", "Добавление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    context.Object.Add(new Object() { Name = ObjectName });
                    operations.Add(new Operation() { UserID = user, OperationDate = DateTime.Now, OperationText = "Добавление объекта " + ObjectName, ComputerName = Environment.MachineName, ID_Status = 1, ID_Type = 3});
                    context.SaveChanges();
                }
                else
                    return MessageBoxResult.Cancel;
            if (FNum + "" == "") FNum = "Н/Д";

            log = "Номер:" + FNum + "\nНазвание:" + Name + "\nИзмеряемый параметр:" + Param + "\nЕдиницы измерения:" + MetrData + "\nОбъект:" + ObjectName + "\nСрок годности:" + ExpDate + "\nДоп. информация:" + NoteText;


            switch (MessageBox.Show("Вы хотите добавить:\n\"" + log + "\"?", "Добавление", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    Device device = new Device()
                    {
                        FNum = FNum,
                        Name = Name,
                        IDObject = context.Object.Where(o => o.Name == ObjectName).FirstOrDefault().Object_ID,
                        Param = Param,
                        MetrData = MetrData,
                        ExpDate = ExpDate,
                        NoteText = NoteText,
                        Hidden = false,
                        Removed = false,
                        PPR_Removed = noPPR,
                        PPR_Period = Period                     
                    };
                    context.Device.Add(device);
                    context.SaveChanges();
                    tempId = context.Device.Last().Device_ID;
                    context.Operation.Add(new Operation() { UserID = user, OperationDate = DateTime.Now, OperationText = "Добавление прибора\n" + log, ComputerName = Environment.MachineName, ID_Status = 1, ID_Type = 3, ID_Device = tempId });
                    context.SaveChanges();
                    return MessageBoxResult.Yes;

                case MessageBoxResult.No:
                    return MessageBoxResult.No;

                case MessageBoxResult.Cancel:
                    return MessageBoxResult.Cancel;
            }
            return MessageBoxResult.None;
        }
        /// <summary>
        /// Метод изменения прибора
        /// </summary>
        /// <param name="dev">Начальный прибор класса Device</param>
        /// <param name="Name">Название</param>
        /// <param name="ObjectName">Название объекта</param>
        /// <param name="FNum">Заводской номер</param>
        /// <param name="Param">Измеряемый параметр</param>
        /// <param name="MetrData">Единицы измерения</param>
        /// <param name="ExpDate">Срок годности</param>
        /// <param name="Period">Период прохождения ППР в месяцах</param>
        /// <param name="NoteText">Примечание</param>
        /// <param name="user">Пользователь, производящий изменение</param>
        /// <param name="noPPR">Прохождение ППР</param>
        /// <returns>Возвращает MessageBoxResult где: Yes - изменение подтверждено, No - изменение отмененно пользователем, Cancel - изменение отменено по иным причинам, None - изменение отменено системой</returns>
        public static MessageBoxResult DeviceEdit(Device dev, string Name, string ObjectName, string FNum, string Param, string MetrData, DateTime? ExpDate, int Period, string NoteText, int user, bool? noPPR = null)
        {
            MetrBaseEn context = MetrBaseEn.GetContext();
            
            operations.Clear();

            dev.NoteText = !string.IsNullOrEmpty(dev.NoteText) ? dev.NoteText : "";

            int objId = -1;

            string log = "";

            if (context.Object.Where(o => o.Name.ToLower() == ObjectName).Count() != 0)
                objId = context.Object.Where(o => o.Name.ToLower() == ObjectName).FirstOrDefault().Object_ID;

            if (objId == -1)
                if (MessageBox.Show("В базе данных не был найден объект \"" + ObjectName + "\".\n Добавить новый объект в базу данных?", "Добавление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    context.Object.Add(new Object() { Name = ObjectName });
                    context.SaveChanges();
                    operations.Add(new Operation() { UserID = user, OperationDate = DateTime.Now, OperationText = "Добавление объекта " + ObjectName, ComputerName = Environment.MachineName, ID_Status = 1, ID_Type = 3  });
                }
                else
                    return MessageBoxResult.Cancel;
            
            log += dev.PPR_Period.Value == Period ? "" : dev.PPR_Period + "->" + Period + "\n";
            if (noPPR != null)
            log += dev.PPR_Removed.Value && !noPPR.Value ? "\n>ППР: Включён\n" : !dev.PPR_Removed.Value && noPPR.Value ? "\nППР: Исключён\n" : "";       

            if (FNum + "" == "") FNum = "Н/Д";

            log +=
                (Name + "" != dev.Name.ToString() + "" && !(string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(dev.Name)) ? ">Название\n" + dev.Name + "->" + Name + "\n" : "") +

                (FNum + "" != dev.FNum + "" && !(string.IsNullOrEmpty(FNum) && string.IsNullOrEmpty(dev.FNum)) ? ">Номер\n" + dev.FNum + "->" + FNum + "\n" : "") +

                (ObjectName + "" != dev.Object.Name + "" && !(string.IsNullOrEmpty(ObjectName) && string.IsNullOrEmpty(dev.Object.Name)) ? ">Объект\n" + dev.Object.Name + "->" + ObjectName : "") +

                (Param + "" != dev.Param + "" && !(string.IsNullOrEmpty(Param) && string.IsNullOrEmpty(dev.Param)) ? ">Измеряемый параметр\n" + dev.Param + "->" + Param + "\n" : "") +

                (MetrData + "" != dev.MetrData + "" && !(string.IsNullOrEmpty(MetrData) && string.IsNullOrEmpty(dev.MetrData)) ? ">Единицы измерения\n" + dev.MetrData + "->" + MetrData + "\n" : "") +

                (NoteText + "" != (dev.NoteText + "").Split(':')[0] && !(string.IsNullOrEmpty(NoteText) && string.IsNullOrEmpty(dev.NoteText)) ? ">Примечание\n" + dev.NoteText.Split(':')[0] + "" + "->" + NoteText + "\n" : "") +

                (ExpDate.ToString() + "" != dev.ExpDate.ToString() + "" &&
                !(string.IsNullOrEmpty(ExpDate.ToString()) && string.IsNullOrEmpty(dev.ExpDate.ToString())) ?

                "Срок годности\n" + dev.ExpDate + "->" + ExpDate
                : "") +

                "\n"
                ;

            if (log == "\n") return MessageBoxResult.None;

            switch (MessageBox.Show("Будут проведены следующие изменения:\n" + log + "Сохранить?", "Изменение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    dev = context.Device.Where(d => d.Device_ID == dev.Device_ID).FirstOrDefault();

                    dev.FNum = FNum;
                    dev.ExpDate = ExpDate;
                    dev.MetrData = MetrData;
                    dev.Name = Name;
                    dev.NoteText = NoteText;
                    dev.Param = Param;
                    dev.IDObject = context.Object.Where(o => o.Name == ObjectName).FirstOrDefault().Object_ID;
                    dev.PPR_Period = Period;

                    context.SaveChanges();
                    dev = context.Device.Where(d => d.Device_ID == dev.Device_ID).Single();
                    operations.Add(new Operation() { UserID = user, OperationDate = DateTime.Now, OperationText = "Изменение прибора " + dev.Name + "\n" + log, ComputerName = Environment.MachineName, ID_Status = 1, ID_Type = 8, ID_Device = dev.Device_ID });

                    context.Operation.AddRange(operations);
                    context.SaveChanges();
                    return MessageBoxResult.Yes;
                case MessageBoxResult.No:
                    return MessageBoxResult.No;
                case MessageBoxResult.Cancel:
                    return MessageBoxResult.Cancel;
            }
            return MessageBoxResult.None;
        }


        /// <summary>
        /// Метод скрытия прибора
        /// </summary>
        /// <param name="devices">Скрываемые приборы</param>
        /// <param name="context">Контекст БД</param>
        /// <param name="user">Пользователь, скрывающий приборы</param>
        public static void deviceHide(List<Device> devices, MetrBaseEn context, int user)
        {
            List<Device> devs = new List<Device>();
            string devstring = "";
            if (devices.Count() != 0)
            {
                foreach (Device d in devices)
                {
                        devstring += "\n" + d.Name + " " + d.FNum;
                        devs.Add(d);
                }

                if (MessageBox.Show("Будут скрыты следующие приборы:" + devstring + "\nПрименить?\n При скрытии данные приборы можно будет найти включив 'Скрытые' в критериях поиска.", "Изменение видимости", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (Device d in devs)
                    {
                        d.Hidden = true;
                        d.NoteText +=" \n'Скрыт'";
                        context.Operation.Add(new Operation() { 
                            UserID = user, 
                            OperationDate = DateTime.Now, 
                            OperationText = "Скрытие прибора " + d.Name + " " + d.FNum, 
                            ComputerName = Environment.MachineName,
                            ID_Type = 9,
                            ID_Status = 1,
                            ID_Device = d.Device_ID
                        });
                        context.SaveChanges();
                    }
                    MessageBox.Show("Приборы скрыты", "Изменение видимости", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Метод расскрытия прибора
        /// </summary>
        /// <param name="devices">Расскрываемые приборы</param>
        /// <param name="context">Контекст БД</param>
        /// <param name="user">Пользователь, расскрывающий приборы</param>
        public static void deviceUnHide(List<Device> devices, MetrBaseEn context, int user)
        {
            List<Device> devs = new List<Device>();
            string devstring = "";
            if (devices.Count() != 0)
            {
                foreach (Device d in devices)
                {
                    devstring += "\n" + d.Name + " " + d.FNum;
                    devs.Add(d);
                }
                if (MessageBox.Show("Будут расскрыты следующие приборы:" + devstring + "\nПрименить?\n Данные приборы расскрыты.", "Изменение видимости", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (Device d in devs)
                    {
                        d.Hidden = false;
                        d.NoteText.Replace("'Скрыт'","");

                        context.Operation.Add(new Operation()
                        {
                            UserID = user,
                            OperationDate = DateTime.Now,
                            OperationText = "Расскрытие прибора " + d.Name + " " + d.FNum,
                            ComputerName = Environment.MachineName,
                            ID_Type = 9,
                            ID_Status = 1,
                            ID_Device = d.Device_ID
                        });
                        context.SaveChanges();
                    }
                    context.SaveChanges();
                    MessageBox.Show("Приборы расскрыты", "Изменение видимости", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Метод удаления прибора
        /// </summary>
        /// <param name="devices">Удаляемые приборы</param>
        /// <param name="context">Контекст БД</param>
        /// <param name="user">Пользователь, удаляющий приборы</param>
        public static void deviceDel(List<Device> devices, MetrBaseEn context, int user)
        {
            List<Device> devs = new List<Device>();
            string devstring = "";
            if (devices.Count() != 0)
            {
                foreach (Device d in devices)
                {
                        if (d.Removed.Value)
                            MessageBox.Show(d.Name + " " + d.FNum + " уже удалён, для восстановления необходимо перейти во вкладку 'Исключённые'", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                        {
                            devstring += "\n" + d.Name + " " + d.FNum;
                            devs.Add(d);
                        }
                    }
                }
                
                if (MessageBox.Show("Будут исключены из списка следующие приборы:" + devstring + "\nПрименить?\n Данные приборы можно будет найти и восстановить на вкладке 'Исключённые'.", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (Device d in devs)
                    {                        
                        d.Removed = true;
                        context.Operation.Add(new Operation() { UserID = user, OperationDate = DateTime.Now, OperationText = "Исключение прибора " + d.Name + " " + d.FNum + "\n", ComputerName = Environment.MachineName, ID_Type = 4, ID_Status = 1, ID_Device = d.Device_ID });
                    }
                    context.SaveChanges();
                    MessageBox.Show("Приборы исключены", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            else
            {
                MessageBox.Show("Выделите приборы для удаления");
            }
        }
        /// <summary>
        /// Метод восстановления прибора
        /// </summary>
        /// <param name="devices">Восстановляемые приборы</param>
        /// <param name="context">Контекст БД</param>
        /// <param name="user">Пользователь, восстанавляющий приборы</param>
        public static void deviceRec(List<Device> devices, MetrBaseEn context, int user)
        {
            List<Device> devs = new List<Device>();
            string devstring = "";
            if (devices.Count() != 0)
            {
                foreach (Device d in devices)
                {
                    devstring += "\n" + d.Name + " " + d.FNum;
                    devs.Add(d);
                }
                if (MessageBox.Show("Будут восстановлены следующие приборы:" + devstring + "\nПрименить?\n Данные приборы возвращены на вкладку 'Приборы'.", "Восстановление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (Device d in devs)
                    {
                        d.Removed = false;
                        context.Operation.Add(new Operation() { UserID = user, OperationDate = DateTime.Now, OperationText = "Восстановление прибора " + d.Name + " " + d.FNum, ComputerName = Environment.MachineName, ID_Status = 1, ID_Type = 5, ID_Device = d.Device_ID });
                    }
                    context.SaveChanges();
                    MessageBox.Show("Приборы восстановлены", "Восстановление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выделите приборы для восстановления");
            }
        }

        public static string DevString(List<int> Field, DeviceData device)
        {
            string rString = "";
            foreach (int field in Field)
            {
                //"Пусто"
                //"Порядковый номер"
                //"Название прибора" 
                //"Заводской номер"
                //"Объект" 
                //"Измеряемый параметр"
                //"Единицы измерения" 
                //"МП (Срок годности)" 
                //"ППР (Только текущий месяц)"
                //"МП/ППР 1"
                //"ППР 1"
                //"ППР 2"
                //"ППР 3"
                //"ППР 4"
                //"Период ППР"
                //"Примечания"
                switch (field)
                {
                    case 0: rString += "\t"; break;
                    case 1: rString += device.ID + "\t"; break;
                    case 2: rString += device.Name + "\t"; break;
                    case 3: rString += device.FNum + "\t"; break;
                    case 4: rString += device.ObjectName + "\t"; break;
                    case 5: rString += device.Param + "\t"; break;
                    case 6: rString += device.MetrData + "\t"; break;
                    case 7: rString += device.ExpDate != null ? device.ExpDate.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 8: rString += device.pprMonthDate != null ? device.pprMonthDate.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 9: rString += device.ExpDate != null && device.pprDate1 != null ? device.ExpDate.Value.ToString("dd.MM.yyyy") + "/\n" + device.pprDate1.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 10: rString += device.pprDate1 != null ? device.pprDate1.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 11: rString += device.pprDate2 != null ? device.pprDate2.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 12: rString += device.pprDate3 != null ? device.pprDate3.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 13: rString += device.pprDate4 != null ? device.pprDate4.Value.ToString("dd.MM.yyyy") + "\t" : ""; break;
                    case 14: rString += device.Period + "\t"; break;
                    case 15: rString += device.Note + "\t"; break;
                    default: rString += "\t"; break;
                }
            }
            rString.Trim('\t');
            return rString;
        }
    }

}

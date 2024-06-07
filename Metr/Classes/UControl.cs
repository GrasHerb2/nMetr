using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Metr.Classes
{
    internal class UControl
    {
        public int userID { get; set; }
        public string fullName { get; set; }
        public int roleID { get; set; }
        public string roleTitle { get; set; }
        public bool register { get; set; }
        public bool deactive { get; set; }

        /// <summary>
        /// Контекст БД
        /// </summary>
        static MetrBaseEn context = MetrBaseEn.GetContext();
        /// <summary>
        /// Список пользователей имеющих доступ к системе
        /// </summary>
        public static List<UControl> UserData = new List<UControl>();
        /// <summary>
        /// Список пользователей с деактивированной учётной записью
        /// </summary>
        public static List<UControl> UserDataDeactive = new List<UControl>();
        /// <summary>
        /// Список пользователей запрашивающих доступ к системе
        /// </summary>
        public static List<UControl> UserDataRegister = new List<UControl>();

        /// <summary>
        /// Метод обновления списков пользователей
        /// </summary>
        /// <param name="con">Контекст БД</param>
        public static void UpdateUsers(MetrBaseEn con)
        {
            UserData.Clear();
            UserDataDeactive.Clear();
            UserDataRegister.Clear();
            foreach (User user in con.User.Where(u=>u.User_ID!=1).ToList())
            {
                UserData.Add(new UControl()
                {
                    userID = user.User_ID,
                    fullName = user.FullName,
                    roleID = user.RoleID-1,
                    roleTitle = user.Role.Title,
                    register = user.UPass.Contains("___"),
                    deactive = user.ULogin.Contains("___")
                });
            }
            UserDataRegister = UserData.Where(u => u.register).ToList();
            UserDataDeactive = UserData.Where (u => u.deactive).ToList();
            UserData = UserData.Where(u => !u.register && !u.deactive).ToList();

        }


        /// <summary>
        /// Кодирование строки в хеш-код SHA256
        /// </summary>
        /// <param name="line">Входящая строка для кодирования</param>
        /// <returns>Хеш-код SHA256</returns>
        public static string Sha256Coding(string line)
        {
            var linecode = SHA256.Create();
            StringBuilder builder = new StringBuilder();
            byte[] bytes = linecode.ComputeHash(Encoding.UTF8.GetBytes(line));
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            line = builder.ToString();
            return line;
        }
        /// <summary>
        /// Метод авторизации
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passw">Пароль пользователя</param>
        /// <returns>
        /// Коды результата проверок:
        /// 2   Учетная запись отключена;
        /// 1   Учетная запись не подтверждена;
        /// 0   Успешный вход;
        /// -1  Несовпадение пароля;
        /// -2  Отсутсвие учётной записи с указаным логином;
        /// -3  Невозможность получения данных из базы;
        /// </returns>
        public static int passwCheck(string login, string passw)
        {
            try
            {
                var log256 = Sha256Coding(login);

                

                if (context.User.Where(p => p.ULogin == log256 || p.ULogin == "___" + log256).ToList().Count() == 0) return -2;

                var passwCheck = context.User.Where(p => p.ULogin == log256 || p.ULogin == "___" + log256).FirstOrDefault();


                if (passwCheck.UPass.Contains("___")) return 1;

                if (passwCheck.ULogin.Contains("___")) return 2;

                passw = Sha256Coding(passw);//входящий пароль кодируется
                if (passw == passwCheck.UPass /*пароль в базе*/ )
                    return 0;
                else return -1;
            }
            catch { return -3; }
        }
        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="newLogin">Логин нового пользователя</param>
        /// <param name="newPass">Пароль нового пользователя</param>
        /// <param name="newFullName">ФИО нового пользователя</param>
        /// <param name="newMail">Электронная почта нового пользователя (опционально)</param>
        /// <param name="instant">Создание записи без подтверждения администратором</param>
        /// <param name="role">ID создаваемой записи, по стандарту "Гость"</param>
        /// <returns>
        /// Коды результатов:
        /// 1=Пользователь с таким логином уже записан в БД;
        /// 0=Операця успешна
        /// -1=Ошибка доступа к БД или иное
        /// </returns>
        public static tResult newEmployee(string newLogin, string newPass, string newFullName, string newMail, bool instant = false, int role = 1)
        {
            tResult result = new tResult();
            try
            {
                var a = Sha256Coding(newLogin);
                if (context.User.Where(p => p.ULogin == a).Count() > 0)
                {
                    result.resultid = 1;
                    result.errorText = "Логин занят, необходимо использовать иной";
                    return result;
                }//в системе не может быть двух однинаковых логинов

                result.Operation = new Operation()
                {
                    OperationDate = DateTime.Now,
                    UserID = 0,
                    OperationText = "Регистрация\nКомпьютер:" + Environment.MachineName.ToString() + "\nФИО:" + newFullName + "\n" + DateTime.Now.ToShortDateString()+(instant ? "\nАктивировал " + CurrentUser.user.FullName : "") ,
                    ComputerName = Environment.MachineName.ToString(),
                    ID_Status = instant ? 1 : 2,
                    ID_Type = 1                    
                };
                newLogin = Sha256Coding(newLogin);
                newPass = Sha256Coding(newPass);
                result.User = new User()
                {
                    FullName = newFullName,
                    ULogin = newLogin,
                    RoleID = role,//по стандарту будет роль "Гость"
                    Email = newMail,
                    UPass = (instant ? "" : "___" )+ newPass//instant - мгновенное создание записи администратором
                };
                result.resultid = 0;

                return result;

            }
            catch (System.Exception ex)
            {
                result.resultid = -1;
                result.errorText = ex.InnerException.Message.ToString();
                return result;
            }
        }


        /// <summary>
        /// Изменение пользователя
        /// </summary>
        /// <param name="chgLogin">Изменённый логин пользователя</param>
        /// <param name="chgPass">Изменённый пароль пользователя</param>
        /// <param name="chgFullName">Изменённое ФИО пользователя</param>
        /// <param name="chgMail">Изменённая электронная почта</param>
        /// <returns>
        /// Коды результатов:
        /// 1=Пользователь с таким логином уже записан в БД;
        /// 0=Операця успешна
        /// -1=Ошибка доступа к БД или иное
        /// </returns>
        public static tResult redactEmployee(string chgLogin, string chgPass, string chgFullName, string chgMail)
        {
            tResult result = new tResult();
            try
            {
                var a = Sha256Coding(chgLogin);
                if (chgLogin!=CurrentUser.currentULogin && context.User.Where(p => p.ULogin == a).Count() > 0)
                {
                    result.resultid = 1;
                    result.errorText = "Логин занят, необходимо использовать иной";
                    return result;
                }//в системе не может быть двух однинаковых логинов
                chgLogin = Sha256Coding(chgLogin);
                chgPass = Sha256Coding(chgPass);
                result.User = new User()
                {
                    FullName = chgFullName,
                    ULogin = chgLogin,
                    Email = chgMail,
                    UPass = chgPass
                };
                result.resultid = 0;

                result.Operation = new Operation()
                {
                    UserID = CurrentUser.user.User_ID,
                    OperationDate = DateTime.Now,
                    ComputerName = Environment.MachineName.ToString(),
                    OperationText = CurrentUser.user.FullName + " изменил учётную запись",
                    ID_Status = 1,
                    ID_Type = 9
                };

                return result;

            }
            catch (Exception ex)
            {
                result.resultid = -1;
                result.errorText = ex.InnerException.Message.ToString();
                return result;
            }
        }


        /// <summary>
        /// Деактивация учётной записи пользователя
        /// </summary>
        /// <param name="delIndex">ID диактивируемой учётной записи</param>
        /// <param name="admIndex">ID учётной записи диактивирующего</param>
        /// <returns>Возвращает объект класса tResult хранящий объект отключенной учётной записи, запись в журнал аудита и код операции: 0=Операция успешна  -1=Ошибка деактивации</returns>
        public static tResult deactiveEmp(int delIndex, int admIndex)
        {
            tResult result = new tResult();
            try
            {
                User deluser = context.User.Where(p => p.User_ID == delIndex).FirstOrDefault();
                User admuser = context.User.Where(p => p.User_ID == admIndex).FirstOrDefault();
                result.Operation = new Operation()
                {
                    UserID = admuser.User_ID,
                    OperationDate = DateTime.Now,
                    ComputerName = Environment.MachineName.ToString(),
                    OperationText = admuser.FullName + " отключил учётную запись " + deluser.FullName,
                    ID_Status = 1,
                    ID_Type = 2
                };

                deluser.ULogin = "___" + deluser.ULogin;

                result.User = deluser;
                
                result.resultid = 0;
                
                return result;
            }
            catch (Exception ex){
                result.errorText = "Ошибка деактивации, подробнее:\n" + ex.InnerException.Message.ToString();
                result.resultid = -1;
                return result; }
        }
        /// <summary>
        /// Восстановление учётной записи пользователя
        /// </summary>
        /// <param name="recIndex">ID восстанавливаемой учётной записи</param>
        /// <param name="admIndex">ID учётной записи восстанавливающего</param>
        /// <returns>Возвращает объект класса tResult хранящий объект восстановленной учётной записи, запись в журнал аудита и код операции: 0=Операция успешна  -1=Ошибка восстановления</returns>
        public static tResult recoverEmp(int recIndex, int admIndex)
        {
            tResult result = new tResult();
            try
            {
                User recuser = context.User.Where(p => p.User_ID == recIndex).FirstOrDefault();
                User admuser = context.User.Where(p => p.User_ID == admIndex).FirstOrDefault();
                result.Operation = new Operation()
                {
                    UserID = admuser.User_ID,
                    OperationDate = DateTime.Now,
                    ComputerName = Environment.MachineName.ToString(),
                    OperationText = admuser.FullName + " восстановил учётную запись " + recuser.FullName,
                    ID_Status = 1,
                    ID_Type = 2
                };

                recuser.ULogin = recuser.ULogin.Remove(0,3);

                result.User = recuser;

                result.resultid = 0;

                return result;
            }
            catch (Exception ex)
            {
                result.errorText = "Ошибка восстановления, подробнее:\n" + ex.InnerException.Message.ToString();
                result.resultid = -1;
                return result;
            }
        }
        /// <summary>
        /// Активация учётной записи пользователя
        /// </summary>
        /// <param name="actIndex">ID активируемой учётной записи</param>
        /// <param name="admIndex">ID учётной записи активирующего</param>
        /// <param name="role">Роль, с которой будет активирован пользователь (1 Гость, 2 Пользователь, 3 Администратор)</param>
        /// <returns>Возвращает объект класса tResult хранящий объект активироанной учётной записи, запись в журнал аудита и код операции: 0=Операция успешна  -1=Ошибка активирования</returns>
        public static tResult activateEmp(int actIndex, int admIndex, int role)
        {
            context = MetrBaseEn.GetContext();
            tResult result = new tResult();
            try
            {
                User actuser = context.User.Where(p => p.User_ID == actIndex).FirstOrDefault();
                User admuser = context.User.Where(p => p.User_ID == admIndex).FirstOrDefault();


                actuser.UPass = actuser.UPass.Replace("___","");

                actuser.RoleID = role;

                string roletxt = context.Role.Where(r => r.Role_ID == role).FirstOrDefault().Title;

                Operation op = context.Operation.Where(o => o.UserID == actIndex).FirstOrDefault();

                op.ID_Status = 1;
                op.OperationText += "\nАктивировал " + admuser.FullName;
                context.SaveChanges();
                result.resultid = 0;

                return result;
            }
            catch (Exception ex)
            {
                result.errorText = "Ошибка активации, подробнее:\n" + ex.InnerException.Message.ToString();
                result.resultid = -1;
                return result;
            }
        }
        /// <summary>
        /// Класс результата операции работы с учётными записями.
        /// </summary>
        public class tResult
        {
            public int resultid { get; set; }
            public User User { get; set; }
            public Operation Operation { get; set; }
            public string errorText {  get; set; }
        }
    }
}


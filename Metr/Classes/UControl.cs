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
        static MetrBaseEntities context = MetrBaseEntities.GetContext();
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
        public static void UpdateUsers(MetrBaseEntities con)
        {
            UserData.Clear();
            UserDataDeactive.Clear();
            UserDataRegister.Clear();
            foreach (User user in con.User.Where(u=>u.User_ID!=1).ToList())
            {
                if (!user.ULogin.Contains("___"))
                UserData.Add(new UControl()
                {
                    userID = user.User_ID,
                    fullName = user.FullName,
                    roleID = user.RoleID-1,
                    roleTitle = user.Role.Title,
                    register = user.UPass.Contains("___"),
                    deactive = user.ULogin.Contains("___")
                });
                else
                    UserDataDeactive.Add(new UControl()
                    {
                        userID = user.User_ID,
                        fullName = user.FullName,
                        roleID = user.RoleID,
                        roleTitle = user.Role.Title,
                        register = false
                    });
                UserDataRegister = UserData.Where(u=>u.register).ToList();
                UserData = UserData.Where(u=>!u.register).ToList();
            }            
            
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

                passw = Sha256Coding(passw);/*входящий пароль кодируется*/
                if (passw == passwCheck.UPass /*пароль в базе*/ )
                    return 0;//вход разрешён
                else return -1;//пароль не совпадает
            }
            catch { return -3; } //код -3 возникает при невозможности подключения к БД
        }
        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="newLogin">Логин нового пользователя</param>
        /// <param name="newPass">Пароль нового пользователя</param>
        /// <param name="newFullName">ФИО нового пользователя</param>
        /// <param name="newMail">Электронная почта нового пользователя (опционально)</param>
        /// <returns>Возвращает класс tResult который содержит объект нового пользователя, запись в журнал аудита и код операции:   1=Пользователь с таким логином уже записан в БД;    0=Операця успешна   -1=Ошибка доступа к БД или иное</returns>
        public static tResult newEmployee(string newLogin, string newPass, string newFullName, string newMail, bool instant = false)
        {
            tResult result = new tResult();
            try
            {
                var a = Sha256Coding(newLogin);
                if (context.User.Where(p => p.ULogin == a).Count() > 0)
                {
                    result.resultid = 1;
                    return result;
                }//в системе не может быть двух однинаковых логинов

                result.Action = new Actions()
                {
                    ActionDate = DateTime.Now,
                    UserID = 0,
                    ActionText = "Запрос на регистрацию\nКомпьютер:" + Environment.MachineName.ToString() + "\nФИО:" + newFullName + "\n" + DateTime.Now.ToShortDateString(),
                    ComputerName = Environment.MachineName.ToString()
                };
                newLogin = Sha256Coding(newLogin);
                newPass = Sha256Coding(newPass);
                result.User = new User()
                {
                    FullName = newFullName,
                    ULogin = newLogin,
                    RoleID = 1,//неподтверждённый пользователь может использовать только просмотр
                    Email = newMail,
                    UPass = (instant ? "" : "___" )+ newPass
                };

                result.resultid = 0;

                return result;

            }
            catch (System.Exception ex)
            {
                result.resultid = -1;
                result.Action = new Actions() { ActionText = ex.Message.ToString() };
                return result;
            }
        }
        /// <summary>
        /// Деактивация учётной записи пользователя
        /// </summary>
        /// <param name="delIndex">Логин отключаемой учётной записи</param>
        /// <param name="admIndex">Логин учётной записи диактивирующего</param>
        /// <returns>Возвращает объект класса tResult хранящий объект отключенной учётной записи, запись  журнал аудита и код операции: 0=Операция успешна  -1=Ошибка отключения</returns>
        public static tResult deactiveEmp(int delIndex, int admIndex)
        {
            tResult result = new tResult();
            try
            {
                User deluser = context.User.Where(p => p.User_ID == delIndex).FirstOrDefault();
                User admuser = context.User.Where(p => p.User_ID == admIndex).FirstOrDefault();
                result.Action = new Actions()
                {
                    UserID = admuser.User_ID,
                    ActionDate = DateTime.Now,
                    ComputerName = Environment.MachineName.ToString(),
                    ActionText = admuser.FullName + " отключил учётную запись " + deluser.FullName
                };

                deluser.ULogin = "___" + deluser.ULogin;

                result.User = deluser;
                
                result.resultid = 0;
                
                return result;
            }
            catch {
                result.resultid = -1;
                return result; }
        }
        /// <summary>
        /// Восстановление учётной записи пользователя
        /// </summary>
        /// <param name="delLogin">Логин восстанавливаемой учётной записи</param>
        /// <param name="admLogin">Логин учётной записи восстанавливающего</param>
        /// <returns>Возвращает объект класса tResult хранящий объект восстановленной учётной записи, запись  журнал аудита и код операции: 0=Операция успешна  -1=Ошибка восстановления</returns>
        public static tResult recoverEmp(int recIndex, int admIndex)
        {
            tResult result = new tResult();
            try
            {
                User recuser = context.User.Where(p => p.User_ID == recIndex).FirstOrDefault();
                User admuser = context.User.Where(p => p.User_ID == admIndex).FirstOrDefault();
                result.Action = new Actions()
                {
                    UserID = admuser.User_ID,
                    ActionDate = DateTime.Now,
                    ComputerName = Environment.MachineName.ToString(),
                    ActionText = admuser.FullName + " восстановил учётную запись " + recuser.FullName
                };

                recuser.ULogin = recuser.ULogin.Remove(0,3);

                result.User = recuser;

                result.resultid = 0;

                return result;
            }
            catch
            {
                result.resultid = -1;
                return result;
            }
        }
        /// <summary>
        /// Активация учётной записи пользователя
        /// </summary>
        /// <param name="delLogin">Логин активируемой учётной записи</param>
        /// <param name="admLogin">Логин учётной записи активирующего</param>
        /// <returns>Возвращает объект класса tResult хранящий объект активироанной учётной записи, запись  журнал аудита и код операции: 0=Операция успешна  -1=Ошибка активирования</returns>
        public static tResult activateEmp(int actIndex, int admIndex, int role)
        {
            tResult result = new tResult();
                User actuser = context.User.Where(p => p.User_ID == actIndex).FirstOrDefault();
                User admuser = context.User.Where(p => p.User_ID == admIndex).FirstOrDefault();
                result.Action = new Actions()
                {
                    UserID = admuser.User_ID,
                    ActionDate = DateTime.Now,
                    ComputerName = Environment.MachineName.ToString(),
                    ActionText = admuser.FullName + " активировал учётную запись " + actuser.FullName
                };

                actuser.UPass = actuser.UPass.Remove(0, 3);
                actuser.RoleID = role;

                result.User = actuser;

                result.resultid = 0;

                return result;
        }
        /// <summary>
        /// Класс результата операции работы с учётными записями.
        /// </summary>
        public class tResult
        {
            public int resultid { get; set; }
            public User User { get; set; }
            public Actions Action { get; set; }
        }
    }
}


namespace Metr.Classes
{
    /// <summary>
    /// Класс для пользователя авторизованного в системе
    /// </summary>
    public static class CurrentUser
    {
        public static User user { get; set; }
        public static string currentULogin {  get; set; }
        public static string currentUPass {  get; set; }
        public static string currentUEmail {  get; set; }

        public static void Upd(string login, string pass, string email)
        {
            currentULogin = login;
            currentUPass = pass;
            currentUEmail = email;
        }
    }
    
}

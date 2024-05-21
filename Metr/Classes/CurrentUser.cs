namespace Metr.Classes
{
    /// <summary>
    /// Класс для пользователя авторизованного в системе
    /// </summary>
    public class CurrentUser
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }
    }
}

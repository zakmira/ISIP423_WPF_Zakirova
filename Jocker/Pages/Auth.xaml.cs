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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jocker.Pages
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class Auth : Page
    {
        public int CurrentUserId { get; private set; }
        public string CurrentUserRole { get; private set; }
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        public Auth()
        {
            InitializeComponent();
            FixPasswDB(); // для того, чтобы хэшированный пароль попадал в БД
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public void LogIn(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль обязательны");
                return;
            }

            string hashPassword = HashPassword(password);

            var user = Core.Context.Users
               .FirstOrDefault(u => u.Login == login && u.PasswordHash == hashPassword);

            if (user != null)
            {
                CurrentUserId = user.UserID;
                CurrentUserRole = user.Roles?.RoleName ?? "Пользователь";
                IsAuthenticated = true;

                MessageBox.Show($"Добро пожаловать, {user.Name}!");

                // переход нна страницу каталога и доступ к SideBar
                MainWindow mainWindow = new MainWindow(CurrentUserId, CurrentUserRole, user.Name);
                mainWindow.Show();
                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }

        /// <summary>
        /// Проверка логина на уникальность
        /// </summary>
        /// <param name="login">Логин</param>
        public void CheckLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                Console.WriteLine("Логин не может быть пустым");
                return;
            }

            bool exists = Core.Context.Users.Any(u => u.Login == login);
            
            Console.WriteLine(exists ? "Логин уже занят" : "Логин свободен" );
         
        }

        /// <summary>
        /// Хэширование пароля
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Обработчик кнопки входа
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = Login.Text.Trim();
            string password = Password.Password;

            LogIn(login, password);
        }

        /// <summary>
        /// Вспомогательный метод для того, чтобы хэшированнный пароль передавался в БД
        /// </summary>
        private void FixPasswDB()
        {
            var users = Core.Context.Users.ToList();
            bool changed = false;
            foreach (var user in users)
            {
                if (user.PasswordHash.Length < 30) // если пароль не похож на хэш
                {
                    user.PasswordHash = HashPassword(user.PasswordHash);
                    changed = true;
                }
            }
            if (changed)
            {
                Core.Context.SaveChanges();
            }
        }

        /// <summary>
        /// Обработчик ссылки на регистрацию (внизу страницы)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Register());
        }
    }
}

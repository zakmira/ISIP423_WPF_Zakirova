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
    /// Логика взаимодействия для Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        public Register()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Регистрация пользователя с дальнейшим возвратом на страницу авторизации
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="email">Почта</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public void SignIn(string name, string email, string login, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                 string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Все поля должны быть заполнены");
                return;
            }

            string hashedPassword = HashPassword(password);

            if (Core.Context.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Логин уже занят");
                return;
            }

            if (Core.Context.Users.Any(u => u.Email == email))
            {
                MessageBox.Show("Email уже занят");
                return;
            }

            var defaultRole = Core.Context.Roles.FirstOrDefault(r => r.RoleName == "Пользователь");

            if (defaultRole == null)
            {
                MessageBox.Show("Роль по умолчанию не найдена");
                return;
            }

            var newUser = new Users
            {
                Name = name,
                Email = email,
                Login = login,
                PasswordHash = hashedPassword,
                RoleID = defaultRole.RoleID,
                IsFrozen = false,
                FreezeReason = null
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            MessageBox.Show($"Регистрация успешна, {login}!");

            NavigationService?.Navigate(new Auth());
        }

        /// <summary>
        /// Проверка уникальности логина
        /// </summary>
        /// <param name="login">Логин</param>
        public void CheckLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Логин не может быть пустым");
                return;
            }

            bool exists = Core.Context.Users.Any(u => u.Login == login);
            MessageBox.Show(exists ? "Логин уже занят" : "Логин свободен");
        }

       /// <summary>
       /// Проверка уникальности почты
       /// </summary>
       /// <param name="email">Почта</param>
        public void CheckEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email не может быть пустым");
                return;
            }

            bool exists = Core.Context.Users.Any(u => u.Email == email);
            MessageBox.Show(exists ? "Email уже занят" : "Email свободен");
        }

        /// <summary>
        /// Хэширование пароля
        /// </summary>
        /// <param name="password">Пароль</param>
        /// <returns>Хэшированный пароль</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Обработчик кнопки регистрации
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            string name = Name.Text.Trim();
            string email = Email.Text.Trim();
            string login = Login.Text.Trim();
            string password = Password.Password;

            SignIn(name, email, login, password);
        }

        /// <summary>
        /// Обработчик ссылки на авторизацию (внизу страницы)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param> 
        private void SignInLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Auth());
        }
    }
}

using Jocker.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace Jocker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int CurrentUserId { get; set; }
        public string CurrentUserRole { get; set; }
        public string CurrentUserName { get; set; }
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="userRole">Роль пользователя</param>
        /// <param name="userName">Имя пользователя</param>
        public MainWindow(int userId, string userRole, string userName)
        {
            InitializeComponent();

            CurrentUserId = userId;
            CurrentUserRole = userRole;
            CurrentUserName = userName;

            ConfigureSidebar();
            OpenCatalogPage();
        }

        /// <summary>
        /// Настройка видимости кнопки админа и автора
        /// </summary>
        private void ConfigureSidebar()
        {
            Admin.Visibility = CurrentUserRole == "Админ" ? Visibility.Visible : Visibility.Collapsed;
            AuthorPage.Visibility = CurrentUserRole == "Автор" ? Visibility.Visible : Visibility.Collapsed;

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user != null && user.IsFrozen)
            {
                textFreezeWarning.Text = $"ВНИМАНИЕ! Аккаунт заморожен. Причина: {user.FreezeReason}";
                freezeWarning.Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// Обработчик открытия каталога
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Дополнительные данные</param>
        private void Catalog_Click(object sender, EventArgs e)
        {
            OpenCatalogPage();
        }

        /// <summary>
        /// Обработчик открытия списков книг
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Дополнительные данные</param>
        private void BookLists_Click(object sender, EventArgs e)
        {
            OpenBookListsPage();
        }

        /// <summary>
        /// Обработчик открытия страницы админа
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Дополнительные данные</param>
        private void Admin_Click(object sender, EventArgs e)
        {
            OpenAdminPage();
        }

        /// <summary>
        /// Обработчик открытия каталога
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Дополнительные данные</param>
        private void AuthorPage_Click(object sender, EventArgs e)
        {
            OpenAuthorPage();
        }

        /// <summary>
        /// Обработчик открытия профиля
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Дополнительные данные</param>
        private void Profile_Click(object sender, EventArgs e)
        {
            OpenProfilePage();
        }

        /// <summary>
        /// Обработчик выхода из аккаунта
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Дополнительные данные</param>
        private void Exit_Click(object sender, EventArgs e)
        {
            Logout();
        }

        /// <summary>
        /// Открытие каталога
        /// </summary>
        private void OpenCatalogPage()
        {
            mainFrame.Navigate(new Catalog(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Открытие списков книг
        /// </summary>
        private void OpenBookListsPage()
        {
            mainFrame.Navigate(new BookListsPage(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Открытие страницы админа (если пользователь явялется админом)
        /// </summary>
        private void OpenAdminPage()
        {
            if (CurrentUserRole != "Админ")
            {
                MessageBox.Show("Доступ запрещен");
                return;
            }

            mainFrame.Navigate(new Admin());
        }

        /// <summary>
        /// Открытие страницы автора (если пользователь является автором)
        /// </summary>
        private void OpenAuthorPage()
        {
            if (CurrentUserRole != "Автор")
            {
                MessageBox.Show("Доступ запрещен");
                return;
            }

            mainFrame.Navigate(new AuthorPage(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Открытие профиля
        /// </summary>
        private void OpenProfilePage()
        {
            mainFrame.Navigate(new Profile(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Выход из аккаунта
        /// </summary>
        private void Logout()
        {
            CurrentUserId = 0;
            CurrentUserRole = null;
            CurrentUserName = null;

            mainFrame.Navigate(new Auth());

            ConfigureSidebar();
        }
    }
}
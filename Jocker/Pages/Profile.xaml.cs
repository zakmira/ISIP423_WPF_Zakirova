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
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        private int CurrentUserId { get; set; }
        private string CurrentUserRole { get; set; }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="userRole">Роль пользователя</param>
        public Profile(int userId, string userRole)
        {
            InitializeComponent();

            CurrentUserId = userId;
            CurrentUserRole = userRole;

            this.Loaded += Page_Loaded;
        }

        /// <summary>
        /// Получение информации о пользователе
        /// </summary>
        /// <returns>Пользователя</returns>
        public object GetUserInfo()
        {
            if (CurrentUserId == 0)
                return null;

            var user = Core.Context.Users
                .Where(u => u.UserID == CurrentUserId)
                .Select(u => new
                {
                    u.UserID,
                    u.Login,
                    u.Email,
                    u.Name,
                    RoleName = CurrentUserRole,
                    u.IsFrozen,
                    u.FreezeReason
                })
                .FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Изменение имени
        /// </summary>
        /// <param name="newName">Новое имя</param>
        public void UpdateName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                Console.WriteLine("Имя не может быть пустым");
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user != null)
            {
                user.Name = newName;
                Core.Context.SaveChanges();
                Console.WriteLine("Имя обновлено");
            }
        }

        /// <summary>
        /// Изменение логина
        /// </summary>
        /// <param name="newLogin">Новый логин</param>
        public void UpdateLogin(string newLogin)
        {
            if (string.IsNullOrEmpty(newLogin))
            {
                Console.WriteLine("Логин не может быть пустым");
                return;
            }

            if (Core.Context.Users.Any(u => u.Login == newLogin && u.UserID != CurrentUserId))
            {
                Console.WriteLine("Логин уже занят");
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user != null)
            {
                user.Login = newLogin;
                Core.Context.SaveChanges();
                Console.WriteLine($"Логин изменен на {newLogin}");
            }
        }

        /// <summary>
        /// Изменени почты
        /// </summary>
        /// <param name="newEmail">Новый email</param>
        public void UpdateEmail(string newEmail)
        {
            if (string.IsNullOrEmpty(newEmail))
            {
                Console.WriteLine("Email не может быть пустым");
                return;
            }

            if (Core.Context.Users.Any(u => u.Email == newEmail && u.UserID != CurrentUserId))
            {
                Console.WriteLine("Email уже занят");
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user != null)
            {
                user.Email = newEmail;
                Core.Context.SaveChanges();
                Console.WriteLine("Email обновлен");
            }
        }

        /// <summary>
        /// Изменение пароля
        /// </summary>
        /// <param name="oldPassword">Старый пароль</param>
        /// <param name="newPassword">Новый пароль</param>
        public void UpdatePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                Console.WriteLine("Все поля пароля должны быть заполнены");
                return;
            }

            if (newPassword.Length < 6)
            {
                Console.WriteLine("Новый пароль должен содержать минимум 6 символов");
                return;
            }

            string hashedOldPassword = HashPassword(oldPassword);

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user == null)
            {
                Console.WriteLine("Пользователь не найден");
                return;
            }

            if (user.PasswordHash != hashedOldPassword)
            {
                Console.WriteLine("Неверный старый пароль");
                return;
            }

            user.PasswordHash = HashPassword(newPassword);
            Core.Context.SaveChanges();
            Console.WriteLine("Пароль изменен");
        }

        /// <summary>
        /// Получения отзывов пользователя
        /// </summary>
        /// <returns>Отзывы</returns>
        public object GetUserReviews()
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.UserID == CurrentUserId)
                .Select(r => new
                {
                    r.ReviewID,
                    r.Rating,
                    r.ReviewText,
                    r.IsFrozen,
                    BookTitle = Core.Context.Books
                        .Where(b => b.BookID == r.BookID)
                        .Select(b => b.Title)
                        .FirstOrDefault() ?? "Неизвестная книга",
                    BookID = r.BookID
                })
                .ToList();

            return reviews;
        }

        /// <summary>
        /// Удаление отзыва
        /// </summary>
        /// <param name="reviewId">ID отзыва</param>
        public void DeleteReview(int reviewId)
        {
            var review = Core.Context.Reviews
                .FirstOrDefault(r => r.ReviewID == reviewId && r.UserID == CurrentUserId);

            if (review == null)
            {
                Console.WriteLine("Отзыв не найден");
                return;
            }

            int bookId = review.BookID;
            Core.Context.Reviews.Remove(review);
            Core.Context.SaveChanges();

            UpdateBookRating(bookId);

            Console.WriteLine("Отзыв удален");
        }

        /// <summary>
        /// Обновление рейтинга книги
        /// </summary>
        /// <param name="bookId">ID книги</param>
        private void UpdateBookRating(int bookId)
        {
            var avgRating = Core.Context.Reviews
                .Where(r => r.BookID == bookId && !r.IsFrozen)
                .Average(r => (decimal?)r.Rating) ?? 0;

            var book = Core.Context.Books.FirstOrDefault(b => b.BookID == bookId);
            if (book != null)
            {
                book.Rating = Math.Round(avgRating, 1);
                Core.Context.SaveChanges();
            }
        }

        /// <summary>
        /// Заявка на авторство
        /// </summary>
        public void SubmitAuthorRequest()
        {
            var existingRequest = Core.Context.Requests.FirstOrDefault(r => r.UserID == CurrentUserId);

            if (existingRequest != null)
            {
                Console.WriteLine("Заявка уже была подана");
                return;
            }

            var statusPending = Core.Context.Statuses.FirstOrDefault(s => s.StatusName == "На рассмотрении");

            var request = new Requests
            {
                UserID = CurrentUserId,
                StatusID = statusPending?.StatusID ?? 1
            };

            Core.Context.Requests.Add(request);
            Core.Context.SaveChanges();

            Console.WriteLine("Заявка на роль автора отправлена");
        }

        /// <summary>
        /// Аппеляция заморозки
        /// </summary>
        /// <param name="reason">Аргумент почему профиль должен быть разморожен</param>
        public void AppealFreeze(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                Console.WriteLine("Укажите причину оспаривания");
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user == null || !user.IsFrozen)
            {
                Console.WriteLine("Ваш аккаунт не заморожен");
                return;
            }

            var existingPetition = Core.Context.Petitions.FirstOrDefault(p => p.UserID == CurrentUserId);

            if (existingPetition != null)
            {
                Console.WriteLine("Вы уже подавали заявку на снятие заморозки");
                return;
            }

            var statusPending = Core.Context.Statuses.FirstOrDefault(s => s.StatusName == "На рассмотрении");
            var targetUser = Core.Context.Targets.FirstOrDefault(t => t.TargetName == "Пользователь");

            var petition = new Petitions 
            {
                UserID = CurrentUserId,
                TargetID = CurrentUserId, 
                Targets = targetUser,
                Reason = reason,
                StatusID = statusPending?.StatusID ?? 1
            };

            Core.Context.Petitions.Add(petition);
            Core.Context.SaveChanges();
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
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Загрузка страницы
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserInfo();
            LoadUserReviews();
            CheckFreezeStatus();
        }

        /// <summary>
        /// Загрузка информации о пользователе
        /// </summary>
        private void LoadUserInfo()
        {
            if (CurrentUserId == 0)
            {
                NavigationService?.Navigate(new Auth());
                return;
            }

            dynamic user = GetUserInfo();

            if (user == null)
            {
                NavigationService?.Navigate(new Auth());
                return;
            }

            Login.Text = user.Login;
            Name.Text = user.Name;
            Email.Text = user.Email;
            Role.Text = user.RoleName;
        }

        /// <summary>
        /// Обработчик изменения логина
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void EditLogin_Click(object sender, RoutedEventArgs e)
        {
            string newLogin = Microsoft.VisualBasic.Interaction.InputBox("Введите новый логин:", "Изменение логина", Login.Text);

            if (!string.IsNullOrEmpty(newLogin))
            {
                UpdateLogin(newLogin);
                LoadUserInfo();
                MessageBox.Show("Логин изменен");
            }
        }

        /// <summary>
        /// Обработчик изменения имени
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Введите новое имя:", "Изменение имени", Name.Text);

            if (!string.IsNullOrEmpty(newName))
            {
                UpdateName(newName);
                LoadUserInfo();
                MessageBox.Show("Имя изменено");
            }
        }

        /// <summary>
        /// Обработчик изменения email
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void EditEmail_Click(object sender, RoutedEventArgs e)
        {
            string newEmail = Microsoft.VisualBasic.Interaction.InputBox("Введите новый email:", "Изменение email", Email.Text);

            if (!string.IsNullOrEmpty(newEmail))
            {
                UpdateEmail(newEmail);
                LoadUserInfo();
                MessageBox.Show("Email изменен!");
            }
        }

        /// <summary>
        /// Обработчик изменения пароля
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = Microsoft.VisualBasic.Interaction.InputBox("Введите старый пароль:", "Изменение пароля");

            if (string.IsNullOrEmpty(oldPassword))
            {
                MessageBox.Show("Введите старый пароль");
                return;
            }

            string newPassword = Microsoft.VisualBasic.Interaction.InputBox("Введите новый пароль (мин. 6 символов):", "Изменение пароля");

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Введите новый пароль");
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов");
                return;
            }

            string confirmPassword = Microsoft.VisualBasic.Interaction.InputBox("Подтвердите новый пароль:", "Изменение пароля");

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            UpdatePassword(oldPassword, newPassword);
            MessageBox.Show("Пароль изменен");
        }

        /// <summary>
        /// Загрузка отзывов пользователя
        /// </summary>
        private void LoadUserReviews()
        {
            var reviews = GetUserReviews() as IEnumerable<dynamic>;
            var items = new List<dynamic>();

            foreach (var review in reviews)
            {
                items.Add(new
                {
                    BookTitle = review.BookTitle,
                    Rating = review.Rating,
                    Comment = review.ReviewText,
                    IsFrozen = review.IsFrozen
                });
            }

            ReviewsControl.ItemsSource = items;
        }

        /// <summary>
        /// Проверка, заморожен ли аккаунт
        /// </summary>
        private void CheckFreezeStatus()
        {
            if (CurrentUserId == 0) return;

            dynamic user = GetUserInfo();

            if (user.IsFrozen && user.RoleName != "Админ")
            {
                FreezeWarning.Text = $"Ваш аккаунт заморожен! Причина: {user.FreezeReason}";
                FreezeWarningB.Visibility = Visibility.Visible;
            }
            else
            {
                FreezeWarningB.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Обработчик кнопки подачи заявки на авторство
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void SubmitAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserRole == "Автор")
            {
                MessageBox.Show("Вы уже и так автор");
                return;
            }

            var existingRequest = Core.Context.Requests
                .FirstOrDefault(r => r.UserID == CurrentUserId);

            if (existingRequest != null)
            {
                MessageBox.Show("Вы уже подавали заявку на роль автора");
                return;
            }

            var result = MessageBox.Show("Подтвердите заявку",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SubmitAuthorRequest();
                MessageBox.Show("Заявка на роль автора отправлена!");
            }
        }

        /// <summary>
        /// Обработчик кнопки аппеляции заморозки
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину оспаривания заморозки:", "Оспаривание заморозки");

            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Укажите причину оспаривания");
                return;
            }

            AppealFreeze(reason);
            MessageBox.Show("Заявка на разморозку отправлена");
        }

        /// <summary>
        /// Обработчик удаления кнопки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int reviewId = (int)button.Tag;

            var result = MessageBox.Show("Подтвердите удаление", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DeleteReview(reviewId);
                LoadUserReviews();
                LoadUserInfo();
            }
        }
    }
}

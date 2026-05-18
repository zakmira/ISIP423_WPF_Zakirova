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
    /// Логика взаимодействия для Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        private string CurrentSection = "Appeals";

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        public Admin()
        {
            InitializeComponent();

            this.Loaded += Page_Loaded;
        }
         /// <summary>
         /// Получение списка аппеляций
         /// </summary>
         /// <returns>Аппеляции</returns>
        public object GetAppeals()
        {
            var appeals = Core.Context.Appeals
                 .Where(a => a.Statuses.StatusName == "На рассмотрении")
                 .Select(a => new
                 {
                     a.AppeaLID,
                     a.TargetID,
                     a.Text,
                     TargetName = a.Targets.TargetName,
                     UserName = a.Users.Login,
                     StatusName = a.Statuses.StatusName
                 })
                 .ToList();
            return appeals;
        }

        /// <summary>
        /// Принятие аппеляции
        /// </summary>
        /// <param name="appealId">ID аппеляции</param>
        public void AcceptAppeal(int appealId)
        {
            var appeal = Core.Context.Appeals.FirstOrDefault(a => a.AppeaLID == appealId);
            if (appeal == null) return;

            var approvedStatus = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "Принята");
            appeal.StatusID = approvedStatus?.StatusID ?? 2;

            string targetType = appeal.Targets?.TargetName;

            if (targetType == "Книга")
            {
                var book = Core.Context.Books.FirstOrDefault(b => b.BookID == appeal.TargetID);
                if (book != null)
                {
                    book.IsFrozen = true;
                    book.FreezeReason = $"Заморожено по жалобе #{appealId}: {appeal.Text}";
                }
            }
            else if (targetType == "Отзыв")
            {
                var review = Core.Context.Reviews.FirstOrDefault(r => r.ReviewID == appeal.TargetID);
                if (review != null)
                {
                    review.IsFrozen = true;
                    review.FreezeReason = $"Заморожено по жалобе #{appealId}: {appeal.Text}";
                }
            }
            else if (targetType == "Пользователь" || targetType == "Автор")
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.UserID == appeal.TargetID);
                if (user != null)
                {
                    user.IsFrozen = true;
                    user.FreezeReason = $"Заморожен по жалобе #{appealId}: {appeal.Text}";
                }
            }

            Core.Context.SaveChanges();
            Console.WriteLine($"Жалоба #{appealId} принята");
        }

        /// <summary>
        /// Отклонение аппеляции
        /// </summary>
        /// <param name="appealId">ID аппеляции</param>
        public void RejectAppeal(int appealId)
        {
            var appeal = Core.Context.Appeals.FirstOrDefault(a => a.AppeaLID == appealId);
            if (appeal == null) return;

            var rejectedStatus = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "Отклонена");

            appeal.StatusID = rejectedStatus?.StatusID ?? 3;
            Core.Context.SaveChanges();

            Console.WriteLine($"Жалоба #{appealId} отклонена");
        }

        /// <summary>
        /// Получение заявок на разморозку
        /// </summary>
        /// <returns>Заявки на разморозку</returns>
        public object GetUnfreezePetitions()
        {
            var petitions = Core.Context.Petitions
                .Where(p => p.Statuses.StatusName == "На рассмотрении")
                .Select(p => new
                {
                    p.ComplaintID,
                    p.Reason,
                    p.StatusID,
                    UserName = p.Users.Login,
                    TargetName = p.Targets.TargetName,
                    TargetID = p.TargetID,
                })
                .ToList();
            return petitions;
        }

        /// <summary>
        /// Принятие заявки (разморозка)
        /// </summary>
        /// <param name="petitionId">ID заявки</param>
        public void AcceptUnfreezePetition(int petitionId)
        {
            var petition = Core.Context.Petitions.FirstOrDefault(p => p.ComplaintID == petitionId);
            if (petition == null) return;

            var approvedStatus = Core.Context.Statuses.FirstOrDefault(s => s.StatusName == "Принята");
            petition.StatusID = approvedStatus?.StatusID ?? 2;

            string targetType = petition.Targets?.TargetName;

            if (targetType == "Пользователь" || targetType == "Автор")
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.UserID == petition.TargetID);
                if (user != null)
                {
                    user.IsFrozen = false;
                    user.FreezeReason = null;
                    Core.Context.SaveChanges();
                }
            }
            else if (targetType == "Книга")
            {
                var book = Core.Context.Books.FirstOrDefault(b => b.BookID == petition.TargetID);
                if (book != null)
                {
                    book.IsFrozen = false;
                    book.FreezeReason = null;
                    Core.Context.SaveChanges();
                }
            }
            else if (targetType == "Отзыв")
            {
                var review = Core.Context.Reviews.FirstOrDefault(r => r.ReviewID == petition.TargetID);
                if (review != null)
                {
                    review.IsFrozen = false;
                    review.FreezeReason = null;
                    Core.Context.SaveChanges();
                }
            }

            Core.Context.SaveChanges();
            Console.WriteLine($"Заявка на разморозку #{petitionId} принята");
        }

        /// <summary>
        /// Отклонение заявки
        /// </summary>
        /// <param name="petitionId">ID заявки</param>
        public void RejectUnfreezePetition(int petitionId)
        {
            var petition = Core.Context.Petitions.FirstOrDefault(p => p.ComplaintID == petitionId);
            if (petition == null) return;

            var rejectedStatus = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "Отклонена");

            petition.StatusID = rejectedStatus?.StatusID ?? 3;
            Core.Context.SaveChanges();
        }

        /// <summary>
        /// Получение заявок на авторство
        /// </summary>
        /// <returns>Заявки</returns>
        public object GetAuthorRequests()
        {
            var requests = Core.Context.Requests
                .Where(r => r.Statuses.StatusName == "На рассмотрении")
                .Select(r => new
                {
                    r.RequestID,
                    UserName = r.Users.Login,
                    r.UserID
                })
                .ToList();
            return requests;
        }

        /// <summary>
        /// Принятие заявки на авторство
        /// </summary>
        /// <param name="requestId">ID заявки</param>
        public void AcceptAuthorRequest(int requestId)
        {
            var request = Core.Context.Requests.FirstOrDefault(r => r.RequestID == requestId);
            if (request == null) return;

            var approvedStatus = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "Одобрено");
            request.StatusID = approvedStatus?.StatusID ?? 2;

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == request.UserID);
            if (user != null)
            {
                var authorRole = Core.Context.Roles.FirstOrDefault(r => r.RoleName == "Автор");
                if (authorRole != null)
                {
                    user.RoleID = authorRole.RoleID;
                }

                var author = new Authors
                {
                    UserID = user.UserID,
                    AuthorName = user.Name
                };
                Core.Context.Authors.Add(author);
            }

            Core.Context.SaveChanges();
            Console.WriteLine($"Заявка на авторство #{requestId} принята");
        }

        /// <summary>
        /// Отклонение заявки
        /// </summary>
        /// <param name="requestId">ID заявки</param>
        public void RejectAuthorRequest(int requestId)
        {
            var request = Core.Context.Requests.FirstOrDefault(r => r.RequestID == requestId);
            if (request == null) return;

            var rejectedStatus = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "Отклонено");

            request.StatusID = rejectedStatus?.StatusID ?? 3;
            Core.Context.SaveChanges();

            Console.WriteLine($"Заявка на авторство #{requestId} отклонена");
        }

        /// <summary>
        /// Получение замороженных книг
        /// </summary>
        /// <returns>Замороженные книги</returns>
        public object GetFrozenBooks()
        {
            return Core.Context.Books
               .Where(b => b.IsFrozen == true)
               .Select(b => new { b.BookID, b.Title, b.FreezeReason })
               .ToList();
        }

        /// <summary>
        /// Получение замороженных пользователей
        /// </summary>
        /// <returns>Замороженные пользователи</returns>
        public object GetFrozenUsers()
        {
            return Core.Context.Users
                 .Where(u => u.IsFrozen == true)
                 .Select(u => new { u.UserID, u.Login, u.Name, u.Email, u.FreezeReason })
                 .ToList();
        }

        /// <summary>
        /// Получение замороженных отзывов
        /// </summary>
        /// <returns>Замороженные отзывы</returns>
        public object GetFrozenReviews()
        {
            return Core.Context.Reviews
                .Where(r => r.IsFrozen == true)
                .Select(r => new { r.ReviewID, r.Rating, r.ReviewText, r.FreezeReason })
                .ToList();
        }

        /// <summary>
        /// Получение всех пользователей
        /// </summary>
        /// <returns></returns>
        public object GetAllUsers()
        {
            return Core.Context.Users
                .Select(u => new { u.UserID, u.Login, u.Name, u.Email, RoleName = u.Roles.RoleName, u.IsFrozen })
                .OrderBy(u => u.Login)
                .ToList();
        }

        /// <summary>
        /// Присваивание роли пользователю
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="roleName">Название роли</param>
        public void AssignRole(int userId, string roleName)
        {
            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return;

            var role = Core.Context.Roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null) return;

            user.RoleID = role.RoleID;
            Core.Context.SaveChanges();

            Console.WriteLine($"Пользователю {user.Login} назначена роль {roleName}");
        }

        /// <summary>
        /// Смена пароля пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="newPassword">Новый пароль</param>
        public void ChangeUserPassword(int userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                Console.WriteLine("Пароль должен содержать минимум 6 символов");
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return;

            user.PasswordHash = HashPassword(newPassword);
            Core.Context.SaveChanges();

            Console.WriteLine($"Пароль пользователя {user.Login} изменен");
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
            LoadSection();
        }

        /// <summary>
        /// Загрузка RadioButton
        /// </summary>
        private void LoadSection()
        {
            switch (CurrentSection)
            {
                case "Appeals":
                    LoadAppeals();
                    break;
                case "Unfreeze":
                    LoadUnfreezePetitions();
                    break;
                case "AuthorRequests":
                    LoadAuthorRequests();
                    break;
                case "Frozen":
                    LoadFrozenItems();
                    break;
                case "Users":
                    LoadUsers();
                    break;
            }
        }

        /// <summary>
        /// Загрузка жалоб
        /// </summary>
        private void LoadAppeals()
        {
            if (ItemsControl == null) return;

            var appeals = GetAppeals() as IEnumerable<dynamic>;
            var items = new List<dynamic>();

            foreach (var appeal in appeals)
            {
                items.Add(new
                {
                    Id = appeal.AppeaLID,
                    DisplayText = $"Жалоба на {appeal.TargetName}",
                    SubText = appeal.Text,
                    ShowAccept = true,
                    ShowReject = true,
                    ShowAssignRole = false,
                    ShowChangePassword = false,
                    ShowFreezeUser = false,
                    ShowUnfreezeUser = false
                });
            }

            ItemsControl.ItemsSource = items;
        }

        /// <summary>
        /// Загрузка заявок на разморозку
        /// </summary>
        private void LoadUnfreezePetitions()
        {
            if (ItemsControl == null) return;
            var petitions = GetUnfreezePetitions() as IEnumerable<dynamic>;
            var items = new List<dynamic>();

            foreach (var petition in petitions)
            {
                items.Add(new
                {
                    Id = petition.ComplaintID,
                    DisplayText = $"Заявка на разморозку: {petition.TargetName}",
                    SubText = petition.Reason,
                    ShowAccept = true,
                    ShowReject = true,
                    ShowAssignRole = false,
                    ShowChangePassword = false,
                    ShowFreezeUser = false,
                    ShowUnfreezeUser = false
                });
            }

            ItemsControl.ItemsSource = items;
        }

        /// <summary>
        /// Загрузка запросов на авторство
        /// </summary>
        private void LoadAuthorRequests()
        {
            var requests = GetAuthorRequests() as IEnumerable<dynamic>;
            var items = new List<dynamic>();

            foreach (var request in requests)
            {
                items.Add(new
                {
                    Id = request.RequestID,
                    DisplayText = $"Заявка на роль автора",
                    SubText = $"Пользователь: {request.UserName}",
                    ShowAccept = true,
                    ShowReject = true,
                    ShowAssignRole = false,
                    ShowChangePassword = false,
                    ShowFreezeUser = false,
                    ShowUnfreezeUser = false
                });
            }

            ItemsControl.ItemsSource = items;
        }

        /// <summary>
        /// Загрузка замороженных пользователей/отзывов/книг
        /// </summary>
        private void LoadFrozenItems()
        {
            var frozenBooks = GetFrozenBooks() as IEnumerable<dynamic>;
            var frozenUsers = GetFrozenUsers() as IEnumerable<dynamic>;
            var frozenReviews = GetFrozenReviews() as IEnumerable<dynamic>;

            var items = new List<dynamic>();

            if (frozenBooks != null && frozenBooks.Any())
            {
                foreach (var book in frozenBooks)
                {
                    items.Add(new
                    {
                        Id = book.BookID,
                        DisplayText = $"Замороженная книга: {book.Title}",
                        SubText = book.FreezeReason,
                        ShowAccept = false,
                        ShowReject = false,
                        ShowAssignRole = false,
                        ShowChangePassword = false,
                        ShowFreezeUser = false,
                        ShowUnfreezeUser = false
                    });
                }
            }

            if (frozenUsers != null && frozenUsers.Any())
            {
                foreach (var user in frozenUsers)
                {
                    items.Add(new
                    {
                        Id = user.UserID,
                        DisplayText = $"Замороженный пользователь: {user.Login}",
                        SubText = $"{user.Name} | {user.Email} | Причина: {user.FreezeReason}",
                        ShowAccept = false,
                        ShowReject = false,
                        ShowAssignRole = false,
                        ShowChangePassword = false,
                        ShowFreezeUser = false,
                        ShowUnfreezeUser = false
                    });
                }
            }

            if (frozenReviews != null && frozenReviews.Any())
            {
                foreach (var review in frozenReviews)
                {
                    items.Add(new
                    {
                        Id = review.ReviewID,
                        DisplayText = $"Замороженный отзыв",
                        SubText = $"{review.ReviewText} | Причина: {review.FreezeReason}",
                        ShowAccept = false,
                        ShowReject = false,
                        ShowAssignRole = false,
                        ShowChangePassword = false,
                        ShowFreezeUser = false,
                        ShowUnfreezeUser = false
                    });
                }
            }

            ItemsControl.ItemsSource = items;
        }

        /// <summary>
        /// Загрузка пользователей
        /// </summary>
        private void LoadUsers()
        {
            var users = GetAllUsers() as IEnumerable<dynamic>;
            var items = new List<dynamic>();

            foreach (var user in users)
            {
                items.Add(new
                {
                    Id = user.UserID,
                    DisplayText = user.Login,
                    SubText = $"{user.Name} | {user.Email} | Роль: {user.RoleName}",
                    ShowAccept = false,
                    ShowReject = false,
                    ShowAssignRole = true,
                    ShowChangePassword = true,
                    ShowFreezeUser = !user.IsFrozen,
                    ShowUnfreezeUser = user.IsFrozen
                });
            }

            ItemsControl.ItemsSource = items;
        }

        /// <summary>
        /// Обработчик кнопки "Принять"
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int id = (int)btn.Tag;

            if (CurrentSection == "Appeals")
                AcceptAppeal(id);
            else if (CurrentSection == "Unfreeze")
                AcceptUnfreezePetition(id);
            else if (CurrentSection == "AuthorRequests")
                AcceptAuthorRequest(id);

            LoadSection();
        }

        /// <summary>
        /// Обработчик кнопки "Отклонить"
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int id = (int)btn.Tag;

            if (CurrentSection == "Appeals")
                RejectAppeal(id);
            else if (CurrentSection == "Unfreeze")
                RejectUnfreezePetition(id);
            else if (CurrentSection == "AuthorRequests")
                RejectAuthorRequest(id);

            LoadSection();
        }

        /// <summary>
        /// Обработчик назначения роли
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AssignRole_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int userId = (int)btn.Tag;

            var dialog = new Window
            {
                Title = "Выбор роли",
                Width = 250,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            var comboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
            comboBox.Items.Add("Пользователь");
            comboBox.Items.Add("Автор");
            comboBox.Items.Add("Админ");
            comboBox.SelectedIndex = 0;

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 60 };

            okButton.Click += (s, ev) =>
            {
                string roleName = comboBox.SelectedItem.ToString();
                AssignRole(userId, roleName);
                MessageBox.Show($"Роль '{roleName}' назначена");
                LoadSection();
                dialog.Close();
            };

            cancelButton.Click += (s, ev) => dialog.Close();

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(buttonPanel);
            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Обработчик заморозки пользователя
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void FreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int userId = (int)btn.Tag;

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }

            if (user.IsFrozen)
            {
                MessageBox.Show("Пользователь уже заморожен");
                return;
            }

            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину заморозки:", "Заморозка пользователя");

            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Введите причину заморозки");
                return;
            }

            user.IsFrozen = true;
            user.FreezeReason = reason;
            Core.Context.SaveChanges();

            MessageBox.Show($"Пользователь {user.Login} заморожен");
            LoadSection();
        }

        /// <summary>
        /// Обработчик разморозки
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void UnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int userId = (int)btn.Tag;

            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return;

            if (!user.IsFrozen)
            {
                MessageBox.Show("Пользователь не заморожен");
                return;
            }

            user.IsFrozen = false;
            user.FreezeReason = null;
            Core.Context.SaveChanges();

            MessageBox.Show($"Пользователь {user.Login} разморожен");
            LoadSection();
        }

        /// <summary>
        /// Обработчик кнопки смены пароля
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int userId = (int)btn.Tag;

            string newPassword = Microsoft.VisualBasic.Interaction.InputBox("Введите новый пароль (мин. 6 символов):", "Смена пароля");
            if (!string.IsNullOrEmpty(newPassword))
            {
                ChangeUserPassword(userId, newPassword);
                MessageBox.Show("Пароль изменен");
            }
        }

        /// <summary>
        /// Обработчик переключения RadioButton
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Section_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio != null && radio.IsChecked == true)
            {
                if (radio == rbAppeals) CurrentSection = "Appeals";
                else if (radio == rbUnfreeze) CurrentSection = "Unfreeze";
                else if (radio == rbAuthorRequests) CurrentSection = "AuthorRequests";
                else if (radio == rbFrozen) CurrentSection = "Frozen";
                else if (radio == rbUsers) CurrentSection = "Users";

                LoadSection();
            }
        }
    }
}

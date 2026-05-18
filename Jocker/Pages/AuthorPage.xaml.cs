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
    /// Логика взаимодействия для AuthorPage.xaml
    /// </summary>
    public partial class AuthorPage : Page
    {
        private int CurrentUserId { get; set; }
        private string CurrentListType { get; set; }
        private string CurrentUserRole { get; set; }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        /// <param name="currentUserId"></param>
        public AuthorPage(int currentUserId, string currentUserRole)
        {
            InitializeComponent();
            CurrentUserId = currentUserId;
            CurrentListType = "Опубликованные";
            CurrentUserRole = currentUserRole;

            this.Loaded += Page_Loaded;
        }

        /// <summary>
        /// Получение ID автора через пользователя
        /// </summary>
        /// <returns></returns>
        public int GetAuthorId()
        {
            var author = Core.Context.Authors
                .FirstOrDefault(a => a.UserID == CurrentUserId);

            return author?.AuthorID ?? 0;
        }

        /// <summary>
        /// Проверка, является ли пользователь автором
        /// </summary>
        /// <returns>ID пользовавтеля</returns>
        public bool IsUserAuthor()
        {
            return Core.Context.Authors.Any(a => a.UserID == CurrentUserId);
        }

        /// <summary>
        /// Получение опубликованных книг автора
        /// </summary>
        /// <returns>Книги</returns>
        public object GetPublishedBooks()
        {
            var authorId = GetAuthorId();
            if (authorId == 0)
                return new List<object>();

            var books = Core.Context.Books
                .Where(b => b.AuthorID == authorId && !b.IsFrozen)
                .Select(b => new
                {
                    b.BookID,
                    b.Title,
                    b.Description,
                    b.Rating,
                    b.CoverImage,
                    ShowAppealButton = false,
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .OrderBy(b => b.Title)
                .ToList();

            return books;
        }

        /// <summary>
        /// Навигация к странице добавления книги
        /// </summary>
        public void OpenAddBookPage()
        {
            NavigationService?.Navigate(new AddBookPage(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Навигация к странице редактирования книги
        /// </summary>
        /// <param name="bookId">ID книги</param>
        public void OpenEditBookPage(int bookId)
        {
            NavigationService?.Navigate(new EditBookPage(CurrentUserId, CurrentUserRole, bookId));
        }

        /// <summary>
        /// Получение замороженных книг
        /// </summary>
        /// <returns>Книги</returns>
        public object GetFrozenBooks()
        {
            var authorId = GetAuthorId();
            if (authorId == 0)
                return new List<object>();

            var books = Core.Context.Books
                .Where(b => b.AuthorID == authorId && b.IsFrozen)
                .Select(b => new
                {
                    b.BookID,
                    b.Title,
                    b.Description,
                    b.Rating,
                    b.CoverImage, 
                    b.FreezeReason,
                    ShowAppealButton = true,
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList();

            return books;
        }

        /// <summary>
        /// Подача аппеляции для разморозки книги
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="reason"></param>
        public void AppealBookFreeze(int bookId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                Console.WriteLine("Укажите причину оспаривания");
                return;
            }

            var authorId = GetAuthorId();
            if (authorId == 0)
            {
                Console.WriteLine("Автор не найден");
                return;
            }

            var book = Core.Context.Books
                .FirstOrDefault(b => b.BookID == bookId && b.AuthorID == authorId);

            if (book == null || !book.IsFrozen)
            {
                Console.WriteLine("Книга не заморожена");
                return;
            }

            var existingPetition = Core.Context.Petitions
                .FirstOrDefault(p => p.UserID == CurrentUserId && p.TargetID == bookId);

            if (existingPetition != null)
            {
                MessageBox.Show("Вы уже подавали заявку на разморозку этой книги!");
                return;
            }

            var statusPending = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "На рассмотрении");

            var petition = new Petitions
            {
                UserID = CurrentUserId,
                TargetID = bookId,
                Reason = reason,
                StatusID = statusPending?.StatusID ?? 1,
            };

            Core.Context.Petitions.Add(petition);
            Core.Context.SaveChanges();

            MessageBox.Show("Заявка на разморозку книги отправлена");
        }

        /// <summary>
        /// Удаление книги
        /// </summary>
        /// <param name="bookId">ID книги</param>
        public void DeleteBook(int bookId)
        {
            var authorId = GetAuthorId();

            var book = Core.Context.Books.FirstOrDefault(b => b.BookID == bookId && b.AuthorID == authorId);

            if (book == null)
            {
                Console.WriteLine("Книга не найдена");
                return;
            }

            var bookGenres = Core.Context.BookGenres.Where(bg => bg.BookID == bookId).ToList();
            Core.Context.BookGenres.RemoveRange(bookGenres);

            var reviews = Core.Context.Reviews.Where(r => r.BookID == bookId).ToList();
            Core.Context.Reviews.RemoveRange(reviews);

            var bookLists = Core.Context.BookLists.Where(bl => bl.BookID == bookId).ToList();
            Core.Context.BookLists.RemoveRange(bookLists);

            var appeals = Core.Context.Appeals
                .Where(a => a.TargetID == bookId && a.Targets.TargetName == "Книга")
                .ToList();
            Core.Context.Appeals.RemoveRange(appeals);

            var petitions = Core.Context.Petitions
                .Where(p => p.TargetID == bookId && p.Targets.TargetName == "Книга")
                .ToList();
            Core.Context.Petitions.RemoveRange(petitions);

            Core.Context.Books.Remove(book);
            Core.Context.SaveChanges();

            Console.WriteLine($"Книга удалена");
        }

        /// <summary>
        /// Загрузка страницы
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        /// <summary>
        /// Загрузка книг
        /// </summary>
        private void LoadBooks()
        {
            if (BooksControl == null) return;

            if (CurrentListType == "Опубликованные")
            {
                var books = GetPublishedBooks();
                BooksControl.ItemsSource = books as System.Collections.IEnumerable;
            }
            else
            {
                var books = GetFrozenBooks();
                BooksControl.ItemsSource = books as System.Collections.IEnumerable;
            }
        }

        /// <summary>
        /// Вспомогательный метод для выбора книг (Опубликованные/Замороженные)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void BookStatus_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio != null && radio.IsChecked == true)
            {
                CurrentListType = radio.Content.ToString();
                LoadBooks();
            }
        }

        /// <summary>
        /// Обработчик кнопки добавления книги (переход на страницу AddBookPage)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            OpenAddBookPage();
        }

        /// <summary>
        /// Обработчик кнопки редактирования книги (переход на страницу EditBookPage)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int bookId = (int)button.Tag;
            OpenEditBookPage(bookId);
        }

        /// <summary>
        /// Удаленик ниги
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int bookId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту книгу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteBook(bookId);
                LoadBooks();
                MessageBox.Show("Книга удалена");
            }
        }

        /// <summary>
        /// Обрабочтик кнопки оспаривания заморозки
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int bookId = (int)button.Tag;

            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину оспаривания:", "Оспаривание заморозки");

            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Введите причину оспаривания");
                return;
            }

            AppealBookFreeze(bookId, reason);
        }

        /// <summary>
        /// Обработчик нажатия на книгу (открытие страницы нажатой книги)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void BookCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                int bookId = (int)border.Tag;
                NavigationService?.Navigate(new BookPage(CurrentUserId, CurrentUserRole, bookId));
            }
        }
    }
}

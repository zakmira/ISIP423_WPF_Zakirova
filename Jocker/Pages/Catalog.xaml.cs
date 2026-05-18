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
    /// Логика взаимодействия для Catalog.xaml
    /// </summary>
    public partial class Catalog : Page
    {
        private int CurrentUserId { get; set; }
        private string CurrentUserRole { get; set; }

        /// <summary>
        /// Поулчение данных из БД
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="userRole">Роль пользователя (необохдима для перехода на BookPage)</param>
        public Catalog(int userId, string userRole)
        {
            InitializeComponent();
            CurrentUserId = userId;
            CurrentUserRole = userRole;

            this.Loaded += Page_Loaded;
        }

        /// <summary>
        /// Получение всех книг
        /// </summary>
        /// <returns>Все книги из БД</returns>
        public List<dynamic> GetAllBooks()
        {
            var books = Core.Context.Books
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    CoverImage = b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList(),
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Получение книги по ее ID
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Книгу</returns>
        public List<dynamic> GetBookById(int bookId)
        {
            var book = Core.Context.Books
                .Where(b => b.BookID == bookId)
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Description,
                    b.Rating,
                    b.CoverImage,
                    b.IsFrozen,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                            bg => bg.GenreID,
                            g => g.GenreID,
                           (bg, g) => g.GenreName)
                        .ToList()
                })
                .FirstOrDefault<dynamic>();

            return book;
        }

        /// <summary>
        /// Переход на страницу книги
        /// В BookPage был добавлен параметр bookId
        /// </summary>
        /// <param name="bookId">Передается ID книги</param>
        public void OpenBookPage(int bookId)
        {
            NavigationService?.Navigate(new BookPage(CurrentUserId, CurrentUserRole, bookId));
        }

        /// <summary>
        /// Добавление книги в список
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="bookId">ID книги</param>
        /// <param name="listTypeName">Название списка</param>
        public void AddBookToList(int userId, int bookId, string listTypeName)
        {
            var listType = Core.Context.ListTypes.FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
            {
                Console.WriteLine($"Тип списка '{listTypeName}' не найден");
                return;
            }

            var existing = Core.Context.BookLists
                .FirstOrDefault(bl => bl.UserID == userId && bl.BookID == bookId &&
                                bl.ListTypeID == listType.ListTypeID);

            if (existing != null)
            {
                Console.WriteLine("Книга уже есть в этом списке");
                return;
            }

            var newEntry = new BookLists
            {
                UserID = userId,
                BookID = bookId,
                ListTypeID = listType.ListTypeID
            };

            Core.Context.BookLists.Add(newEntry);
            Core.Context.SaveChanges();

            Console.WriteLine($"Книга добавлена в список '{listTypeName}'");
        }

        /// <summary>
        /// Поиск книги по названию
        /// </summary>
        /// <param name="title">Название книги</param>
        /// <returns>Книгу</returns>
        public List<dynamic> SearchByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return GetAllBooks();

            var books = Core.Context.Books
                .Where(b => b.Title.Contains(title))
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Поиск книги по автору
        /// </summary>
        /// <param name="authorName">Имя автора</param>
        /// <returns>Книгу</returns>
        public List<dynamic> SearchByAuthor(string authorName)
        {
            if (string.IsNullOrEmpty(authorName))
                return GetAllBooks();

            var authorIds = Core.Context.Authors
                .Where(a => a.AuthorName.Contains(authorName))
                .Select(a => a.AuthorID)
                .ToList();

            var books = Core.Context.Books
                .Where(b => authorIds.Contains(b.AuthorID))
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Сортировка в алфавитном порядке
        /// </summary>
        /// <returns>Книги от А до Я</returns>
        public List<dynamic> SortByNameAsc()
        {
            var books = Core.Context.Books
                .OrderBy(b => b.Title)
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Сортировка в обратном порядке
        /// </summary>
        /// <returns>Книги от Я до А </returns>
        public List<dynamic> SortByNameDesc()
        {
            var books = Core.Context.Books
                .OrderByDescending(b => b.Title)
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Сортировка по рейтингу
        /// </summary>
        /// <returns>Книги с рейтингом по убыванию</returns>
        public List<dynamic> SortByRatingAsc()
        {
            var books = Core.Context.Books
                .OrderBy(b => b.Rating)
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Сортировка по рейтингу
        /// </summary>
        /// <returns>Книги с рейтингом по возрастанию</returns>
        public List<dynamic> SortByRatingDesc()
        {
            var books = Core.Context.Books
                .OrderByDescending(b => b.Rating)
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Фильтрация по жанру
        /// </summary>
        /// <param name="genreName">Название жанра</param>
        /// <returns></returns>
        public List<dynamic> FilterByGenre(string genreName)
        {
            if (string.IsNullOrEmpty(genreName))
                return GetAllBooks();

            var bookIdsWithGenre = Core.Context.BookGenres
                .Join(Core.Context.Genres,
                    bg => bg.GenreID,
                    g => g.GenreID,
                    (bg, g) => new { bg.BookID, g.GenreName })
                .Where(x => x.GenreName == genreName)
                .Select(x => x.BookID)
                .ToList();

            var books = Core.Context.Books
                .Where(b => bookIdsWithGenre.Contains(b.BookID))
                .Select(b => new
                {
                    b.BookID,
                    b.AuthorID,
                    b.Title,
                    b.Rating,
                    b.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList<dynamic>();

            return books;
        }

        /// <summary>
        /// Вспомогательный метод получения всех жанров
        /// </summary>
        /// <returns>Все жанры</returns>
        public List<string> GetAllGenreNames()
        {
            var genres = Core.Context.Genres
                .Select(g => g.GenreName)
                .ToList();

            return genres;
        }

        /// <summary>
        /// Загрузка страницы (методов сортировки и жанров)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Sort.Items.Add("По названию (А-Я)");
            Sort.Items.Add("По названию (Я-А)");
            Sort.Items.Add("По рейтингу (сначала высокие)");
            Sort.Items.Add("По рейтингу (сначала низкие)");
            Sort.SelectedIndex = 0;

            var genres = GetAllGenreNames();
            genres.Insert(0, "Все жанры");
            Genre.ItemsSource = genres;
            Genre.SelectedIndex = 0;

            LoadBooks();
        }

        /// <summary>
        /// Загрузка книг на страницу
        /// </summary>
        private void LoadBooks()
        {
            var books = GetAllBooks();
            BooksControl.ItemsSource = books;
        }

       /// <summary>
       /// Отображение книг
       /// </summary>
       /// <param name="books"></param>
        private void DisplayBooks(List<dynamic> books)
        {
            BooksControl.ItemsSource = books;
        }

        /// <summary>
        /// Поиск книг
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string title = SearchTitle.Text;
            string author = SearchAuthor.Text;

            var results = GetAllBooks();

            if (!string.IsNullOrEmpty(title))
                results = SearchByTitle(title);

            if (!string.IsNullOrEmpty(author))
                results = SearchByAuthor(author);

            DisplayBooks(results);
        }

        /// <summary>
        /// Обработчик сортировки
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = Sort.SelectedIndex;
            List<dynamic> results = null;

            switch (index)
            {
                case 0: results = SortByNameAsc(); break;
                case 1: results = SortByNameDesc(); break;
                case 2: results = SortByRatingDesc(); break;
                case 3: results = SortByRatingAsc(); break;
            }

            DisplayBooks(results);
        }

        /// <summary>
        /// Обработчик выбора жанра
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Genre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string genre = Genre.SelectedItem as string;

            if (genre == null || genre == "Все жанры")
            {
                LoadBooks();
            }
            else
            {
                DisplayBooks(FilterByGenre(genre));
            }
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
                OpenBookPage(bookId);
            }
        }

        /// <summary>
        /// Обработчик кнопки добавления книги в список
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            dynamic book = button.CommandParameter;

            var contextMenu = new ContextMenu();
            var listTypes = Core.Context.ListTypes.ToList();

            foreach (var listType in listTypes)
            {
                var menuItem = new MenuItem
                {
                    Header = listType.ListTypeName,
                    Tag = new { BookID = book.BookID, ListTypeName = listType.ListTypeName }
                };
                menuItem.Click += AddBookToList_Click;
                contextMenu.Items.Add(menuItem);
            }

            button.ContextMenu = contextMenu;
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// Добавление книги в выбранный список
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AddBookToList_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            dynamic data = menuItem.Tag;
            AddBookToList(CurrentUserId, data.BookID, data.ListTypeName);
        }
    }
}
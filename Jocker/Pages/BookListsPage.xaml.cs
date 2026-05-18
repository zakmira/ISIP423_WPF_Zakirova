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
    /// Логика взаимодействия для BookListsPage.xaml
    /// </summary>
    public partial class BookListsPage : Page
    {
        private int CurrentUserId { get; set; }
        private string CurrentListType { get; set; }
        private string CurrentUserRole { get; set; }

        public BookListsPage(int userId, string userRole)
        {
            InitializeComponent();
            CurrentUserId = userId;
            CurrentListType = "Читаю";
            CurrentUserRole = userRole;

            this.Loaded += Page_Loaded;
        }

        /// <summary>
        /// Получение всех списков книг пользователя
        /// </summary>
        /// <returns>Все списки пользователя</returns>
        public object GetAllUserBooks()
        {
            var allBookLists = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId)
                .Select(bl => new
                {
                    bl.BookID,
                    bl.ListTypeID,
                    ListTypeName = bl.ListTypes.ListTypeName,
                    BookTitle = bl.Books.Title,
                    BookRating = bl.Books.Rating,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == bl.Books.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == bl.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .ToList();

            return allBookLists;
        }

        /// <summary>
        /// Вспомогательный метод получения всех типов списков книг
        /// </summary>
        /// <returns>Типы списков</returns>
        public List<string> GetListTypes()
        {
            var listTypes = Core.Context.ListTypes
                .Select(lt => lt.ListTypeName)
                .ToList();

            return listTypes;
        }

        /// <summary>
        /// Просмотр книг из конкретного списка
        /// </summary>
        /// <param name="listTypeName">Название списка</param>
        /// <returns>Книги из этого списка</returns>
        public object GetBooksByListType(string listTypeName)
        {
            var listType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
                return new List<object>();

            var books = Core.Context.BookLists
                 .Where(bl => bl.UserID == CurrentUserId && bl.ListTypeID == listType.ListTypeID)
                 .Select(bl => new
                 {
                     bl.BookID,
                     bl.Books.Title,
                     bl.Books.Rating,
                     bl.Books.CoverImage,
                     AuthorName = Core.Context.Authors
                         .Where(a => a.AuthorID == bl.Books.AuthorID)
                         .Select(a => a.AuthorName)
                         .FirstOrDefault() ?? "Неизвестный автор",
                     Genres = Core.Context.BookGenres
                         .Where(bg => bg.BookID == bl.BookID)
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
        /// Получение списка, в котором находится книга
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Список книг</returns>
        public string GetBookListType(int bookId)
        {
            var bookList = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId && bl.BookID == bookId)
                .Select(bl => bl.ListTypes.ListTypeName)
                .FirstOrDefault();

            return bookList ?? "Не в списках";
        }

        /// <summary>
        /// Получение кол-ва книг в каждом списке
        /// </summary>
        /// <returns>Количество</returns>
        public object GetBookInListCounts()
        {
           var counts = Core.Context.ListTypes
                .Select(lt => new
                {
                    lt.ListTypeName,
                    Count = Core.Context.BookLists
                        .Count(bl => bl.UserID == CurrentUserId && bl.ListTypeID == lt.ListTypeID)
                })
                .ToList();

            return counts;
        }

        /// <summary>
        /// Перемещение книги в другой список
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <param name="newListTypeName">Новый список, в котором находится книга</param>
        public void MoveBookToList(int bookId, string newListTypeName)
        {
            var newListType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == newListTypeName);

            if (newListType == null)
            {
                Console.WriteLine($"Тип списка '{newListTypeName}' не найден");
                return;
            }

            var existingEntry = Core.Context.BookLists
                .FirstOrDefault(bl => bl.UserID == CurrentUserId && bl.BookID == bookId);

            if (existingEntry == null)
            {
                var newEntry = new BookLists
                {
                    UserID = CurrentUserId,
                    BookID = bookId,
                    ListTypeID = newListType.ListTypeID
                };
                Core.Context.BookLists.Add(newEntry);
            }
            else
            {
                existingEntry.ListTypeID = newListType.ListTypeID;
            }

            Core.Context.SaveChanges();
            Console.WriteLine($"Книга перемещена в список '{newListTypeName}'");
        }

        /// <summary>
        /// Поиск книги по названию
        /// </summary>
        /// <param name="listTypeName">Название списка, в котором ищется книга</param>
        /// <param name="title">Название книги</param>
        /// <returns></returns>
        public object SearchByTitle(string listTypeName, string title)
        {
            var listType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
                return new List<object>();

            if (string.IsNullOrEmpty(title))
                return GetBooksByListType(listTypeName);

            var books = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId &&
                             bl.ListTypeID == listType.ListTypeID &&
                             bl.Books.Title.Contains(title))
                .Select(bl => new
                {
                    bl.BookID,
                    bl.Books.Title,
                    bl.Books.Rating,
                    bl.Books.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == bl.Books.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == bl.BookID)
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
        /// Поиск по автору
        /// </summary>
        /// <param name="listTypeName">Список, в котором ищется книга</param>
        /// <param name="authorName">Имя автора</param>
        /// <returns></returns>
        public object SearchByAuthor(string listTypeName, string authorName)
        {
            var listType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
                return new List<object>();

            if (string.IsNullOrEmpty(authorName))
                return GetBooksByListType(listTypeName);

            var authorIds = Core.Context.Authors
                .Where(a => a.AuthorName.Contains(authorName))
                .Select(a => a.AuthorID)
                .ToList();

            var books = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId &&
                             bl.ListTypeID == listType.ListTypeID &&
                             authorIds.Contains(bl.Books.AuthorID))
                .Select(bl => new
                {
                    bl.BookID,
                    bl.Books.Title,
                    bl.Books.Rating,
                    bl.Books.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == bl.Books.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == bl.BookID)
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
        /// Сортировка по имени
        /// </summary>
        /// <param name="listTypeName">Название списка</param>
        /// <param name="ascending">Сортировка в алфавитном порядке</param>
        /// <returns>Отсортированный список книг</returns>
        public object SortByName(string listTypeName, bool ascending = true)
        {
            var listType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
                return new List<object>();

            var query = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId && bl.ListTypeID == listType.ListTypeID)
                .Select(bl => new
                {
                    bl.BookID,
                    bl.Books.Title,
                    bl.Books.Rating,
                    bl.Books.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == bl.Books.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",

                    Genres = Core.Context.BookGenres
                         .Where(bg => bg.BookID == bl.BookID)
                         .Join(Core.Context.Genres,
                             bg => bg.GenreID,
                             g => g.GenreID,
                             (bg, g) => g.GenreName)
                         .ToList()
                });

            var result = ascending
                ? query.OrderBy(x => x.Title).ToList()   
                : query.OrderByDescending(x => x.Title).ToList(); 

            return result;
        }

        /// <summary>
        /// Сортировка по рейтингу
        /// </summary>
        /// <param name="listTypeName">Название списка</param>
        /// <param name="descending">Сортировка по возрастанию</param>
        /// <returns></returns>
        public object SortByRating(string listTypeName, bool descending = true)
        {
            var listType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
                return new List<object>();

            var query = Core.Context.BookLists
                 .Where(bl => bl.UserID == CurrentUserId && bl.ListTypeID == listType.ListTypeID)
                 .Select(bl => new
                 {
                     bl.BookID,
                     bl.Books.Title,
                     bl.Books.Rating,
                     bl.Books.CoverImage,
                     AuthorName = Core.Context.Authors
                         .Where(a => a.AuthorID == bl.Books.AuthorID)
                         .Select(a => a.AuthorName)
                         .FirstOrDefault() ?? "Неизвестный автор",
                     Genres = Core.Context.BookGenres
                         .Where(bg => bg.BookID == bl.BookID)
                         .Join(Core.Context.Genres,
                               bg => bg.GenreID,
                               g => g.GenreID,
                               (bg, g) => g.GenreName)
                         .ToList()
                 });

            var result = descending
                ? query.OrderByDescending(x => x.Rating).ToList() 
                : query.OrderBy(x => x.Rating).ToList();

            return result;
        }

        /// <summary>
        /// Вспомогательный метод получения всех жанров
        /// </summary>
        /// <returns>Жанры</returns>
        public List<string> GetAllGenres()
        {
            var genres = Core.Context.Genres
                .Select(g => g.GenreName)
                .ToList();

            return genres;
        }

        /// <summary>
        /// Фильтрация по жанрам
        /// </summary>
        /// <param name="listTypeName">Название списка</param>
        /// <param name="genreName">Название жанра</param>
        /// <returns></returns>
        public object FilterByGenre(string listTypeName, string genreName)
        {
            var listType = Core.Context.ListTypes
                .FirstOrDefault(lt => lt.ListTypeName == listTypeName);

            if (listType == null)
                return new List<object>();

            if (string.IsNullOrEmpty(genreName))
                return GetBooksByListType(listTypeName);

            var bookIdWithGenre = Core.Context.BookGenres
                .Join(Core.Context.Genres,
                      bg => bg.GenreID,
                      g => g.GenreID,
                      (bg, g) => new { bg.BookID, g.GenreName })
                .Where(x => x.GenreName == genreName)
                .Select(x => x.BookID)
                .ToList();

            var books = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId &&
                             bl.ListTypeID == listType.ListTypeID &&
                             bookIdWithGenre.Contains(bl.BookID))
                .Select(bl => new
                {
                    bl.BookID,
                    bl.Books.Title,
                    bl.Books.Rating,
                    bl.Books.CoverImage,
                    AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == bl.Books.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                    Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == bl.BookID)
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
        /// Удаление книги из всех списков
        /// </summary>
        /// <param name="bookId">ID книги</param>
        public void RemoveBook(int bookId)
        {
            var entries = Core.Context.BookLists
                .Where(bl => bl.UserID == CurrentUserId && bl.BookID == bookId)
                .ToList();

            if (entries.Any())
            {
                Core.Context.BookLists.RemoveRange(entries);
                Core.Context.SaveChanges();
                Console.WriteLine("Книга удалена из всех списков");
            }
            else
            {
                Console.WriteLine("Книга не найдена в списках");
            }
        }

        /// <summary>
        /// Загрузка сортировок и жанров
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

            var genres = GetAllGenres();
            genres.Insert(0, "Все жанры");
            Genre.ItemsSource = genres;
            Genre.SelectedIndex = 0;

            LoadBooks();
        }

        /// <summary>
        /// Вспомогательный метод загрузки книг
        /// </summary>
        private void LoadBooks()
        {
            var books = GetBooksByListType(CurrentListType);
            BooksControl.ItemsSource = books as System.Collections.IEnumerable;
        }

        /// <summary>
        /// Обработка выбора RadioButton
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void ListType_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio != null && radio.IsChecked == true)
            {
                CurrentListType = radio.Content.ToString();
                LoadBooks();
            }
        }

        /// <summary>
        /// Обработка кнопки поиска
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string title = SearchTitle.Text;
            string author = SearchAuthor.Text;

            object results = GetBooksByListType(CurrentListType);

            if (!string.IsNullOrEmpty(title))
                results = SearchByTitle(CurrentListType, title);

            if (!string.IsNullOrEmpty(author))
                results = SearchByAuthor(CurrentListType, author);

            BooksControl.ItemsSource = results as System.Collections.IEnumerable;
        }

        /// <summary>
        /// Обработка кнопки сортировки
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = Sort.SelectedIndex;
            object results = null;

            switch (index)
            {
                case 0:
                    results = SortByName(CurrentListType, true);
                    break;
                case 1:
                    results = SortByName(CurrentListType, false);
                    break;
                case 2:
                    results = SortByRating(CurrentListType, true);
                    break;
                case 3:
                    results = SortByRating(CurrentListType, false);
                    break;
            }

            BooksControl.ItemsSource = results as System.Collections.IEnumerable;
        }

        /// <summary>
        /// Обработка выбора жанра
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
                var results = FilterByGenre(CurrentListType, genre);
                BooksControl.ItemsSource = results as System.Collections.IEnumerable;
            }
        }

        /// <summary>
        /// Обработка выбора нового списка для книги
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void MoveToList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || combo.SelectedItem == null) return;

            int bookId = (int)combo.Tag;
            string selectedItem = (combo.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedItem == "Удалить из списков")
            {
                RemoveBook(bookId);
                MessageBox.Show("Книга удалена из всех списков");
            }
            else
            {
                MoveBookToList(bookId, selectedItem);
                MessageBox.Show($"Книга перемещена в список '{selectedItem}'");
            }

            combo.SelectedItem = null;
            LoadBooks();
        }

        /// <summary>
        /// Переход на страницу книги при нажатии на "окошко"
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
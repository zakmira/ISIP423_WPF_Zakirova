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

namespace Jocker.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditBookPage.xaml
    /// </summary>
    public partial class EditBookPage : Page
    {
        private int CurrentUserId { get; set; }
        private int CurrentBookId { get; set; }
        private string CurrentUserRole { get; set; }
        private List<int> SelectedGenreIds { get; set; }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="currentUserRole">Его роль</param>
        /// <param name="bookId">ID книги</param>
        public EditBookPage(int userId, string currentUserRole, int bookId)
        {
            InitializeComponent();
            CurrentUserId = userId;
            CurrentBookId = bookId;
            CurrentUserRole = currentUserRole;
            SelectedGenreIds = new List<int>();

            LoadGenres();
            LoadBookData();
            
        }

        /// <summary>
        /// Загрузка жанров
        /// </summary>
        private void LoadGenres()
        {
            var genres = Core.Context.Genres.Select(g => new { g.GenreID, g.GenreName }).ToList();
            lstGenres.ItemsSource = genres;
        }

        /// <summary>
        /// Загрузка книг
        /// </summary>
        private void LoadBookData()
        {
            var book = Core.Context.Books
                .Where(b => b.BookID == CurrentBookId)
                .Select(b => new
                {
                    b.Title,
                    b.Description
                })
                .FirstOrDefault();

            if (book != null)
            {
                Title.Text = book.Title;
                Description.Text = book.Description;
            }
        }

        /// <summary>
        /// Вспомогательный метод для выбора жанра из списка
        /// </summary>
        /// <returns>Все жанры</returns>
        public object GetAllGenres()
        {
            return Core.Context.Genres.Select(g => new { g.GenreID, g.GenreName }).ToList();
        }

        /// <summary>
        /// Сохранение изменений
        /// </summary>
        /// <param name="newTitle">Новое название</param>
        /// <param name="newDescription">Новое описание</param>
        /// <param name="newGenreIds">Новый жанр (или жанры)</param>
        public void SaveChanges(string newTitle, string newDescription, List<int> newGenreIds)
        {
            var author = Core.Context.Authors
               .FirstOrDefault(a => a.UserID == CurrentUserId);

            if (author == null)
            {
                Console.WriteLine("Вы не являетесь автором");
                return;
            }

            var book = Core.Context.Books
                .FirstOrDefault(b => b.BookID == CurrentBookId && b.AuthorID == author.AuthorID);

            if (book == null)
            {
                Console.WriteLine("Книга не найдена");
                return;
            }

            if (!string.IsNullOrEmpty(newTitle))
                book.Title = newTitle;

            if (newDescription != null)
                book.Description = newDescription;

            var existingGenres = Core.Context.BookGenres
                .Where(bg => bg.BookID == CurrentBookId)
                .ToList();

            Core.Context.BookGenres.RemoveRange(existingGenres);

            foreach (var genreId in newGenreIds)
            {
                Core.Context.BookGenres.Add(new BookGenres
                {
                    BookID = CurrentBookId,
                    GenreID = genreId
                });
            }

            Core.Context.SaveChanges();

            Console.WriteLine($"Книга '{book.Title}' обновлена");
        }

        /// <summary>
        /// Обработчик "галочки" на выборе жанра
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Genre_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag != null)
            {
                int genreId = (int)checkBox.Tag;
                if (!SelectedGenreIds.Contains(genreId))
                    SelectedGenreIds.Add(genreId);
            }
        }

        /// <summary>
        /// Обработчик снятия "галочки" на выборе жанра
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Genre_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag != null)
            {
                int genreId = (int)checkBox.Tag;
                if (SelectedGenreIds.Contains(genreId))
                    SelectedGenreIds.Remove(genreId);
            }
        }

        /// <summary>
        /// Обработчик сохранения изменений
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newTitle = Title.Text.Trim();
            string newDescription = Description.Text.Trim();

            if (string.IsNullOrEmpty(newTitle))
            {
                MessageBox.Show("Введите название книги");
                return;
            }

            SaveChanges(newTitle, newDescription, SelectedGenreIds);
            MessageBox.Show("Книга обновлена");

            NavigationService?.Navigate(new AuthorPage(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Обработчик отмены изменений
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthorPage(CurrentUserId, CurrentUserRole));
        }
    }
}

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
    /// Логика взаимодействия для AddBook.xaml
    /// </summary>
    public partial class AddBookPage : Page
    {
        private int CurrentUserId { get; set; }
        private string CurrentUserRole { get; set; }
        private List<int> SelectedGenreIds { get; set; }

        /// <summary>
        /// Загрузка данных из БД
        /// </summary>
        /// <param name="currentUserId">Пользователь в настоящий момент</param>
        /// <param name="currentUserRole">Роль этого пользователя</param>
        public AddBookPage(int currentUserId, string currentUserRole)
        {
            InitializeComponent();
            CurrentUserId = currentUserId;
            SelectedGenreIds = new List<int>();
            LoadGenres();
            CurrentUserRole = currentUserRole;
        }

        /// <summary>
        /// Метод необходим для привязки будущего визуала к управлению
        /// </summary>
        private void LoadGenres()
        {
            var genres = GetAllGenres();
            lstGenres.ItemsSource = genres;
        }

        /// <summary>
        /// Добавление книги и сохранение изменений в БД
        /// </summary>
        /// <param name="title">Название книги</param>
        /// <param name="description">Описание книги</param>
        /// <param name="genreIds">Доступные жанры (определяются по ID)</param>
        /// <param name="coverImage">Обложка книги</param>
        public void AddBook(string title, string description, List<int> genreIds, string coverImage = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название книги");
                return;
            }

            var author = Core.Context.Authors.FirstOrDefault(a => a.UserID == CurrentUserId);
            int authorId = author?.AuthorID ?? 0;
            
            if (authorId == 0)
            {
                MessageBox.Show("Вы не являетесь автором");
                return;
            }

            var newBook = new Books
            {
                Title = title,
                Description = description,
                CoverImage = coverImage ?? "",
                AuthorID = authorId,
                Rating = 0,
                IsFrozen = false
            };

            Core.Context.Books.Add(newBook);
            Core.Context.SaveChanges();

            foreach (var genreId in genreIds)
            {
                Core.Context.BookGenres.Add(new BookGenres
                {
                    BookID = newBook.BookID,
                    GenreID = genreId
                });
            }
            Core.Context.SaveChanges();

            MessageBox.Show($"Книга '{title}' добавлена!");

            NavigationService?.Navigate(new AuthorPage(CurrentUserId, CurrentUserRole));
        }

        /// <summary>
        /// Вспомогающий метод для вывода всех жанров
        /// </summary>
        /// <returns>Жанры книг</returns>
        public List<dynamic> GetAllGenres()
        {
            return Core.Context.Genres.Select(g => new { g.GenreID, g.GenreName }).ToList<dynamic>();
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
        /// Обработчик кнопки "Сохранить"
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = Title.Text.Trim();
            string description = Description.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название книги");
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Введите описание");
                return;
            }

            if (SelectedGenreIds.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один жанр");
                return;
            }

            AddBook(title, description, SelectedGenreIds);
        }

        /// <summary>
        /// Обрабочтик кнопки "Отмена" (возврат на страницу автора)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthorPage(CurrentUserId, CurrentUserRole));
        }
    }
}

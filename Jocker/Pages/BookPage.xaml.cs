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
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        private int CurrentUserId { get; set; }
        private string CurrentUserRole { get; set; }
        private int CurrentBookId { get; set; }

        /// <summary>
        /// Получение данных из БД
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="userRole">Роль пользователя</param>
        public BookPage(int userId, string userRole, int bookId)
        {
            InitializeComponent();

            CurrentUserId = userId;
            CurrentUserRole = userRole;
            CurrentBookId = bookId;

            LoadBookData();
            Loaded += Page_Loaded;
        }

        /// <summary>
        /// Получение информации о книге
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Информацию о книге</returns>
        public object GetBookInfo(int bookId)
        {
            var book = Core.Context.Books
                 .Where(b => b.BookID == bookId)
                 .Select(b => new
                 {
                     b.BookID,
                     b.Title,
                     b.Description,
                     b.Rating,
                     b.CoverImage,
                     b.IsFrozen,
                     b.FreezeReason,
                     AuthorName = Core.Context.Authors
                        .Where(a => a.AuthorID == b.AuthorID)
                        .Select(a => a.AuthorName)
                        .FirstOrDefault() ?? "Неизвестный автор",
                     b.AuthorID,
                     Genres = Core.Context.BookGenres
                        .Where(bg => bg.BookID == b.BookID)
                        .Join(Core.Context.Genres,
                              bg => bg.GenreID,
                              g => g.GenreID,
                              (bg, g) => g.GenreName)
                        .ToList()
                })
                .FirstOrDefault();

            return book;
        }

        /// <summary>
        /// Жалоба на книгу
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <param name="reason">Причина жалобы</param>
        public void ComplainBook(int bookId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                Console.WriteLine("Укажите причину жалобы");
                return;
            }

            int authorId = GetAuthorIdByBook(bookId);
            var author = Core.Context.Authors.FirstOrDefault(a => a.AuthorID == authorId);
            if (author != null && author.UserID == CurrentUserId)
            {
                MessageBox.Show("Зачем вы жалуетесь на свою же книгу?");
                return;
            }

            var existing = Core.Context.Appeals
                .FirstOrDefault(a => a.UserID == CurrentUserId &&
                                a.ObjectID == bookId &&
                                a.TargetID == 1);

            if (existing != null)
            {
                MessageBox.Show("Вы уже подавали жалобу на эту книгу");
                return;
            }

            var statusPending = Core.Context.Statuses
               .FirstOrDefault(s => s.StatusName == "На рассмотрении");

            var targetBook = Core.Context.Targets
                .FirstOrDefault(t => t.TargetName == "Книга");

            var appeal = new Appeals
            {
                UserID = CurrentUserId,
                TargetID = 1, 
                ObjectID = bookId, 
                Text = reason,
                StatusID = statusPending?.StatusID ?? 1
            };

            Core.Context.Appeals.Add(appeal);
            Core.Context.SaveChanges();

            MessageBox.Show("Жалоба на книгу отправлена");
        }

        /// <summary>
        /// Жалоба на автора
        /// </summary>
        /// <param name="authorId">ID автора</param>
        /// <param name="reason">Причина жалобы</param>
        public void ComplainAuthor(int authorId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Укажите причину жалобы");
                return;
            }


            var author = Core.Context.Authors.FirstOrDefault(a => a.AuthorID == authorId);
            if (author != null && author.UserID == CurrentUserId)
            {
                MessageBox.Show("Зачем вы жалуетесь на самого себя?");
                return;
            }

            var existing = Core.Context.Appeals
                .FirstOrDefault(a => a.UserID == CurrentUserId &&
                                a.ObjectID == authorId &&
                                a.TargetID == 2);

            if (existing != null)
            {
                MessageBox.Show("Вы уже подавали жалобу на этого автора");
                return;
            }

            var statusPending = Core.Context.Statuses
                .FirstOrDefault(s => s.StatusName == "На рассмотрении");

            var appeal = new Appeals
            {
                UserID = CurrentUserId,
                TargetID = 2,
                ObjectID = authorId,
                Text = reason,
                StatusID = statusPending?.StatusID ?? 1
            };

            Core.Context.Appeals.Add(appeal);
            Core.Context.SaveChanges();
            MessageBox.Show("Жалоба на автора отправлена");
        }

        /// <summary>
        /// Получение отзывов о книге
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Отзывы</returns>
        public object GetReviews(int bookId)
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.BookID == bookId && !r.IsFrozen)
                .Select(r => new
                {
                    r.ReviewID,
                    r.Rating,
                    r.ReviewText,
                    UserName = Core.Context.Users
                        .Where(u => u.UserID == r.UserID)
                        .Select(u => u.Name)
                        .FirstOrDefault() ?? "Неизвестный пользователь",
                    r.IsFrozen,
                    ShowFreezeButton = CurrentUserRole == "Админ"
                })
                .ToList();
            return reviews;
        }

        /// <summary>
        /// Добавление отзыва о книге
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <param name="rating">Оценка</param>
        /// <param name="reviewText">Текст отзыва</param>
        public void AddReview(int bookId, int rating, string reviewText)
        {
            if (!CanWriteReview(bookId))
            {
                MessageBox.Show("Вы не можете оставить отзыв на свою же книгу!");
                return;
            }

            if (rating < 1 || rating > 5)
            {
                MessageBox.Show("Оценка должна быть от 1 до 5");
                return;
            }

            if (string.IsNullOrEmpty(reviewText))
            {
                MessageBox.Show("Текст отзыва не может быть пустым");
                return;
            }

            var review = new Reviews
            {
                BookID = bookId,
                UserID = CurrentUserId,
                Rating = rating,
                ReviewText = reviewText,
                IsFrozen = false
            };

            Core.Context.Reviews.Add(review);
            Core.Context.SaveChanges();

            UpdateBookRating(bookId);

            MessageBox.Show("Отзыв добавлен");
        }

        /// <summary>
        /// Вспомогательный метод обновления рейтинга книги 
        /// </summary>
        /// <param name="bookId">ID книги</param>
        private void UpdateBookRating(int bookId)
        {
            var avgRating = Core.Context.Reviews
                .Where(r => r.BookID == bookId && !r.IsFrozen)
                .Average(r => (double?)r.Rating) ?? 0;

            var book = Core.Context.Books.FirstOrDefault(b => b.BookID == bookId);
            if (book != null)
            {
                book.Rating = (decimal)Math.Round(avgRating, 1);
                Core.Context.SaveChanges();
            }
        }

        /// <summary>
        /// Жалоба на отзыв
        /// </summary>
        /// <param name="reviewId">ID отзыва</param>
        /// <param name="reason">Причина жалобы</param>
        public void ComplainReview(int reviewId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Укажите причину жалобы");
                return;
            }

            var existing = Core.Context.Appeals
                .FirstOrDefault(a => a.UserID == CurrentUserId &&
                                a.ObjectID == reviewId &&
                                a.TargetID == 3);

            if (existing != null)
            {
                MessageBox.Show("Вы уже подавали жалобу на этот отзыв");
                return;
            }

            var statusPending = Core.Context.Statuses
             .FirstOrDefault(s => s.StatusName == "На рассмотрении");

            var targetReview = Core.Context.Targets
                .FirstOrDefault(t => t.TargetName == "Отзыв");

            var appeal = new Appeals
            {
                UserID = CurrentUserId,
                TargetID = 3,
                ObjectID = reviewId,
                Text = reason,
                StatusID = statusPending?.StatusID ?? 1,
            };

            Core.Context.Appeals.Add(appeal);
            Core.Context.SaveChanges();
            MessageBox.Show("Жалоба на отзыв отправлена");

        }

        /// <summary>
        /// Заморозка книги
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <param name="reason">Причина заморозки</param>
        public void FreezeBook(int bookId, string reason)
        {
            if (CurrentUserRole != "Админ")
            {
                MessageBox.Show("Доступ запрещен");
                return;
            }

            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Укажите причину заморозки");
                return;
            }

            var book = Core.Context.Books.FirstOrDefault(b => b.BookID == bookId);
            if (book != null)
            {
                if (book.IsFrozen)
                {
                    MessageBox.Show($"Книга '{book.Title}' уже заморожена\nПричина: {book.FreezeReason}");
                    return;
                }

                book.IsFrozen = true;
                book.FreezeReason = reason;
                Core.Context.SaveChanges();
                MessageBox.Show($"Книга '{book.Title}' заморожена");

                LoadBookData();
            }
        }

        /// <summary>
        /// Заморозка отзыва
        /// </summary>
        /// <param name="reviewId">ID отзыва</param>
        /// <param name="reason">Причина заморозки</param>
        public void FreezeReview(int reviewId, string reason)
        {
            if (CurrentUserRole != "Админ")
            {
                MessageBox.Show("Доступ запрещен");
                return;
            }

            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Укажите причину заморозки");
                return;
            }

            var review = Core.Context.Reviews.FirstOrDefault(r => r.ReviewID == reviewId);
            if (review != null)
            {
                if (review.IsFrozen)
                {
                    MessageBox.Show("Этот отзыв уже заморожен");
                    return;
                }

                review.IsFrozen = true;
                review.FreezeReason = reason;
                Core.Context.SaveChanges();

                var book = Core.Context.Books.FirstOrDefault(b => b.BookID == review.BookID);
                if (book != null)
                {
                    UpdateBookRating(book.BookID);
                }

                MessageBox.Show("Отзыв заморожен");
                LoadReviews();
            }
        }

        /// <summary>
        /// Проверка, может ли пользователь писать отзыв 
        /// (нельзя писать замороженному пользователю, а также автору на свою же книгу)
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>True, если пользователь не заморожен и не является автором книги</returns>
        public bool CanWriteReview(int bookId)
        {
            var user = Core.Context.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
            if (user == null || user.IsFrozen)
                return false;

            var book = Core.Context.Books.FirstOrDefault(b => b.BookID == bookId);
            if (book != null)
            {
                var author = Core.Context.Authors.FirstOrDefault(a => a.AuthorID == book.AuthorID);
                if (author != null && author.UserID == CurrentUserId)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Вспомогательный метод определения автора по книге
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Книгу</returns>
        public int GetAuthorIdByBook(int bookId)
        {
            var book = Core.Context.Books
                .Where(b => b.BookID == bookId)
                .Select(b => b.AuthorID)
                .FirstOrDefault();

            return book;
        }

        /// <summary>
        /// Вспомогательный метод загрузки данных о книгах
        /// </summary>
        private void LoadBookData()
        {
            dynamic book = GetBookInfo(CurrentBookId);

            Title.Text = book.Title;
            Author.Text = $"Автор: {book.AuthorName}";
            Rating.Text = $"⭐ {book.Rating:F1}";
            Genres.Text = $"Жанры: {string.Join(", ", book.Genres)}";
            Description.Text = book.Description;

            if (!string.IsNullOrEmpty(book.CoverImage?.ToString()))
            {
                try
                {
                    CoverImage.Source = new BitmapImage(new Uri(book.CoverImage, UriKind.Relative));
                }
                catch
                {
                    CoverImage.Source = null;
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод загрузки отзывов
        /// </summary>
        private void LoadReviews()
        {
            var reviews = GetReviews(CurrentBookId) as System.Collections.IEnumerable;
            ReviewsControl.ItemsSource = reviews;

            btnFreezeBook.Visibility = CurrentUserRole == "Админ"
                ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Мето для прогрузки страницы (без него плохо прогружались данные)
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBookData();
            LoadReviews();

            if (!CanWriteReview(CurrentBookId))
            {
                btnAddReview.IsEnabled = false;
                btnAddReview.ToolTip = "Вы не можете оставить отзыв на свою же книгу!";
            }

            int authorId = GetAuthorIdByBook(CurrentBookId);
            var author = Core.Context.Authors.FirstOrDefault(a => a.AuthorID == authorId);
            if (author != null && author.UserID == CurrentUserId)
            {
                btnComplainBook.IsEnabled = false;
                btnComplainBook.ToolTip = "Зачем вы жалуетесь на свою же книгу?";

                ComplainAuthorBtn.IsEnabled = false;
                ComplainAuthorBtn.ToolTip = "Зачем вы жалуетесь на самого себя?";
            }
        }

        /// <summary>
        /// Обработчик кнопки жалобы на книгу
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void ComplainBook_Click(object sender, RoutedEventArgs e)
        {
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину жалобы:", "Жалоба на книгу");
            ComplainBook(CurrentBookId, reason);
        }

        /// <summary>
        /// Обработчик кнопки жалобы на автора
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void ComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            int authorId = GetAuthorIdByBook(CurrentBookId);
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину жалобы:", "Жалоба на автора");
            ComplainAuthor(authorId, reason);
        }

        /// <summary>
        /// Обработчик кнопки жалобы на отзыв
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void ComplainReview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int reviewId = (int)button.Tag;
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину жалобы:", "Жалоба на отзыв");
            ComplainReview(reviewId, reason);
        }

        /// <summary>
        /// Обработчик кнопки добавления отзыва
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Дополнительная информация</param>
        private void AddReview_Click(object sender, RoutedEventArgs e)
        {
            int rating = int.Parse((RatingCombo.SelectedItem as ComboBoxItem).Content.ToString());
            string reviewText = ReviewComment.Text;
            AddReview(CurrentBookId, rating, reviewText);
            ReviewComment.Text = "";
            LoadReviews();
        }

        /// <summary>
        /// Обрабочтик кнопки заморозки книги
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FreezeBook_Click(object sender, RoutedEventArgs e)
        {
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину заморозки:", "Заморозка книги");
            FreezeBook(CurrentBookId, reason);
            LoadBookData();
        }

        /// <summary>
        /// Обрабочтик кнопки заморозки отзыва
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FreezeReview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int reviewId = (int)button.Tag;
            string reason = Microsoft.VisualBasic.Interaction.InputBox("Укажите причину заморозки:", "Заморозка отзыва");
            FreezeReview(reviewId, reason);
            LoadReviews();
        }
    }
}


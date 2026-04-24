using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp17.Pages.Admin
{
    /// <summary>
    /// Логика взаимодействия для AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        private Users _user;

        public AddUser()
        {
            InitializeComponent();
            _user = null;
            Title = "Добавление пользователя";
            Phone.TextChanged += Phone_TextChanged;
        }

        public AddUser(Users user)
        {
            InitializeComponent();
            _user = user;
            Title = "Редактирование пользователя";
            Phone.TextChanged += Phone_TextChanged;
            LoadUserData();
        }

        private void Phone_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string phone = Phone.Text;
            if (!string.IsNullOrEmpty(phone))
            {
                if (!IsValidPhone(phone))
                {
                    PhoneWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    PhoneWarning.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PhoneWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadUserData()
        {
            if (_user != null)
            {
                FullName.Text = _user.FullName;
                Phone.Text = _user.Phone;
                Login.Text = _user.Login;

                foreach (ComboBoxItem item in Role.Items)
                {
                    if (item.Content.ToString() == _user.Role)
                    {
                        Role.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            foreach (char c in cleanPhone)
            {
                if (!char.IsDigit(c) && c != '+')
                    return false;
            }

            string digitsOnly = cleanPhone.Replace("+", "");
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 12;
        }

        private string NormalizePhone(string phone)
        {
            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
            {
                cleanPhone = "+7" + cleanPhone.Substring(1);
            }
            else if (cleanPhone.StartsWith("7") && cleanPhone.Length == 11)
            {
                cleanPhone = "+" + cleanPhone;
            }
            else if (!cleanPhone.StartsWith("+") && cleanPhone.Length == 10)
            {
                cleanPhone = "+7" + cleanPhone;
            }

            return cleanPhone;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullName.Text))
            {
                MessageBox.Show("Введите ФИО");
                FullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон");
                Phone.Focus();
                return;
            }

            if (!IsValidPhone(Phone.Text))
            {
                MessageBox.Show("Введите корректный номер телефона");
                Phone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин");
                Login.Focus();
                return;
            }

            string normPhone = NormalizePhone(Phone.Text);

            string newRole = ((ComboBoxItem)Role.SelectedItem).Content.ToString();

            if (_user == null)
            {
                bool loginExists = Core.Context.Users.Any(u => u.Login == Login.Text.Trim());
                if (loginExists)
                {
                    MessageBox.Show("Логин уже существует");
                    Login.Focus();
                    return;
                }

                bool phoneExists = Core.Context.Users.Any(u => u.Phone == normPhone);
                if (phoneExists)
                {
                    MessageBox.Show("Телефон уже зарегистрирован");
                    Phone.Focus();
                    return;
                }

                var newUser = new Users
                {
                    FullName = FullName.Text.Trim(),
                    Phone = normPhone,
                    Login = Login.Text.Trim(),
                    Password = Password.Password.Length > 0 ? Password.Password : "123",
                    Role = newRole,
                    IsFrozen = false,
                    RegistrationDate = DateTime.Now
                };

                Core.Context.Users.Add(newUser);
            }
            else
            {
                if (_user.Login != Login.Text.Trim())
                {
                    bool loginExists = Core.Context.Users.Any(u => u.Login == Login.Text.Trim());
                    if (loginExists)
                    {
                        MessageBox.Show("Логин уже существует");
                        Login.Focus();
                        return;
                    }
                }

                if (_user.Phone != normPhone)
                {
                    bool phoneExists = Core.Context.Users.Any(u => u.Phone == normPhone);
                    if (phoneExists)
                    {
                        MessageBox.Show("Телефон уже зарегистрирован");
                        Phone.Focus();
                        return;
                    }
                }

                if (_user.Role == "Admin" && newRole != "Admin" && Core.CurrentUserId != _user.Id)
                {
                    MessageBox.Show("Нельзя изменить роль другого администратора");
                    return;
                }

                _user.FullName = FullName.Text.Trim();
                _user.Phone = normPhone;
                _user.Login = Login.Text.Trim();
                _user.Role = newRole;

                if (!string.IsNullOrEmpty(Password.Password))
                {
                    _user.Password = Password.Password;
                }
            }

            Core.Context.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
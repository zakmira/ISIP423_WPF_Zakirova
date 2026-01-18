using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WpfApp1.Pages
{
    public partial class Page5 : Page
    {
        private CarConfig CurrentConfig;
        private bool _hasUnsavedChanges = false;
        public Page5(CarConfig config)
        {
            InitializeComponent();
            CurrentConfig = config;

            NameBox.Text = "";
            PhoneBox.Text = "";
            MailBox.Text = "";

            if (this.NavigationService != null)
            {
                this.NavigationService.Navigating += NavigationService_Navigating;
            }
                               
            UpdateFormState();
        }

        public bool HasUnsavedChanges => !string.IsNullOrWhiteSpace(NameBox.Text) ||
                                         !string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                                         !string.IsNullOrWhiteSpace(MailBox.Text);

        public bool IsFormValidNow => IsFormValid();

        private bool IsFormValid()
        {
            if (NameBox == null || PhoneBox == null || MailBox == null)
            {
                return false;
            }

            string name = NameBox.Text?.Trim() ?? "";
            string phone = PhoneBox.Text?.Trim() ?? "";
            string email = MailBox.Text?.Trim() ?? "";

            if (name.Length < 2 || !Regex.IsMatch(name, @"^[а-яА-Яa-zA-Z\s]+$"))
                return false;

            string digitsOnly = Regex.Replace(phone, @"[^0-9]", "");
            if (digitsOnly.Length < 10)
                return false;

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
                return false;

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex >= email.Length - 1)
                return false;

            return true;
        }

        private void UpdateFormState()
        {
            _hasUnsavedChanges = !string.IsNullOrWhiteSpace(NameBox.Text) ||
                                 !string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                                 !string.IsNullOrWhiteSpace(MailBox.Text);

            SubmitButton.IsEnabled = IsFormValid();
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateFormState();
        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateFormState();
        private void MailBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateFormState();

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Заявка отправлена!", "Успех", MessageBoxButton.OK);

            // Опционально: очистить форму
            NameBox.Text = "";
            PhoneBox.Text = "";
            MailBox.Text = "";
            UpdateFormState();
        }

        private void NavigationService_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (_hasUnsavedChanges && !IsFormValid())
            {
                var result = MessageBox.Show(
                    "Вы не завершили оформление заявки.\nУйти без отправки?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }




    }
}

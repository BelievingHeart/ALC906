using System.Windows;
using System.Windows.Controls;
using Core.ViewModels.Login;

namespace UI.Views.Login
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }


        private void InputPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = (LoginViewModel) DataContext;
            var passwordBox = (PasswordBox) sender;
            viewModel.InputPassWord = passwordBox.Password;
        }

        private void OldPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = (LoginViewModel) DataContext;
            var passwordBox = (PasswordBox) sender;
            viewModel.InputPassWord = passwordBox.Password;
        }

        private void NewPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = (LoginViewModel) DataContext;
            var passwordBox = (PasswordBox) sender;
            viewModel.NewPassword = passwordBox.Password;
        }

        private void NewPasswordBoxDoubleCheckPasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = (LoginViewModel) DataContext;
            var passwordBox = (PasswordBox) sender;
            viewModel.NewPasswordDoubleCheck = passwordBox.Password;
        }

        private void ClearInputPasswordBox(object sender, RoutedEventArgs e)
        {
            InputPasswordBox.Clear();
        }
        
    }
}
using System;
using System.Windows;
using System.Windows.Input;
using Exercise.Models;
using Exercise.MVVM;
using Exercise.Services;

namespace Exercise.ViewModels
{
    public class LoginViewModel
    {
        private AuthService _authService;

        public string Username { get; set; }
        public string Password { get; set; } 

        public Action CloseAction { get; set; }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            var password = passwordBox != null ? passwordBox.Password : this.Password;

            if (_authService.ValidateUser(Username, password))
            {
                // Cập nhật vào Session
                UserSession.Instance.SetLogin(Username);

                MessageBox.Show($"Xin chào {Username}!", "Thông báo");

                CloseAction?.Invoke();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi");
            }
        }
    }
}
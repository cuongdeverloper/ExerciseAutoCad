using System;

namespace Exercise.Models
{
    public class UserSession
    {
        private static UserSession _instance;
        public static UserSession Instance => _instance ?? (_instance = new UserSession());

        public bool IsLoggedIn { get; private set; }
        public string Username { get; private set; }

        // Sự kiện để báo cho Ribbon biết khi nào cần cập nhật (Bật/Tắt nút)
        public event EventHandler AuthenticationChanged;

        private UserSession() { }

        public void SetLogin(string username)
        {
            IsLoggedIn = true;
            Username = username;
            OnAuthenticationChanged();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            Username = string.Empty;
            OnAuthenticationChanged();
        }

        private void OnAuthenticationChanged()
        {
            AuthenticationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
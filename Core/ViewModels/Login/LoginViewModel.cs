using System;
using System.Windows.Input;
using System.Xml.Serialization;
using Core.IoC.Loggers;
using MaterialDesignThemes.Wpf;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Login
{
    public class LoginViewModel : AutoSerializableBase<LoginViewModel>
    {
        private string _inputPassWord;
        
        
        public string RememberedPassword { get; set; } = "123456";

        [XmlIgnore] public string NewPassword { get; set; }
        [XmlIgnore] public ISnackbarMessageQueue MessageQueue { get; set; }

        [XmlIgnore] public string NewPasswordDoubleCheck { get; set; }

        [XmlIgnore] public ICommand LoginCommand { get; set; }

        [XmlIgnore] public ICommand ChangePasswordCommand { get; set; }

        [XmlIgnore] public string InputPassWord { get; set; }

        [XmlIgnore] public bool Authorized { set; get; }

        [XmlIgnore]
        public ICommand LogoutCommand { get; }


        public LoginViewModel()
        {
            LoginCommand = new SimpleCommand(o=>Login(), o=>!Authorized);
            LogoutCommand = new SimpleCommand(o=>Logout(), o=>Authorized);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
        }

        private void Logout()
        {
            Authorized = false;
            Log("Log out success");
        }

        private void ChangePassword()
        {
            if (InputPassWord != RememberedPassword)
            {
                Log("Incorrect old password");
                return;
            }

            if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(NewPasswordDoubleCheck))
            {
                Log("New password can not be empty");
                return;
            }

            if (NewPassword.Length < 6 || NewPasswordDoubleCheck.Length < 6)
            {
                Log("New password must at least have 6 characters");
                return;
            }

            if (NewPassword != NewPasswordDoubleCheck)
            {
                Log("New password does not match");
                return;
            }

            RememberedPassword = NewPassword;
            Log("Change password success!");
        }

        private void Log(string message)
        {
            MessageQueue?.Enqueue(message);
        }

        private void Login()
        {
            Authorized = false;
            if (string.IsNullOrEmpty(RememberedPassword))
            {
                Log("Password has not registered");
                return;
            }

            if (string.IsNullOrEmpty(InputPassWord))
            {
                Log("Password can not be empty");
                return;
            }

            if (InputPassWord != RememberedPassword)
            {
                Log("Incorrect password");
                return;
            }

            Authorized = true;
            Log("Login success");
        }
    }
}
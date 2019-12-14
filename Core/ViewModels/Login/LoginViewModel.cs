using System;
using System.Windows.Input;
using System.Xml.Serialization;
using Core.IoC.Loggers;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Login
{
    public class LoginViewModel : AutoSerializableBase<LoginViewModel>
    {
        private string _inputPassWord;
        public string RememberedPassword { get; set; } = "123456";

        [XmlIgnore] public string NewPassword { get; set; }

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
            Logger.LogStateChanged("Log out success");
        }

        private void ChangePassword()
        {
            if (InputPassWord != RememberedPassword)
            {
                Logger.LogStateChanged("Incorrect old password");
                return;
            }

            if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(NewPasswordDoubleCheck))
            {
                Logger.LogStateChanged("New password can not be empty");
                return;
            }

            if (NewPassword.Length < 6 || NewPasswordDoubleCheck.Length < 6)
            {
                Logger.LogStateChanged("New password must at least have 6 characters");
                return;
            }

            if (NewPassword != NewPasswordDoubleCheck)
            {
                Logger.LogStateChanged("New password does not match");
                return;
            }

            RememberedPassword = NewPassword;
            Logger.LogStateChanged("Change password success!");
        }

        private void Login()
        {
            Authorized = false;
            if (string.IsNullOrEmpty(RememberedPassword))
            {
                Logger.LogStateChanged("Password has not registered");
                return;
            }

            if (string.IsNullOrEmpty(InputPassWord))
            {
                Logger.LogStateChanged("Password can not be empty");
                return;
            }

            if (InputPassWord != RememberedPassword)
            {
                Logger.LogStateChanged("Incorrect password");
                return;
            }

            Authorized = true;
            Logger.LogStateChanged("Login success");
        }
    }
}
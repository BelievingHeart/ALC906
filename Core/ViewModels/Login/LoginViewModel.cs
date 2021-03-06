﻿using System;
using System.Windows.Input;
using System.Windows.Threading;
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
        private bool _authorized;


        public string RememberedPassword { get; set; } = "123456";

        [XmlIgnore] public string NewPassword { get; set; }
        [XmlIgnore] public ISnackbarMessageQueue MessageQueue { get; set; }

        [XmlIgnore] public string NewPasswordDoubleCheck { get; set; }

        [XmlIgnore] public ICommand LoginCommand { get; set; }

        [XmlIgnore] public ICommand ChangePasswordCommand { get; set; }

        [XmlIgnore] public string InputPassWord { get; set; }

        [XmlIgnore]
        public bool Authorized
        {
            set
            {
                _authorized = value;
                if (_authorized)
                {
                    // Start time out for auto log out
                    if (_logoutTimer != null) _logoutTimer.Tick -= Logout;
                    _logoutTimer = new DispatcherTimer(DispatcherPriority.Background)
                    {
                        Interval = TimeSpan.FromMinutes(10)
                    };
                    _logoutTimer.Tick += Logout;
                    _logoutTimer.Start();
                }
                else
                {
                    Log("已登出");
                }
            }
            get { return _authorized; }
        }

        [XmlIgnore]
        public ICommand LogoutCommand { get; }

        private DispatcherTimer _logoutTimer;


        public LoginViewModel()
        {
            LoginCommand = new SimpleCommand(o=>Login(), o=>!Authorized);
            LogoutCommand = new SimpleCommand(o=>Logout(null, null), o=>Authorized);
            ChangePasswordCommand = new RelayCommand(ChangePassword);
        }

        private void Logout(object sender, EventArgs eventArgs)
        {
            Authorized = false;
        }

        private void ChangePassword()
        {
            if (InputPassWord != RememberedPassword)
            {
                Log("密码错误");
                return;
            }

            if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(NewPasswordDoubleCheck))
            {
                Log("密码不能为空");
                return;
            }

            if (NewPassword.Length < 6 || NewPasswordDoubleCheck.Length < 6)
            {
                Log("新密码必须含有6位以上数字或字母");
                return;
            }

            if (NewPassword != NewPasswordDoubleCheck)
            {
                Log("新密码两次输入不对应");
                return;
            }

            RememberedPassword = NewPassword;
            Log("修改密码成功!");
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
                Log("密码还没设置");
                return;
            }

            if (string.IsNullOrEmpty(InputPassWord))
            {
                Log("密码不能为空");
                return;
            }

            if (InputPassWord != RememberedPassword)
            {
                Log("密码错误");
                return;
            }

            Authorized = true;
            Log("登陆成功");
        }
    }
}
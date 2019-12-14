using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Instrumentation;
using System.Windows.Controls;
using Core.Enums;
using UI.Views.Database;
using UI.Views.Home;
using UI.Views.LineScan;
using UI.Views.Login;
using UI.Views.Server;
using UI.Views.Settings;
using UI.Views.Vision2D;

namespace UI.Converters
{
    public class EnumToApplicationPageConverter : ValueConverterBase<EnumToApplicationPageConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var pageEnum = (ApplicationPageType) value;
                return RetrievePage(pageEnum);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static UserControl RetrievePage(ApplicationPageType pageEnum)
        {
            UserControl output;
            // Try get the first application page of specific type 
            try
            {
                output = pageEnum == ApplicationPageType.CameraHostPage
                    ? ApplicationPages.First(ele => ele is CameraHostView)
                    : pageEnum == ApplicationPageType.Home
                        ? ApplicationPages.First(ele => ele is HomeView)
                        : pageEnum == ApplicationPageType.LineScanHostPage
                            ? ApplicationPages.First(ele => ele is LineScanHostView)
                            : pageEnum == ApplicationPageType.ServerPage
                                ? ApplicationPages.First(ele => ele is ServerView)
                                : pageEnum == ApplicationPageType.SettingsPage
                                    ? ApplicationPages.First(ele => ele is SettingsView)
                                    : pageEnum== ApplicationPageType.DatabaseHostPage? 
                                     ApplicationPages.First(ele => ele is DatabaseHostView) : ApplicationPages.First(ele => ele is LoginView);
            }
            // If the list not contain an instance of such type
            // Add one and return it
            catch (InvalidOperationException e)
            {
                output = pageEnum == ApplicationPageType.CameraHostPage ? new CameraHostView() :
                    pageEnum == ApplicationPageType.Home ? new HomeView() :
                    pageEnum == ApplicationPageType.LineScanHostPage ? new LineScanHostView() :
                    pageEnum == ApplicationPageType.ServerPage? new ServerView():
                    pageEnum == ApplicationPageType.SettingsPage ? new SettingsView() :
                    pageEnum == ApplicationPageType.DatabaseHostPage ? new DatabaseHostView() :
                   new LoginView() as UserControl;


                ApplicationPages.Add(output);
            }


            return output;
        }

        private static List<UserControl> ApplicationPages { get; } = new List<UserControl>();
    }
}
using AsystBussiness.Controls;
using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages;
using AsystBussiness.Pages.UserAccount;
using AsystBussiness.Pages.Address;
using ColonyConcierge.APIData.Data.Logistics.NotificationData;
using PCLAppConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Xamarin.Forms;

namespace AsystBussiness
{
    public partial class App : Application
    {
        public event EventHandler Started;

        protected App()
        {
            Assembly assembly = typeof(App).GetTypeInfo().Assembly;
            Stream configStream = assembly.GetManifestResourceStream("AsystBussiness.App.config");
            try
            {
                using (configStream)
                {
                    ConfigurationManager.Initialise(configStream);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            try
            {
                InitializeComponent();

                this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(this.MainPage))
                    {
                        var appServices = DependencyService.Get<IAppServices>();
                        appServices.SetNetworkBar(false);
                    }
                };
            }
            catch(Exception e)
            {
                ExceptionHandler.Catch(e);
            }          
        }

        public App(int destinationId) : this()
        {
            Shared.LocalAddress = null;
        }

        public App(LogisticsNotification logisticsNotification = null) : this()
        {
            if (Shared.IsLoggedIn)
            {
                Shared.LocalAddress = null;
                Application.Current.MainPage = new HomePage();
            }
            else
            {
                Shared.LocalAddress = null;
                var mainPage = new SigninPage();
                Application.Current.MainPage = mainPage;
            }

            MainPage.SetValue(NavigationPage.BarTextColorProperty, Color.White);
            MainPage.SetValue(NavigationPage.BackButtonTitleProperty, AppResources.Back);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            if (Started != null)
            {
                Started(this, EventArgs.Empty);
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

    public class CustomLogger : FFImageLoading.Helpers.IMiniLogger
    {
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Error(string errorMessage)
        {
            System.Diagnostics.Debug.WriteLine(errorMessage);
        }

        public void Error(string errorMessage, Exception ex)
        {
            Error(errorMessage + System.Environment.NewLine + ex.ToString());
        }
    }
}

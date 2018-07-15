using AsystBussiness.Core;
using AsystBussiness.Pages.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Pages.Login
{
    class LoginPageModel : NotifyPropertyChanged
    {
        public void OnAppearing()
        {
            if (Application.Current.Properties.ContainsKey("Username"))
            {
                Username = (string)Application.Current.Properties["Username"];
                Password = (string)Application.Current.Properties["Password"];

                Login.Execute(null);
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetField(ref _password, value); }
        }

        public Command Login
        {
            get
            {
                return new Command(async () =>
                {
                    var loginResult = Shared.APIs.ILogins.GetLoginToken(Username, Password);
                    //var loginResult = Shared.APIs.ILogins.GetLoginTokenForRole(Username, "business_app_user", Password);
                    if (loginResult != null)
                    {
                        if (loginResult.Token != null && loginResult.OK)
                        {
                            Application.Current.Properties["Username"] = Username;
                            Application.Current.Properties["Password"] = Password;
                            await Application.Current.SavePropertiesAsync();

                            Shared.LoginToken = loginResult.Token;

                            var userModel = Shared.APIs.IUsers.GetCurrentUser();
                            Shared.UserId = userModel.ID;

                            NavigationHandler.SetDetailPage(new DashboardPage());
                        }
                    };
                });
            }
        }
    }
}

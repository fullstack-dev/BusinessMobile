using AsystBussiness.Controls;
using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages.Address;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.UserAccount
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SigninPage : ContentPageBase
    {
        IAppServices mAppServices;

        public Action<User> Done
        {
            get;
            set;
        }

        public Action Back
        {
            get;
            set;
        }

        public User UserModel
        {
            get;
            set;
        }

        public ExtendedAddress ServiceAddress
        {
            get;
            set;
        }

        public bool IsFromSignIn
        {
            get;
            set;
        }



        public SigninPage()
        {
            try
            {
                InitializeComponent();

                mAppServices = DependencyService.Get<IAppServices>();

                LabelForgotYourPassword.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        Device.OpenUri(new Uri(PCLAppConfig.ConfigurationManager.AppSettings["ForgotPasswordUrl"]));
                    })
                });
            }
            catch(Exception e)
            {
                ExceptionHandler.Catch(e);
            }
            
        }

        public void ShowBack(CustomNavigationPage customNavigationPage)
        {
            try
            {
                this.Navigation.InsertPageBefore(new Page(), this);
                customNavigationPage.PopView = (page) =>
                {
                    if (page == this)
                    {
                        Back.Invoke();
                        return true;
                    }
                    return false;
                };
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }            
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                var isSuccessed = false;
                this.IsBusy = true;
                (sender as VisualElement).IsEnabled = false;

                Task.Run(() =>
                {
                    try
                    {
                        var loginResult = Shared.APIs.ILogins.GetLoginToken(emailEntry.Text, passwordEntry.Text);
                        if (loginResult != null)
                        {
                            if (loginResult.Token != null && loginResult.OK)
                            {
                                Shared.LoginToken = loginResult.Token;
                                Shared.LocalAddress = ServiceAddress;

                                var userModel = Shared.APIs.IUsers.GetCurrentUser();
                                UserModel = userModel;
                                Shared.UserId = userModel.ID;

                                try
                                {
                                    string notificationId = mAppServices.GetRegistrationNotificationId();
                                    string uniqueDeviceId = mAppServices.UniqueDeviceId;

                                    if (!string.IsNullOrEmpty(notificationId))
                                    {
                                        var platform = "android";
                                        if (Device.RuntimePlatform == Device.iOS)
                                        {
                                            platform = "ios";
                                        }
                                        bool isDeviceTokenSet = Shared.APIs.IUsers.AddDeviceToken(userModel.ID, platform, notificationId, uniqueDeviceId);
                                        if (!isDeviceTokenSet)
                                        {
                                            throw new Exception(AppResources.RegisterNotificationFail);
                                        }
                                        Shared.NotificationToken = notificationId;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        (sender as VisualElement).IsEnabled = true;
                                        Utils.ShowErrorMessage(new CustomException(ex.Message, ex));
                                    });
                                }

                                isSuccessed = true;
                                System.Diagnostics.Debug.WriteLine("UserID : " + UserModel.ID + " UserName : " + UserModel.Username);
                            }
                            else
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    (sender as VisualElement).IsEnabled = true;
                                    Utils.ShowErrorMessage(loginResult.Message);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            (sender as VisualElement).IsEnabled = true;
                            Utils.ShowErrorMessage(ex);
                        });
                    }
                }).ContinueWith(t =>
                {
                    if (isSuccessed)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            this.IsBusy = false;
                            if (this.Done != null)
                            {
                                this.Done(UserModel);
                            }
                            else
                            {
                                Application.Current.MainPage = new HomePage();
                            }
                        });
                    }
                    else
                    {
                        (sender as VisualElement).IsEnabled = true;
                        this.IsBusy = false;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception exeption)
            {
                ExceptionHandler.Catch(exeption);
            }
        }
    }
}
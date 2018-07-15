using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages;
using AsystBussiness.ViewModels;
using AsystBussiness.Pages.UserAccount;
using AsystBussiness.Pages.Dashboard;
using AsystBussiness.Pages.Orders;
using AsystBussiness.Pages.ScheduleDelivery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeMasterPage : ContentPageBase
    {
        bool IsLoggedIn = true;
        ColonyConcierge.APIData.Data.User userModel = null;
        IAppServices mAppServices;

        public HomeMasterPage()
        {
            try
            {
                InitializeComponent();
                IsLoggedIn = Shared.IsLoggedIn;
                mAppServices = DependencyService.Get<IAppServices>();

                LabelAppVersion.Text = "v" + mAppServices.AppVersion + " " + AppResources.AppVersion;

                this.SizeChanged += (sender, e) =>
                {
                    try
                    {
                        ImageLogo.WidthRequest = this.Width * 0.6;
                        if (this.Width >= 350)
                        {
                            LabelAppVersion.FontSize = Device.GetNamedSize(NamedSize.Small, LabelAppVersion);
                        }
                        else if (this.Width >= 290)
                        {
                            LabelAppVersion.FontSize = Device.GetNamedSize(NamedSize.Small, LabelAppVersion) * 0.9;
                        }
                        else if (this.Width >= 250)
                        {
                            LabelAppVersion.FontSize = Device.GetNamedSize(NamedSize.Small, LabelAppVersion) * 0.8;
                        }
                        else
                        {
                            LabelAppVersion.FontSize = Device.GetNamedSize(NamedSize.Small, LabelAppVersion) * 0.7;
                        }
                    }
                    catch (Exception e1)
                    {
                        ExceptionHandler.Catch(e1);
                    }
                };

                LoadMenu();
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                ReloadData();
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public void ReloadData()
        {
            if (IsLoggedIn != Shared.IsLoggedIn)
            {
                LoadData();
            }
        }

        public void LoadData()
        {
            IsLoggedIn = Shared.IsLoggedIn;

            if (!this.IsBusy)
            {
                LoadMenu();
                LoadUser();
            }
        }

        public void LoadUser()
        {
            if (Shared.IsLoggedIn)
            {
                this.IsBusy = true;
                Task.Run(() =>
                {
                    try
                    {
                        userModel = Shared.APIs.IUsers.GetCurrentUser();
                    }
                    catch (Exception)
                    {
                        userModel = null;
                    }
                }).ContinueWith(t =>
                {
                    this.IsBusy = false;
                    if (userModel != null)
                    {
                        lbName.Text = userModel.FirstName + " " + userModel.LastName;
                    }
                    else
                    {
                        lbName.Text = string.Empty;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public void LoadMenu()
        {
            try
            {
                IsLoggedIn = Shared.IsLoggedIn;

                this.Pages.Clear();

                this.Pages.Add(new MasterPageItemViewModel
                {
                    Title = "Dashboard",
                    IconSource = "home_white.png",
                    TargetType = typeof(DashboardPage)
                });

                this.Pages.Add(new MasterPageItemViewModel
                {
                    Title = "Schedule Delivery",
                    IconSource = "calendar_white.png",
                    TargetType = typeof(ZipPage)
                });

                /*this.Pages.Add(new MasterPageItemViewModel
                {
                    Title = "Orders",
                    IconSource = "icon_cart.png",
                    TargetType = typeof(OrdersPage)
                });*/

                /*this.Pages.Add(new MasterPageItemViewModel
                {
                    Title = "Reports",
                    IconSource = "support.png",
                    TargetType = typeof(ReportsPage)
                });*/

                this.Pages.Add(new MasterPageItemViewModel
                {
                    Title = "Profile",
                    IconSource = "my_profile.png",
                    TargetType = typeof(ProfilePage)
                });
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        private ObservableCollection<MasterPageItemViewModel> pages = new ObservableCollection<MasterPageItemViewModel>();
        public ObservableCollection<MasterPageItemViewModel> Pages
        {
            get { return pages; }
            set
            {
                pages = value;

                this.OnPropertyChanged(nameof(Pages));
            }
        }
        public ListView ListView { get { return listView; } }
    }
}
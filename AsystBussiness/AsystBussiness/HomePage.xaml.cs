using AsystBussiness.Controls;
using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages.Orders;
using AsystBussiness.Pages.ScheduleDelivery;
using AsystBussiness.Pages.Dashboard;
using AsystBussiness.Pages.UserAccount;
using AsystBussiness.ViewModels;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : MasterDetailPage
    {
        public static HomePage Instance { get; private set; }

        private bool IsChanging = false;
        IAppServices mAppServices;

        public HomePage(Page page = null)
        {
            try
            {
                Instance = this;
                InitializeComponent();

                NavigationHandler.SetMasterPage(this);
                var servicesTabPage = new DashboardPage();
                this.Detail = new NavigationPage(servicesTabPage)
                {
                    BarTextColor = Color.White
                };
                mAppServices = DependencyService.Get<IAppServices>();

                this.PropertyChanged += (sender, e) =>
                {
                    try
                    {
                        if (e.PropertyName == nameof(this.Detail))
                        {
                            mAppServices.SetNetworkBar(false);
                        }
                    }
                    catch (Exception e1)
                    {
                        ExceptionHandler.Catch(e1);
                    }  
                };

                if (Shared.IsLoggedIn
                        && Shared.UserId == -1)
                {
                    var userModel = Shared.APIs.IUsers.GetCurrentUser();
                    Shared.UserId = userModel.ID;
                }

                masterPage.ListView.ItemTapped += ListView_ItemTapped;

                this.IsPresentedChanged += (sender, e) =>
                {
                    try
                    {
                        if (IsPresented)
                        {
                            var itemsSource = masterPage.ListView.ItemsSource;
                            masterPage.ListView.ItemsSource = null;
                            masterPage.ListView.ItemsSource = itemsSource;
                        }
                    }
                    catch(Exception e1)
                    {
                        ExceptionHandler.Catch(e1);
                    }
                };
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public void NavigateTo(Type targetType)
        {
            if (targetType != null)
            {
                mAppServices = DependencyService.Get<IAppServices>();
                var page = (Page)Activator.CreateInstance(targetType);
                NavigationHandler.SetDetailPage(page);

                Task.Run(() =>
                {
                    new System.Threading.ManualResetEvent(false).WaitOne(200);
                }).ContinueWith(t =>
                {
                    IsPresented = false;
                    var itemsSource = masterPage.ListView.ItemsSource;
                    masterPage.ListView.ItemsSource = null;
                    masterPage.ListView.ItemsSource = itemsSource;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = e.Item as MasterPageItemViewModel;
                if (item != null && !IsChanging)
                {
                    masterPage.ListView.SelectedItem = null;
                    foreach (var masterPageItemViewModel in masterPage.ListView.ItemsSource.Cast<MasterPageItemViewModel>().ToList())
                    {
                        //masterPageItemViewModel.IsSelected = masterPageItemViewModel == item;
                    }
                    if (item.TargetType != null)
                    {
                        mAppServices = DependencyService.Get<IAppServices>();
                        var page = (Page)Activator.CreateInstance(item.TargetType);
                        NavigationHandler.SetDetailPage(page);

                        Task.Run(() =>
                        {
                            new System.Threading.ManualResetEvent(false).WaitOne(200);
                        }).ContinueWith(t =>
                        {
                            IsPresented = false;
                            var itemsSource = masterPage.ListView.ItemsSource;
                            masterPage.ListView.ItemsSource = null;
                            masterPage.ListView.ItemsSource = itemsSource;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
            catch (Exception e1)
            {
                ExceptionHandler.Catch(e1);
            }
        }
    }
}
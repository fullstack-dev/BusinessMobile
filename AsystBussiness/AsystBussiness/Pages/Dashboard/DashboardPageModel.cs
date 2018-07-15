using AsystBussiness.Adapters;
using AsystBussiness.Core;
using AsystBussiness.Pages.ScheduleDelivery;
using ColonyConcierge.APIData.Data;
using ColonyConcierge.Client.PlatformServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Pages.Dashboard
{
    class DashboardPageModel : NotifyPropertyChanged
    {
        ObservableCollection<OrderAdapterModel> _pendingOrders = new ObservableCollection<OrderAdapterModel>();
        public ObservableCollection<OrderAdapterModel> PendingOrders
        {
            get { return _pendingOrders; }
            set { SetField(ref _pendingOrders, value); }
        }

        DateTime _displayedDay = DateTime.UtcNow;
        string _displayedDayFormatted = "Today";
        public string DisplayedDayFormatted
        {
            get { return _displayedDayFormatted; }
            set { SetField(ref _displayedDayFormatted, value); }
        }

        bool _isShowingEmptyResults = false;
        public bool IsShowingEmptyResults
        {
            get { return _isShowingEmptyResults; }
            set { SetField(ref _isShowingEmptyResults, value); }
        }

        bool _isShowingResults = false;
        public bool IsShowingResults
        {
            get { return _isShowingResults; }
            set { SetField(ref _isShowingResults, value); }
        }

        bool _canGoBack = false;
        public bool CanGoBack
        {
            get { return _canGoBack; }
            set { SetField(ref _canGoBack, value); }
        }

        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetField(ref _isBusy, value); }
        }

        public Command PreviousDay
        {
            get
            {
                return new Command(() =>
                {
                    SetDay(_displayedDay.AddDays(-1));
                });
            }
        }

        public Command NextDay
        {
            get
            {
                return new Command(() =>
                {
                    SetDay(_displayedDay.AddDays(1));
                });
            }
        }

        public Command NavigateToZipCode
        {
            get
            {
                return new Command(() =>
                {
                    HomePage.Instance.NavigateTo(typeof(ZipPage));
                });
            }
        }

        public Command ViewToday
        {
            get
            {
                return new Command(() =>
                {
                    SetDay(DateTime.UtcNow);
                });
            }
        }

        public void SetDay(DateTime dateTime)
        {
            CanGoBack = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day) > DateTime.Today;

            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;

                if (string.IsNullOrEmpty(Shared.RestaurantLocation))
                {
                    var restaurants = await Shared.APIs.IRestaurantBusiness.GetLocationIds_Async();
                    if (restaurants.Count > 0)
                    {
                        Shared.RestaurantLocation = restaurants.First().ToString();
                    }
                }

                PendingOrders.Clear();
                var start = dateTime.AddHours(-dateTime.Hour).AddMinutes(-dateTime.Minute).AddSeconds(-dateTime.Second);
                var end = start.AddHours(23).AddMinutes(59).AddSeconds(59);

                _displayedDay = new DateTime(dateTime.Ticks);

                if (start.Year == DateTime.Today.Year && start.Month == DateTime.Today.Month && start.Day == DateTime.Today.Day)
                    DisplayedDayFormatted = "Today";
                else
                    DisplayedDayFormatted = _displayedDay.ToString("MMMM dd, yyyy");

                TimeStamp startStamp = start;
                TimeStamp endStamp = end;

                IsShowingEmptyResults = false;
                IsShowingResults = false;

                var deliveryResponse = await Shared.APIs.IRestaurantBusiness.GetOrders_Async(Shared.RestaurantLocation, true, false, true, true, startStamp, endStamp);
                var pickupResponse = await Shared.APIs.IRestaurantBusiness.GetOrders_Async(Shared.RestaurantLocation, false, true, true, true, startStamp, endStamp);

                PendingOrders.Clear();

                if (deliveryResponse.Count > 0 || deliveryResponse.Count > 0)
                {
                    deliveryResponse.AddRange(pickupResponse);
                    var groups = deliveryResponse.GroupBy(x => x.WrappedScheduledService.ServiceStartTime.Time);
                    foreach (var group in groups)
                    {

                        var time = group.First().WrappedScheduledService.ServiceStartTime.Time;

                        var delivery = group.Count(x => (x.WrappedScheduledService as ScheduledRestaurantService).Delivery == true);
                        var pickups = group.Count(x => (x.WrappedScheduledService as ScheduledRestaurantService).Delivery == false);

                        PendingOrders.Add(new OrderAdapterModel(DateTime.Parse(time).ToString("hh:mm tt"), delivery, pickups));
                    }

                    IsShowingEmptyResults = false;
                    IsShowingResults = true;

                }
                else
                {
                    IsShowingEmptyResults = true;
                    IsShowingResults = false;
                }

                IsBusy = false;
            });
            
        }

        public void OnAppearing()
        {
            SetDay(DateTime.UtcNow);
        }
    }
}

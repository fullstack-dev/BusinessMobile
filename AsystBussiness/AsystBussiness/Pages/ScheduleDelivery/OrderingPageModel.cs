using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Pages.Checkout;
using AsystBussiness.Pages.Restaurant;
using ColonyConcierge.APIData.Data;
using ColonyConcierge.Client.PlatformServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Pages.ScheduleDelivery
{
    class OrderingPageModel : NotifyPropertyChanged
    {
        public string ZipCode { get; private set; }
        public DateTime DeliveryDate { get; private set; }
        public string DeliveryTime { get; private set; }
        public string DeliveryAddress { get; private set; }

        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetField(ref _isBusy, value); }
        }

        AddressFacade _addressFacade = new AddressFacade();

        private static OrderingPageModel _singleton;
        public static OrderingPageModel GetSingleton(INavigation navigation)
        {
            if (_singleton == null)
                _singleton = new OrderingPageModel();

            _singleton.OnAppearing(navigation);

            return _singleton;
        }

        INavigation _navigation;
        private void OnAppearing(INavigation navigation)
        {
            _navigation = navigation;
        }

        private OrderingPageModel()
        {

        }

        public void InitializeAsync(string deliveryAddress, string zipCode, DateTime deliveryDate, string deliveryTime)
        {
            this.DeliveryAddress = deliveryAddress;
            this.ZipCode = zipCode;
            this.DeliveryDate = deliveryDate;
            this.DeliveryTime = deliveryTime;
        }

        public Command NavigateToPreviousStep
        {
            get
            {
                return new Command(() =>
                {
                    //Utils.PushAsync(_navigation, new DeliveryPage(ZipCode));
                });
            }
        }

        public Command NavigateToZipCode
        {
            get
            {
                return new Command(() =>
                {
                    //Utils.PushAsync(_navigation, new ZipPage());
                });
            }
        }

        public Command NavigateToScheduleDelivery
        {
            get
            {
                return new Command(async () =>
                {
                    if (IsBusy)
                        return;

                    IsBusy = true;

                    try
                    {
                        var serviceAddress = new ExtendedAddress();
                        var details = await _addressFacade.GetGmsDetails(DeliveryAddress);
                        _addressFacade.FillExtendedAddress(serviceAddress, details);

                        RestaurantFacade restaurantFacade = new RestaurantFacade();
                        var restaurant = restaurantFacade.GetRestaurant();
                        restaurant.IsSelected = true;
                        RestaurantDetailPage restaurantDetailPage = new RestaurantDetailPage(restaurant, DeliveryDate, DeliveryTime);

                        var restaurantDetails = await Shared.APIs.IRestaurant.GetRestaurant_Async(restaurant.RestaurantID);
                        restaurantDetailPage.Title = restaurantDetails.DisplayName;
                        await restaurantDetailPage.LoadData();
                        await Utils.PushAsync(_navigation, new PlaceOrderPage(null, restaurantDetailPage, serviceAddress));
                        restaurant.IsSelected = false;

                        
                    }
                    catch (Exception e)
                    {
                        Crashlytics.Log(e);
                    }

                    IsBusy = false;
                });
            }
        }

        public Command NavigateToMenu
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        if (IsBusy)
                            return;

                        IsBusy = true;

                        var serviceAddress = new ExtendedAddress();
                        var details = await _addressFacade.GetGmsDetails(DeliveryAddress);
                        _addressFacade.FillExtendedAddress(serviceAddress, details);

                        RestaurantFacade restaurantFacade = new RestaurantFacade();
                        var restaurant = restaurantFacade.GetRestaurant();
                        restaurant.IsSelected = true;
                        RestaurantDetailPage restaurantDetailPage = new RestaurantDetailPage(restaurant, DeliveryDate, DeliveryTime);

                        var restaurantDetails = await Shared.APIs.IRestaurant.GetRestaurant_Async(restaurant.RestaurantID);
                        restaurantDetailPage.Title = restaurantDetails.DisplayName;
                        await Utils.PushAsync(_navigation, restaurantDetailPage, true);
                        restaurant.IsSelected = false;
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Catch(e);
                    }

                    IsBusy = false;
                });
            }
        }
    }
}

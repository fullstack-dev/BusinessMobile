using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages.Restaurant;
using AsystBussiness.ViewModels;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.Checkout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyCartPage : ContentPageBase
    {
        public RestaurantDetailPage RestaurantDetailPage
        {
            get;
            set;
        }

        List<MenuGroupModifierItem> listMenuGroupModifierItemSelected;

        public MyCartPage(RestaurantDetailPage restaurantDetailPage)
        {
            RestaurantDetailPage = restaurantDetailPage;

            InitializeComponent();

            NavigationPage.SetBackButtonTitle(this, AppResources.Back);

            var listMenuItemViews = RestaurantDetailPage.ListMenuItemViews.Values.SelectMany(t => t);
            listMenuGroupModifierItemSelected = listMenuItemViews
                                                    .SelectMany(t => t.ListMenuGroupModifierItemSelected)
                                                    .ToList();

            if (listMenuGroupModifierItemSelected.Count == 0)
            {
                ButtonCheckout.IsEnabled = false;
            }

            var cartItemsSource = new ObservableCollection<CartMenuGroupModifierItemViewModel>();
            var deleteAction = new Action<CartMenuGroupModifierItemViewModel>((t) =>
            {
                cartItemsSource.Remove(t);
                listMenuGroupModifierItemSelected.Remove(t.Model);
                foreach (var listMenuItemView in listMenuItemViews)
                {
                    listMenuItemView.ListMenuGroupModifierItemSelected.Remove(t.Model);
                    listMenuItemView.IsSelected = listMenuItemView.ListMenuGroupModifierItemSelected.Count > 0;
                }
                UpdatePrice();

                if (listMenuGroupModifierItemSelected.Count == 0)
                {
                    ButtonCheckout.IsEnabled = false;
                }
            });
            var cartItems = listMenuGroupModifierItemSelected.Select(t => new CartMenuGroupModifierItemViewModel(t, deleteAction)).ToList();
            foreach (var cartItem in cartItems)
            {
                cartItemsSource.Add(cartItem);
            }
            ListViewCarts.ItemsSource = cartItemsSource;
            UpdatePrice();

            ListViewCarts.ItemTapped += (sender, e) =>
            {
                CartMenuGroupModifierItemViewModel cartMenuGroupModifierItem = e.Item as CartMenuGroupModifierItemViewModel;
                ListViewCarts.SelectedItem = null;
                RestaurantMenuItemModifierPage restaurantMenuItemModifierPage = new RestaurantMenuItemModifierPage(RestaurantDetailPage.RestaurantVM, cartMenuGroupModifierItem.Model.MenuItemView, cartMenuGroupModifierItem.Model);
                restaurantMenuItemModifierPage.Saved += (obj) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        obj.RaiseAllOnChanged();
                        UpdatePrice();
                    });
                };
                Utils.PushAsync(Navigation, restaurantMenuItemModifierPage, true);
            };

            ButtonCheckout.Clicked += async (sender, e) =>
            {
                UserFacade userFacade = new UserFacade();
                try
                {
                    var isSucceed = false;
                    this.IsBusy = true;
                    if (restaurantDetailPage.GroupedDeliveryDestination != null)
                    {
                        Shared.LocalAddress = restaurantDetailPage.GroupedDeliveryDestination.Address;
                    }

                    var serviceAddress = new AddressFacade().GetUserAddress();
                    var myCartPage = new PlaceOrderPage(String.Empty, RestaurantDetailPage, serviceAddress);
                    await Utils.PushAsync(Navigation, myCartPage, true);

                    this.IsBusy = false;
                }
                catch (Exception ex)
                {
                    this.IsBusy = false;
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var message = string.IsNullOrEmpty(ex.Message) ? AppResources.SomethingWentWrong : ex.Message;
                        await Utils.ShowErrorMessage(message, 5);
                    });
                }
            };
        }

        public void UpdatePrice()
        {
            decimal totalPrice = 0;
            foreach (var menuGroupModifierItemSelected in listMenuGroupModifierItemSelected)
            {
                menuGroupModifierItemSelected.UpdatePrice();
                totalPrice += menuGroupModifierItemSelected.Price;
            }
            ListViewCarts.Footer = ("$" + totalPrice.ToString("0.00"));
        }

        bool timeSlotTrigger = false;
        bool timeSlotTriggerLocked = false;
        bool IsAppearing = false;

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            IsAppearing = false;
            timeSlotTrigger = false;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsAppearing = true;

            if (!timeSlotTrigger)
            {
                timeSlotTrigger = true;
                Device.StartTimer(new TimeSpan(0, 0, 0, 2), () =>
                {
                    if (!timeSlotTriggerLocked && timeSlotTrigger)
                    {
                        try
                        {
                            timeSlotTriggerLocked = true;
                            if (!OrderClosed)
                            {
                                SearchAvailableOrderTimes();
                            }
                            timeSlotTriggerLocked = false;
                        }
                        catch (Exception)
                        {
                            timeSlotTriggerLocked = false;
                            return timeSlotTrigger;
                        }
                    }
                    return timeSlotTrigger;
                });
            }
        }

        private bool OrderClosed = false;
        public void CloseOrder()
        {
            OrderClosed = true;
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (this.Navigation.NavigationStack.LastOrDefault() == this && IsAppearing)
                    {
                        await DisplayAlert(AppResources.MenuIsClosed, AppResources.OrderExpired, AppResources.OK);
                        var pages = Navigation.NavigationStack.Reverse().Skip(1).ToList();
                        foreach (var page in pages)
                        {
                            if (page != RestaurantDetailPage)
                            {
                                Navigation.RemovePage(page);
                            }
                            else
                            {
                                Navigation.RemovePage(page);
                                break;
                            }
                        }
                        await Navigation.PopAsync(true).ConfigureAwait(false);
                    }
                }
                catch (Exception) { }
            });
        }

        public void SearchAvailableOrderTimes()
        {
            var currentDate = RestaurantDetailPage.GetDateTimeRestaurantFromUtc(DateTime.UtcNow);
            var selectedDate = RestaurantDetailPage.SelectedDate;
            if (currentDate.Date > RestaurantDetailPage.SelectedDate)
            {
                CloseOrder();
                return;
            }
            else
            {
                RMenuOrderingAvailableSlot menuOrderingAvailableSlot = RestaurantDetailPage.MenuOrderingAvailables
                        .Where(s => RestaurantDetailPage.GetDateTimeRestaurantFromLocal(s.CutoffTime.Time).Date == selectedDate.Date)
                        .Where(s => RestaurantDetailPage.GetDateTimeRestaurantFromLocal(s.CutoffTime.Time) > currentDate)
                        .FirstOrDefault();
                if (menuOrderingAvailableSlot == null)
                {
                    CloseOrder();
                    return;
                }
            }
            var menuIds = RestaurantDetailPage.ListMenuItemViews.Values.SelectMany(t => t)
                                               .SelectMany(t => t.ListMenuGroupModifierItemSelected)
                                              .Select(s => s.MenuItemView.RestaurantMenuItem.MenuId).Distinct().ToList();
            foreach (var menuId in menuIds)
            {
                var minutes = RestaurantDetailPage.SearchAvailableOrderTimes(menuId, false);
                if (minutes <= 0)
                {
                    CloseOrder();
                    return;
                }
                else if (minutes > 0 && minutes <= 5)
                {
                    RestaurantDetailPage.SearchAvailableOrderTimes(menuId, true);
                    break;
                }
            }
        }
    }
}
using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages.Address;
using AsystBussiness.Pages.Checkout;
using AsystBussiness.ViewModels;
using AsystBussiness.Views;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.Restaurant
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestaurantDetailPage : ContentPageBase
    {
        //AddressFacade mAddressFacade = new AddressFacade();
        private IAppServices mAppServices;

        public GroupedDeliveryDestination GroupedDeliveryDestination { get; set; }
        public Service GroupedService { get; set; }

        public RestaurantVM RestaurantVM { get; set; }
        public Dictionary<int, ObservableCollection<MenuItemViewModel>> ListMenuItemViews = new Dictionary<int, ObservableCollection<MenuItemViewModel>>();
        public bool IsDeliverySelected;
        public string Comment;
        public List<RMenuOrderingAvailableSlot> MenuOrderingAvailables = new List<RMenuOrderingAvailableSlot>();
        public List<RMenuOrderingAvailableSlot> RefreshMenuAvailables = new List<RMenuOrderingAvailableSlot>();
        public List<DateTime> DateRestaurantCloses = new List<DateTime>();
        public DateTime? MinDate = null;
        public int NextDays = 7;
        public int CheckNextDays = 7;

        //ExtendedAddress mServiceAddress;
        //List<Service> Services = new List<Service>();

        List<RMenuVM> Menus = new List<RMenuVM>();
        List<DateTime> Dates = new List<DateTime>();
        List<DateTime> Times = new List<DateTime>();
        RestaurantFacade restaurantFacade = new RestaurantFacade();
        private int MenuTabIndex;
        private TimeZoneInfo RestaurantTimeZone;
        private bool FirstLoad = true;

        List<RMenuOrderingAvailableSlot> MenuOrderingAvailablesTemp = new List<RMenuOrderingAvailableSlot>();

        public StackLayout StackLayoutMenus
        {
            get;
            set;
        }

        public ScrollView ScrollViewMenus
        {
            get;
            set;
        }

        private bool mIsLoadingRestaurantDetail = false;
        public bool IsLoadingRestaurantDetail
        {
            set
            {
                this.OnPropertyChanging(nameof(IsLoadingRestaurantDetail));
                mIsLoadingRestaurantDetail = value;
                this.OnPropertyChanged(nameof(IsLoadingRestaurantDetail));
            }
            get
            {
                return mIsLoadingRestaurantDetail;
            }
        }

        private bool mDisclaimerVisible = false;
        public bool DisclaimerVisible
        {
            set
            {
                this.OnPropertyChanging(nameof(DisclaimerVisible));
                mDisclaimerVisible = value;
                this.OnPropertyChanged(nameof(DisclaimerVisible));
            }
            get
            {
                return mDisclaimerVisible;
            }
        }
        private string mDisclaimerText = string.Empty;
        public string DisclaimerText
        {
            set
            {
                this.OnPropertyChanging(nameof(DisclaimerText));
                mDisclaimerText = value;
                this.OnPropertyChanged(nameof(DisclaimerText));
            }
            get
            {
                return mDisclaimerText;
            }
        }

        private bool mCheckMenuAvailableVisible = false;
        public bool CheckMenuAvailableVisible
        {
            set
            {
                this.OnPropertyChanging(nameof(CheckMenuAvailableVisible));
                mCheckMenuAvailableVisible = value;
                this.OnPropertyChanged(nameof(CheckMenuAvailableVisible));
            }
            get
            {
                return mCheckMenuAvailableVisible;
            }
        }
        private string mCheckMenuAvailableText = string.Empty;
        public string CheckMenuAvailableText
        {
            set
            {
                this.OnPropertyChanging(nameof(CheckMenuAvailableText));
                mCheckMenuAvailableText = value;
                this.OnPropertyChanged(nameof(CheckMenuAvailableText));
            }
            get
            {
                return mCheckMenuAvailableText;
            }
        }

        private bool mCheckHolidayVisible = false;
        public bool CheckHolidayVisible
        {
            set
            {
                this.OnPropertyChanging(nameof(CheckHolidayVisible));
                mCheckHolidayVisible = value;
                this.OnPropertyChanged(nameof(CheckHolidayVisible));
            }
            get
            {
                return mCheckHolidayVisible;
            }
        }

        private bool mCheckTimeDeliveryVisible = false;
        public bool CheckTimeDeliveryVisible
        {
            set
            {
                this.OnPropertyChanging(nameof(CheckTimeDeliveryVisible));
                mCheckTimeDeliveryVisible = value;
                this.OnPropertyChanged(nameof(CheckTimeDeliveryVisible));
            }
            get
            {
                return mCheckTimeDeliveryVisible;
            }
        }
        private string mCheckTimeDeliveryText = string.Empty;
        public string CheckTimeDeliveryText
        {
            set
            {
                this.OnPropertyChanging(nameof(CheckTimeDeliveryText));
                mCheckTimeDeliveryText = value;
                this.OnPropertyChanged(nameof(CheckTimeDeliveryText));
            }
            get
            {
                return mCheckTimeDeliveryText;
            }
        }

        private bool mIsLoadingDateTime = false;
        public bool IsLoadingDateTime
        {
            set
            {
                this.OnPropertyChanging(nameof(IsLoadingDateTime));
                mIsLoadingDateTime = value;
                this.OnPropertyChanged(nameof(IsLoadingDateTime));
            }
            get
            {
                return mIsLoadingDateTime;
            }
        }

        private bool mIsSelectedDateDisplay = false;
        public bool IsSelectedDateDisplay
        {
            get
            {
                return mIsSelectedDateDisplay;
            }
            set
            {
                OnPropertyChanging(nameof(IsSelectedDateDisplay));
                mIsSelectedDateDisplay = value;
                OnPropertyChanged(nameof(IsSelectedDateDisplay));
            }
        }

        public DateTime SelectedDate { get; set; }

        public RestaurantDetailPage(RestaurantVM restaurantVM, DateTime deliveryDate, string deliveryTime)
        {
            try
            {
                SelectedDate = new DateTime(deliveryDate.Year, deliveryDate.Month, deliveryDate.Day).Add(DateTime.ParseExact(deliveryTime,
                                    "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay);

                this.RestaurantVM = restaurantVM;

                InitializeComponent();

                NavigationPage.SetBackButtonTitle(this, AppResources.Back);

                mAppServices = DependencyService.Get<IAppServices>();

                this.RestaurantTimeZone = mAppServices.GetTimeZoneById(RestaurantVM.Location.TimeZone);
                if (this.RestaurantTimeZone == null)
                {
                    this.RestaurantTimeZone = TimeZoneInfo.Local;
                }

                this.IsDeliverySelected = true;

                LabelItemCarts.FontSize = Device.GetNamedSize(NamedSize.Small, LabelItemCarts) * 0.7;
                LabelItemsPrice.FontSize = Device.GetNamedSize(NamedSize.Medium, LabelItemsPrice) * 0.63;
                LabelFees.FontSize = Device.GetNamedSize(NamedSize.Small, LabelFees) * 0.7;

                GridCheckButton.SizeChanged += (sender, e) =>
                {
                    if (GridCheckButton.Height > 1 && GridCheckButton.Width > 1)
                    {
                        GridCheckout.HeightRequest = GridCheckButton.Height;
                    //GridCheckout.HeightRequest = ImageCheckButton.Height;
                    //GridCheckButton.HeightRequest = ImageCheckButton.Height;
                    //GridCheckButton.WidthRequest = ImageCheckButton.Width;
                    if (this.Width >= 480)
                        {
                            LabelItemCarts.FontSize = Device.GetNamedSize(NamedSize.Small, LabelItemCarts) * 1.11;
                            LabelItemsPrice.FontSize = Device.GetNamedSize(NamedSize.Medium, LabelItemsPrice) * 1.11;
                            LabelFees.FontSize = Device.GetNamedSize(NamedSize.Small, LabelFees) * 1.11;
                        }
                        else if (this.Width >= 400)
                        {
                            LabelItemCarts.FontSize = Device.GetNamedSize(NamedSize.Small, LabelItemCarts) * 0.99;
                            LabelItemsPrice.FontSize = Device.GetNamedSize(NamedSize.Medium, LabelItemsPrice) * 0.99;
                            LabelFees.FontSize = Device.GetNamedSize(NamedSize.Small, LabelFees) * 0.99;
                        }
                        else if (this.Width >= 350)
                        {
                            LabelItemCarts.FontSize = Device.GetNamedSize(NamedSize.Small, LabelItemCarts) * 0.88;
                            LabelItemsPrice.FontSize = Device.GetNamedSize(NamedSize.Medium, LabelItemsPrice) * 0.88;
                            LabelFees.FontSize = Device.GetNamedSize(NamedSize.Small, LabelFees) * 0.88;
                        }
                        else
                        {
                            LabelItemCarts.FontSize = Device.GetNamedSize(NamedSize.Small, LabelItemCarts) * 0.8;
                            LabelItemsPrice.FontSize = Device.GetNamedSize(NamedSize.Medium, LabelItemsPrice) * 0.8;
                            LabelFees.FontSize = Device.GetNamedSize(NamedSize.Small, LabelFees) * 0.72;
                        }
                        GridCheckButton.IsVisible = true;
                    }
                };

                GridCheckButton.GestureRecognizers.Add(new TapGestureRecognizer()
                {
                    Command = new Command(() =>
                    {
                        if (CheckHolidayVisible)
                        {
                            DisplayAlert(AppResources.RestaurantIsClosed, "", AppResources.OK);
                        }
                        else
                        {
                            var myCartPage = new MyCartPage(this);
                            Utils.PushAsync(Navigation, myCartPage, true);
                        }
                    }),
                });              
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }
        
        public void CalculateFees()
        {
            LabelFees.Text = AppResources.FeeTaxes;
        }

        public void CheckCheckout()
        {
            var listMenuGroupModifierItemSelected = ListMenuItemViews.Values.SelectMany(t => t)
                                         .Where(t => t.IsSelected && t.ListMenuGroupModifierItemSelected.Count > 0)
                                         .SelectMany(t => t.ListMenuGroupModifierItemSelected);
            var count = listMenuGroupModifierItemSelected.Sum((t) => t.Quantity);
            decimal totalPrice = 0;
            foreach (var menuGroupModifierItemSelected in listMenuGroupModifierItemSelected)
            {
                menuGroupModifierItemSelected.UpdatePrice();
                totalPrice += menuGroupModifierItemSelected.Price;
            }
            LabelItemsPrice.Text = (AppResources.SubTotal + ": $" + totalPrice.ToString("0.00"));

            if (count < 1)
            {
                GridCheckout.IsVisible = false;
                GridCheckButton.IsVisible = false;
            }
            else
            {
                LabelItemCarts.Text = count + " " + (count > 1 ? AppResources.ItemsUp : AppResources.Item);
                GridCheckout.IsVisible = true;
                GridCheckButton.IsVisible = true;
            }
        }

        bool IsAppearing = false;

        public override void ReloadPage()
        {
            base.ReloadPage();

            LoadData();
        }

        public async Task LoadData()
        {
            this.IsLoadingRestaurantDetail = true;
            this.IsBusy = true;
            Menus.Clear();
            ListMenuItemViews.Clear();
            if (StackLayoutMenus != null)
            {
                StackLayoutMenus.Children.Clear();
            }
            GridMenuItems.Children.Clear();
            GridMenus.Children.Clear();

            await Task.Run(() =>
            {
                Utils.IReloadPageCurrent = this;
                try
                {
                    var dateUTC = DateTime.UtcNow;
                    var date = SelectedDate;
                    var nextTimeSlot = date;

                    bool isNext = false;

                    var menus = Shared.APIs.IRestaurant.GetMenus(RestaurantVM.LocationId);

                    RefreshMenuAvailables = restaurantFacade.RefreshMenus(menus, RestaurantVM.LocationId, dateUTC, date.AddDays(CheckNextDays), IsDeliverySelected, GroupedDeliveryDestination);
                    MenuOrderingAvailables = restaurantFacade.SearchAvailableOrderTimes(RefreshMenuAvailables)
                                     .OrderBy(t => t.StartTime)
                                     .ToList();


                    //MenuOrderingAvailables = restaurantFacade.SearchAvailableOrderTimes(RestaurantVM.LocationId, date.AddDays(days).ToString("MM/dd/yyyy"), date.AddDays(CheckNextDays).ToString("MM/dd/yyyy"), IsDeliverySelected)
                    //                                         .OrderBy(t => t.StartTime)
                    //                                         .ToList();

                    foreach (var menuOrderingAvailable in MenuOrderingAvailables.ToList())
                    {
                        string timeString = menuOrderingAvailable.CutoffTime.Time;
                        var time = GetDateTimeRestaurantFromLocal(timeString);
                        if (date < time)
                        {
                            timeString = menuOrderingAvailable.StartTime.Time;
                            time = GetDateTimeRestaurantFromLocal(timeString);
                            nextTimeSlot = time;
                            isNext = true;
                            break;
                        }
                    }

                    IsSelectedDateDisplay = true;

                    DateRestaurantCloses = restaurantFacade.GetDateRestaurantCloses(RestaurantTimeZone, RestaurantVM.LocationId, dateUTC, date.AddDays(CheckNextDays));

                    SearchHoliday(RestaurantVM.LocationId);

                    if (menus != null)
                    {
                        menus = menus.OrderBy((arg) => arg.ID).ToList();
                        foreach (var menu in menus)
                        {
                            RMenuVM menuVM = new RMenuVM();
                            menuVM.Menu = menu;
                            menuVM.MenuGroups = new List<RMenuGroupVM>();
                            Task.Run(() =>
                            {
                                try
                                {
                                    List<RMenuGroupVM> MenuGroups = new List<RMenuGroupVM>();
                                    var menusgroup = Shared.APIs.IRestaurant.GetMenuGroups(RestaurantVM.LocationId, menu.ID);
                                    foreach (var menugroup in menusgroup)
                                    {
                                        RMenuGroupVM GroupMenu = new RMenuGroupVM
                                        {
                                            Group = menugroup,
                                        };
                                        MenuGroups.Add(GroupMenu);
                                        int retryGroup = 0;
                                        while (retryGroup < 3)
                                        {
                                            try
                                            {
                                                var menuItems = Shared.APIs.IRestaurant.GetChildMenuItems(RestaurantVM.LocationId, menu.ID, menugroup.ID);
                                                GroupMenu.MenuItems = menuItems.Select(s => MapMenuItemBasePrice(new RMenuItemVM { LocationId = RestaurantVM.LocationId, MenuId = menu.ID, MenuItem = s })).ToList();
                                                break;
                                            }
                                            catch (Exception)
                                            {
                                                System.GC.Collect();
                                                new System.Threading.ManualResetEvent(false).WaitOne(1000);
                                                retryGroup++;
                                            }
                                        }

                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            var index = Menus.IndexOf(menuVM);
                                            if (index >= 0 && ListMenuItemViews.Count > index)
                                            {
                                                var menuItemViewModels = CreateMenuItemViewModels(GroupMenu, menu.ID);
                                                if (!ListMenuItemViews.ContainsKey(menu.ID))
                                                {
                                                    ListMenuItemViews.Add(menu.ID, new ObservableCollection<MenuItemViewModel>());
                                                    (GridMenuItems.Children[index] as RestaurantMenuListView).ListViewMenus.ItemsSource = ListMenuItemViews[menu.ID];
                                                }
                                                foreach (var menuItemViewModel in menuItemViewModels)
                                                {
                                                    ListMenuItemViews[menu.ID].Add(menuItemViewModel);
                                                    menuItemViewModel.NeedUpdateSize();
                                                }
                                                (GridMenuItems.Children[index] as RestaurantMenuListView).ListViewMenus.IsVisible = ListMenuItemViews[menu.ID].Count > 0;
                                                if (MenuTabIndex == index)
                                                {
                                                    RefreshMenus(RestaurantVM.LocationId, Menus[index].Menu.ID);
                                                }
                                                //(StackLayoutMenus.Children[index] as Layout<View>).Children[2].IsVisible = false;
                                            }
                                        });
                                    }
                                    menuVM.MenuGroups = MenuGroups;
                                }
                                catch (Exception ex)
                                {
                                    var message = ex.Message;
                                    Device.BeginInvokeOnMainThread(async () =>
                                    {
                                        await Utils.ShowErrorMessage(string.IsNullOrEmpty(message) ? AppResources.SomethingWentWrong : message);
                                    });
                                }
                            }).ContinueWith((arg) =>
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    var index = Menus.IndexOf(menuVM);
                                    if (index >= 0 && ListMenuItemViews.Count > index)
                                    {
                                        (GridMenuItems.Children[index] as RestaurantMenuListView).ListViewMenus.IsVisible = true;
                                        (GridMenuItems.Children[index] as RestaurantMenuListView).IndicatorView.IsVisible = false;
                                        (StackLayoutMenus.Children[index] as Layout<View>).Children[2].IsVisible = false;
                                    }
                                });
                            });
                            Menus.Add(menuVM);
                        }
                    }

                }
                catch (Exception ex)
                {
                    if (!this.IsErrorPage && Utils.IReloadPageCurrent == this)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Utils.ShowErrorMessage(string.IsNullOrEmpty(ex.Message) ? AppResources.SomethingWentWrong : ex.Message);
                        });
                    }
                }
                if (Utils.IReloadPageCurrent == this)
                {
                    Utils.IReloadPageCurrent = null;
                }
            }).ContinueWith(t =>
            {
                if (Menus != null)
                {
                    CalculateFees();

                    StackLayoutMenus = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 0,
                    };
                    ScrollViewMenus = new ScrollView
                    {
                        Orientation = ScrollOrientation.Horizontal,
                        IsClippedToBounds = true,
                    };
                    ScrollViewMenus.Content = StackLayoutMenus;
                    GridMenus.Children.Add(ScrollViewMenus);
                    foreach (var menu in Menus)
                    {
                        var gridTab = new Grid();
                        gridTab.BackgroundColor = Color.FromRgb(80, 80, 80);
                        gridTab.HorizontalOptions = LayoutOptions.Start;
                        gridTab.VerticalOptions = LayoutOptions.Start;
                        gridTab.HeightRequest = Device.GetNamedSize(NamedSize.Medium, typeof(Label)) * 2.2;
                        Label labelTab = new Label();
                        labelTab.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                        labelTab.VerticalTextAlignment = TextAlignment.Center;
                        labelTab.HorizontalTextAlignment = TextAlignment.Center;
                        labelTab.VerticalOptions = LayoutOptions.Center;
                        labelTab.HorizontalOptions = LayoutOptions.Center;
                        labelTab.Text = menu.Menu.DisplayName;
                        labelTab.TextColor = Color.Gray;
                        labelTab.Margin = new Thickness(18, 0, 20, 0);
                        ActivityIndicator activityIndicator = new ActivityIndicator();
                        activityIndicator.IsRunning = false;
                        activityIndicator.IsVisible = false;

                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            labelTab.Margin = new Thickness(19, 0, 23, 0);
                            activityIndicator.HeightRequest = 14;
                            activityIndicator.WidthRequest = 14;
                            activityIndicator.Margin = new Thickness(0, 4, 4, 0);
                        }
                        else
                        {
                            activityIndicator.HeightRequest = 14;
                            activityIndicator.WidthRequest = 14;
                            activityIndicator.Margin = new Thickness(0, 4, 4, 0);
                        }
                        activityIndicator.HorizontalOptions = LayoutOptions.End;
                        activityIndicator.VerticalOptions = LayoutOptions.Start;
                        activityIndicator.Color = AppearanceBase.Instance.PrimaryColor;

                        gridTab.GestureRecognizers.Add(new TapGestureRecognizer()
                        {
                            Command = new Command(() =>
                            {
                                if (StackLayoutMenus != null)
                                {
                                    var index = StackLayoutMenus.Children.IndexOf(gridTab);
                                    if (index >= 0)
                                    {
                                        ChangeTabMenu(index);
                                        double scrollX = 0;
                                        for (int i = 0; i < index; i++)
                                        {
                                            scrollX += StackLayoutMenus.Children[i].Width;
                                        }
                                        scrollX += StackLayoutMenus.Children[index].Width / 2;
                                        scrollX = Math.Min(Math.Max(0, (scrollX - ScrollViewMenus.Width / 2)), StackLayoutMenus.Width - ScrollViewMenus.Width);
                                        ScrollViewMenus.ScrollToAsync(scrollX, ScrollViewMenus.ScrollY, true);
                                    }
                                }
                            }),
                        });
                        gridTab.Children.Add(labelTab);
                        gridTab.Children.Add(new Grid
                        {
                            HeightRequest = 2,
                            BackgroundColor = Color.Transparent,
                            VerticalOptions = LayoutOptions.End,
                            IsVisible = false,
                        });
                        gridTab.Children.Add(activityIndicator);

                        StackLayoutMenus.Children.Add(gridTab);
                    }
                    RenderListView(Menus);

                    int menuIndex = 0;
                    bool isMenuAvailable = false;
                    while (Menus.Count > menuIndex)
                    {
                        MenuTabIndex = menuIndex;
                        var menuId = Menus[menuIndex].Menu.ID;
                        SearchAvailableOrderTimes(menuId);
                        RefreshMenus(RestaurantVM.LocationId, menuId, false);
                        if (!CheckMenuAvailableVisible)
                        {
                            isMenuAvailable = true;
                            break;
                        }
                        else
                        {
                            menuIndex++;
                        }
                    }
                    if (!isMenuAvailable && Menus != null && Menus.Count > 0)
                    {
                        MenuTabIndex = 0;
                        var menuId = Menus[0].Menu.ID;
                        SearchAvailableOrderTimes(menuId);
                        RefreshMenus(RestaurantVM.LocationId, menuId, false);
                    }
                    ChangeTabMenu(MenuTabIndex, true);
                }

                this.IsLoadingRestaurantDetail = false;
                this.IsBusy = false;
            }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            IsAppearing = true;

            if (FirstLoad)
            {
                FirstLoad = false;
                LoadData();
            }

            CheckCheckout();
        }

        public DateTime GetDateTimeRestaurantFromLocal(string timeString)
        {
            var dateTime = TimeZoneInfo.ConvertTime(DateTime.Parse(timeString), TimeZoneInfo.Utc);
            return GetDateTimeRestaurantFromUtc(dateTime);
        }

        public DateTime GetDateTimeRestaurantFromUtc(DateTime dateTime)
        {
            var dateTimeZone = dateTime.AddMilliseconds(RestaurantTimeZone.GetUtcOffset(dateTime).TotalMilliseconds);
            return dateTimeZone;
        }

        public DateTime GetDateLocalTimeRestaurant(DateTime dateTime)
        {
            var dateTimeZone = dateTime.AddMilliseconds(-RestaurantTimeZone.GetUtcOffset(dateTime).TotalMilliseconds).ToLocalTime();
            return dateTimeZone;
        }

        public void RenderListView(List<RMenuVM> Menus)
        {
            ListMenuItemViews.Clear();
            foreach (var menuHeader in Menus)
            {
                ObservableCollection<MenuItemViewModel> menuItemViews = new ObservableCollection<MenuItemViewModel>();
                RestaurantMenuListView restaurantMenuListView = new RestaurantMenuListView();
                foreach (var menuSection in menuHeader.MenuGroups)
                {
                    MenuItemViewModel menuItemView = new MenuItemViewModel();
                    menuItemView.SectionName = menuSection.Group.DisplayName;
                    menuItemView.SectionDetail = menuSection.Group.DetailedDescription;

                    menuItemViews.Add(menuItemView);
                    foreach (var menuItem in menuSection.MenuItems)
                    {
                        if (!menuItem.MenuItem.ComboOnly)
                        {
                            menuItemView = new MenuItemViewModel();
                            menuItemView.RestaurantMenuItem = new RestaurantMenuItemViewModel()
                            {
                                Id = menuItem.MenuItem.ID,
                                MenuId = menuHeader.Menu.ID,
                                DisplayName = menuItem.MenuItem.DisplayName,
                                BasePrice = menuItem.MenuItem.BasePrice,
                                DetailedDescription = menuItem.MenuItem.DetailedDescription,
                                MenuItemVM = menuItem
                            };
                            menuItemView.PropertyChanged += (sender, e) =>
                            {
                                if (e.PropertyName == nameof(menuItemView.IsSelected))
                                {
                                    CheckCheckout();
                                }
                            };
                            menuItemViews.Add(menuItemView);
                        }
                    }
                }
                bool restaurantMenuListViewItemTapping = false;
                restaurantMenuListView.ListViewMenus.ItemTapped += async (sender, e) =>
                {
                    if (!restaurantMenuListViewItemTapping)
                    {
                        restaurantMenuListViewItemTapping = true;
                        if (e.Item != null)
                        {
                            var menuItemView = e.Item as MenuItemViewModel;
                            restaurantMenuListView.ListViewMenus.SelectedItem = null;
                            if (menuItemView.IsMenuItem)
                            {
                                if (menuItemView.IsEnabled)
                                {
                                    var name = menuItemView.RestaurantMenuItem.DisplayName;
                                    var restaurantMenuItemModifierPage = new RestaurantMenuItemModifierPage(RestaurantVM, menuItemView);
                                    await Utils.PushAsync(Navigation, restaurantMenuItemModifierPage, true);
                                    restaurantMenuItemModifierPage.Saved += (obj) =>
                                    {
                                        //var menuItemViewModels = (sender as ListView).ItemsSource.Cast<MenuItemViewModel>().ToList();
                                        //var index = menuItemViewModels.IndexOf(menuItemView);
                                        //foreach (var menuItemViewModel in menuItemViewModels.Skip(index - 10).Take(10))
                                        //{
                                        //	menuItemViewModel.NeedUpdateSize();
                                        //}
                                    };
                                }
                                else
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        if (this.Navigation.NavigationStack.LastOrDefault() == this && IsAppearing)
                                        {
                                            DisplayAlert(AppResources.MenuIsClosed, CheckMenuAvailableText, AppResources.OK);
                                        }
                                    });
                                }
                            }
                        }
                        restaurantMenuListViewItemTapping = false;
                    }
                };
                GridMenuItems.Children.Add(restaurantMenuListView);
                restaurantMenuListView.ListViewMenus.ItemsSource = menuItemViews;
                restaurantMenuListView.ListViewMenus.IsVisible = menuItemViews != null && menuItemViews.Count > 0;
                ListMenuItemViews.Add(menuHeader.Menu.ID, menuItemViews);
            }
        }

        public List<MenuItemViewModel> CreateMenuItemViewModels(RMenuGroupVM menuSection, int menuID)
        {
            List<MenuItemViewModel> menuItemViews = new List<MenuItemViewModel>();

            MenuItemViewModel menuItemView = new MenuItemViewModel();
            menuItemView.SectionName = menuSection.Group.DisplayName;
            menuItemView.SectionDetail = menuSection.Group.DetailedDescription;

            menuItemViews.Add(menuItemView);
            foreach (var menuItem in menuSection.MenuItems)
            {
                if (!menuItem.MenuItem.ComboOnly)
                {
                    menuItemView = new MenuItemViewModel();
                    menuItemView.RestaurantMenuItem = new RestaurantMenuItemViewModel()
                    {
                        Id = menuItem.MenuItem.ID,
                        MenuId = menuID,
                        DisplayName = menuItem.MenuItem.DisplayName,
                        BasePrice = menuItem.MenuItem.BasePrice,
                        DetailedDescription = menuItem.MenuItem.DetailedDescription,
                        MenuItemVM = menuItem
                    };
                    menuItemView.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(menuItemView.IsSelected))
                        {
                            CheckCheckout();
                        }
                    };
                    menuItemViews.Add(menuItemView);
                }
            }
            return menuItemViews;
        }

        public List<MenuItemViewModel> CreateMenuItemViewModels(RMenuVM menuHeader)
        {
            List<MenuItemViewModel> menuItemViews = new List<MenuItemViewModel>();
            foreach (var menuSection in menuHeader.MenuGroups)
            {
                var menuItemViewsSection = CreateMenuItemViewModels(menuSection, menuHeader.Menu.ID);
                menuItemViews.AddRange(menuItemViewsSection);
            }
            return menuItemViews;
        }
        
        public void ClearMenuItemViewsClose(List<RMenuOrderingAvailableSlot> refreshMenuAvailables, DateTime selectedDate)
        {
            var menuItemViewsClose = GetMenuItemViewsClose(refreshMenuAvailables, selectedDate);
            foreach (var menuItemViewClose in menuItemViewsClose)
            {
                menuItemViewClose.ListMenuGroupModifierItemSelected.Clear();
                menuItemViewClose.IsSelected = false;
            }
            CheckCheckout();
        }

        public List<MenuItemViewModel> GetMenuItemViewsClose(List<RMenuOrderingAvailableSlot> refreshMenuAvailables, DateTime selectedDate)
        {
            var menuItemViewsClose = new List<MenuItemViewModel>();

            foreach (var menu in Menus)
            {
                var menuId = menu.Menu.ID;
                DateTime fromTime = selectedDate, toTime = selectedDate;
                var menuTimeAvailable = GetMenuTimeAvailables(refreshMenuAvailables, menuId, selectedDate.Date);
                var menuTimeAvailableFirst = menuTimeAvailable.FirstOrDefault();
                var menuTimeAvailableLast = menuTimeAvailable.LastOrDefault();
                bool isAvailable = false;

                if (menuTimeAvailableFirst != null)
                {
                    var fromTimeString = menuTimeAvailableFirst.StartTime.Time;
                    var toTimeString = menuTimeAvailableLast.StartTime.Time;
                    fromTime = GetDateTimeRestaurantFromLocal(fromTimeString);
                    toTime = GetDateTimeRestaurantFromLocal(toTimeString);
                    if (selectedDate >= fromTime && toTime >= selectedDate)
                    {
                        isAvailable = true;
                    }
                    else
                    {
                        isAvailable = false;
                    }
                }

                if (!isAvailable)
                {
                    var listMenuItemViews = ListMenuItemViews.Values.SelectMany(t => t).Where(t => t.IsSelected && t.RestaurantMenuItem != null && t.RestaurantMenuItem.MenuId == menuId).ToList();
                    menuItemViewsClose.AddRange(listMenuItemViews);
                }
            }
            return menuItemViewsClose;
        }

        public List<RMenuOrderingAvailableSlot> GetMenuTimeAvailables(List<RMenuOrderingAvailableSlot> menuOrderingAvailableSlots, int menuId, DateTime date)
        {
            var menuTimeAvailable = menuOrderingAvailableSlots
                                    .Where(t => t.RelatedMenuIDs.Contains(menuId))
                                    .Where(t => date.Date == GetDateTimeRestaurantFromLocal(t.StartTime.Time).Date)
                                    .ToList();
            return menuTimeAvailable;
        }

        public bool CheckAvaibleMenus(int menuId, DateTime date)
        {
            var checkMenuAvailableText = CheckMenuAvailableText;

            CheckMenuAvailableVisible = false;
            CheckMenuAvailableText = string.Empty;

            DateTime fromTime = date, toTime = date;
            var menuTimeAvailable = GetMenuTimeAvailables(RefreshMenuAvailables, menuId, date.Date);
            var menuTimeAvailableFirst = menuTimeAvailable.FirstOrDefault();
            var menuTimeAvailableLast = menuTimeAvailable.LastOrDefault();
            bool isAvailable = false;

            if (menuTimeAvailableFirst != null)
            {
                var fromTimeString = menuTimeAvailableFirst.CutoffTime.Time;
                var toTimeString = menuTimeAvailableLast.CutoffTime.Time;
                fromTime = GetDateTimeRestaurantFromLocal(fromTimeString);
                toTime = GetDateTimeRestaurantFromLocal(toTimeString);
                if (date >= fromTime && toTime >= date)
                {
                    isAvailable = true;
                }
                else
                {
                    isAvailable = false;
                }
            }

            return isAvailable;
        }


        public bool RefreshMenus(int locid, int menuId, bool isShowMessage = false)
        {
            var checkMenuAvailableText = CheckMenuAvailableText;

            CheckMenuAvailableVisible = false;
            CheckMenuAvailableText = string.Empty;

            DateTime fromTime = SelectedDate, toTime = SelectedDate;
            var menuTimeAvailable = GetMenuTimeAvailables(RefreshMenuAvailables, menuId, SelectedDate.Date);

            var menuTimeAvailableFirst = menuTimeAvailable.FirstOrDefault();
            var menuTimeAvailableLast = menuTimeAvailable.LastOrDefault();
            bool isAvailable = false;

            if (menuTimeAvailableFirst != null)
            {
                var fromTimeString = menuTimeAvailableFirst.StartTime.Time;
                var toTimeString = menuTimeAvailableLast.StartTime.Time;
                fromTime = GetDateTimeRestaurantFromLocal(fromTimeString);
                toTime = GetDateTimeRestaurantFromLocal(toTimeString);
                if (SelectedDate >= fromTime && toTime >= SelectedDate)
                {
                    isAvailable = true;
                }
                else
                {
                    isAvailable = false;
                }
                if (!isAvailable)
                {
                    CheckMenuAvailableText = string.Format(AppResources.CheckMenuAvailableTextNoTime);
                    //CheckMenuAvailableText = string.Format(AppResources.CheckMenuAvailableText, fromTime.ToString("h:mm tt"), toTime.ToString("h:mm tt"));
                    CheckMenuAvailableVisible = true;
                }
            }
            else
            {
                CheckMenuAvailableText = string.Format(AppResources.CheckMenuAvailableTextNoTime);
                CheckMenuAvailableVisible = true;
            }

            if (CheckHolidayVisible)
            {
                isAvailable = false;
                CheckMenuAvailableVisible = false;
                CheckTimeDeliveryVisible = false;
            }
            else if (CheckMenuAvailableVisible)
            {
                CheckTimeDeliveryVisible = false;
            }


            if (ListMenuItemViews.ContainsKey(menuId))
            {
                foreach (var menuItemView in ListMenuItemViews[menuId])
                {
                    menuItemView.IsEnabled = isAvailable;
                }
            }

            if (CheckMenuAvailableVisible && !CheckMenuAvailableText.Equals(checkMenuAvailableText) && isShowMessage)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (this.Navigation.NavigationStack.LastOrDefault() == this && IsAppearing)
                    {
                        DisplayAlert(AppResources.MenuIsClosed, CheckMenuAvailableText, AppResources.OK);
                    }
                });
            }

            return isAvailable;
        }

        public bool SearchHoliday(int locid)
        {
            //var result = restaurantFacade.SearchHoliday(locid, SelectedDate);
            //if (result == "Closed")
            //{
            //	CheckHolidayVisible = true;
            //	CheckMenuAvailableVisible = false;
            //	CheckTimeDeliveryVisible = false;
            //	return true;
            //}
            //else if (result == "Holiday")
            //{
            //	CheckHolidayVisible = true;
            //	CheckMenuAvailableVisible = false;
            //	CheckTimeDeliveryVisible = false;
            //	return true;
            //}
            if (DateRestaurantCloses != null && DateRestaurantCloses.Contains(SelectedDate.Date))
            {
                CheckHolidayVisible = true;
                CheckMenuAvailableVisible = false;
                CheckTimeDeliveryVisible = false;
                return true;
            }
            else
            {
                CheckHolidayVisible = false;
                return false;
            }
        }

        public RMenuOrderingAvailableSlot GetRMenuOrderingAvailableSlot(DateTime selectedDate)
        {
            var menu = MenuOrderingAvailables.FirstOrDefault((arg) => GetDateTimeRestaurantFromLocal(arg.StartTime.Time) == selectedDate);
            return menu;
        }

        public DateTime GetCutOffTime()
        {
            var cutOffTime = SelectedDate;
            var menu = MenuOrderingAvailables.FirstOrDefault((arg) => GetDateTimeRestaurantFromLocal(arg.StartTime.Time) == SelectedDate);
            if (menu != null)
            {
                cutOffTime = GetDateTimeRestaurantFromLocal(menu.CutoffTime.Time);
            }
            return cutOffTime;
        }

        public int SearchAvailableOrderTimes()
        {
            if (Menus != null && Menus.Count > MenuTabIndex && Menus[MenuTabIndex].Menu != null)
            {
                var menuId = Menus[MenuTabIndex].Menu.ID;
                return SearchAvailableOrderTimes(menuId);
            }
            return -1;
        }

        public int SearchAvailableOrderTimes(int menuId, bool isUpdateUI = true)
        {
            var date = GetDateTimeRestaurantFromUtc(DateTime.UtcNow);
            DateTime timeCurrent = date;
            var menuTimeAvailable = GetMenuTimeAvailables(RefreshMenuAvailables, menuId, SelectedDate);

            var menuTimeAvailableLast = menuTimeAvailable.LastOrDefault();
            var menuTimeAvailableFirst = menuTimeAvailable.FirstOrDefault();

            if (menuTimeAvailableLast != null)
            {
                var fromTimeString = menuTimeAvailableFirst.StartTime.Time;
                var fromTime = GetDateTimeRestaurantFromLocal(fromTimeString);

                var toTimeCutOffString = menuTimeAvailableLast.CutoffTime.Time;
                var toTimeCutOff = GetDateTimeRestaurantFromLocal(toTimeCutOffString);

                var toEndTimeString = menuTimeAvailableLast.EndTime.Time;
                var toEndTime = GetDateTimeRestaurantFromLocal(toEndTimeString);

                if (toEndTime.Subtract(fromTime).TotalDays < 1)
                {
                    TimeSpan total = toTimeCutOff.Subtract(timeCurrent);
                    var minutes = (int)Math.Ceiling(total.TotalMinutes);
                    if (isUpdateUI)
                    {
                        if (minutes <= 5 && minutes > 0)
                        {
                            CheckTimeDeliveryVisible = true;
                            CheckTimeDeliveryText = AppResources.PleaseHurryUp + minutes + " min.";
                        }
                        else
                        {
                            CheckTimeDeliveryVisible = false;
                        }
                    }
                    return minutes;
                }
            }

            return int.MaxValue;
        }

        // GET: Set Menu Item Base price
        public MenuItemVM MapMenuItemBasePrice(RMenuItemVM model)
        {
            List<decimal?> PriceList = new List<decimal?>();
            MenuItemVM menuItemVM = new MenuItemVM { MenuItemOriginalPrice = model.MenuItem.BasePrice };
            if (model.MenuItem.BasePrice == null)
            {
                if (model.MenuItem.ModifierPriceIDs != null && model.MenuItem.ModifierPriceIDs.Count > 0)
                {
                    foreach (var modifierPrice in model.MenuItem.ModifierPriceIDs)
                    {
                        var pricemodel = Shared.APIs.IRestaurant.GetModifierPrice(model.LocationId, model.MenuId, model.MenuItem.ID, modifierPrice);

                        PriceList.Add(pricemodel.Price);
                    }
                    model.MenuItem.BasePrice = PriceList.Where(f => f > 0).Min();
                }
            }

            menuItemVM.MenuItem = model.MenuItem;
            return menuItemVM;
        }

        private void ChangeTabMenu(int index, bool forceCheck = false)
        {
            MenuTabIndex = index;

            DisclaimerVisible = false;
            if (!string.IsNullOrEmpty(Menus[index].Menu.Disclaimer))
            {
                DisclaimerText = Menus[index].Menu.Disclaimer;
                DisclaimerVisible = true;
            }

            if (!forceCheck)
            {
                var menuId = Menus[MenuTabIndex].Menu.ID;
                SearchAvailableOrderTimes(menuId);
                RefreshMenus(RestaurantVM.LocationId, menuId);
            }

            for (int i = 0; i < StackLayoutMenus.Children.Count; i++)
            {
                (GridMenuItems.Children[i] as RestaurantMenuListView).ListViewMenus.Header = index == i ? this : null;
                GridMenuItems.Children[i].IsVisible = index == i;
                ((StackLayoutMenus.Children[i] as Layout<View>).Children[0] as Label).TextColor = index == i ? Color.White : Color.Gray;
                (StackLayoutMenus.Children[i] as Layout<View>).BackgroundColor = index == i ? AppearanceBase.Instance.OrangeColor : Color.FromRgb(80, 80, 80);
                ((StackLayoutMenus.Children[i] as Layout<View>).Children[1]).BackgroundColor = index == i ? AppearanceBase.Instance.PrimaryColor : Color.Transparent;
            }
        }
    }
}
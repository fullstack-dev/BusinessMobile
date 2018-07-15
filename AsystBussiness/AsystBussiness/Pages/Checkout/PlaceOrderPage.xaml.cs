using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages.Restaurant;
using ColonyConcierge.APIData.Data;
using ColonyConcierge.Client.PlatformServices;
using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.Checkout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaceOrderPage : ContentPageBase
    {
        private bool mFirstLoad = true;
        private string FormatDate = "MM/yyyy";

        public RestaurantDetailPage RestaurantDetailPage
        {
            get;
            set;
        }
        ExtendedAddress mServiceAddress;
        private IAppServices mAppServices;

        decimal TotalPrice = 0;
        ColonyConcierge.APIData.Data.User UserModel;
        //AddressFacade mAddressFacade = new AddressFacade();
        List<Service> Services = new List<Service>();
        private bool IsPauseTrigger = false;

        decimal SubPrice;
        decimal ServiceFee;
        decimal SalesTax;
        decimal Tips;
        decimal Discount;

        private void discountButton_Clicked(object sender, EventArgs e)
        {
            if (AppResources.Remove.Equals(discountButton.Text))
            {
                discountEntry.IsEnabled = true;
                discountEntry.Text = "";
                Discount = 0;

                discountButton.Text = AppResources.Apply;
                discountValue.Text = "$0.00";
                calculateTax();
                UpdatePrice();
            }

            string discountText = discountEntry.Text;
            if (String.IsNullOrEmpty(discountText))
            {
                return;
            }


            this.IsBusy = true;
            PendingDiscount discount = null;
            Task.Run(() =>
            {
                try
                {
                    discount = Shared.APIs.ICoupons.GetAvailableDiscount(discountText, Shared.UserId, Services.FirstOrDefault().ID, "");
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        discountValue.Text = "$0.00";
                        Discount = 0;
                        discountEntry.Text = "";
                        await Utils.ShowWarningMessage(string.IsNullOrEmpty(ex.Message) ? AppResources.SomethingWentWrong : ex.Message, 3);
                    });
                }
            }).ContinueWith((t) =>
            {
                decimal calculatedDiscount = 0;
                if (discount != null)
                {
                    if (discount.LimitToFees)
                    {
                        if (discount.DiscountPercentageAmount != null)
                        {
                            calculatedDiscount = (ServiceFee * discount.DiscountPercentageAmount.GetValueOrDefault());
                            if (discount.DiscountFlatAmount != null && calculatedDiscount > discount.DiscountFlatAmount)
                            {
                                calculatedDiscount = discount.DiscountFlatAmount.GetValueOrDefault();
                            }
                        }
                        else
                        {
                            calculatedDiscount = Math.Min(ServiceFee, discount.DiscountFlatAmount.GetValueOrDefault());
                        }

                    }
                    else
                    {
                        if (discount.DiscountPercentageAmount != null)
                        {
                            calculatedDiscount = (SubPrice * discount.DiscountPercentageAmount.GetValueOrDefault());
                            if (discount.DiscountFlatAmount != null && calculatedDiscount > discount.DiscountFlatAmount)
                            {
                                calculatedDiscount = discount.DiscountFlatAmount.GetValueOrDefault();
                            }
                        }
                        else
                        {
                            if (discount.DiscountAsCash)
                            {
                                calculatedDiscount = discount.DiscountFlatAmount.GetValueOrDefault();
                            }
                            else
                            {
                                SubPrice -= discount.DiscountFlatAmount.GetValueOrDefault();
                                calculatedDiscount = 0;
                                calculateTax();
                            }
                        }
                    }
                    discountValue.Text = "$" + calculatedDiscount.ToString("0.00");
                    Discount = calculatedDiscount;
                    discountButton.Text = AppResources.Remove;
                    discountEntry.IsEnabled = false;
                }

                UpdatePrice();
                this.IsBusy = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());


        }

        void SetupCreditCardFields()
        {
            creditCardNumberEntry.TextChanged += (sender, e) =>
            {
                var oldTextValueLength = e.OldTextValue != null ? e.OldTextValue.Length : 0;
                if (e.NewTextValue != null && e.NewTextValue.Length > oldTextValueLength)
                {
                    string text = creditCardNumberEntry.Text.Replace(" ", "");
                    string newNumber = string.Empty;
                    for (int i = 0; i < text.Length / 4; i++)
                    {
                        newNumber += text.Substring(i * 4, 4) + " ";
                    }
                    var lastNumber = text.Length / 4 * 4;
                    if (lastNumber >= 0 && text.Length > lastNumber)
                    {
                        newNumber += text.Substring(lastNumber);
                    }
                    creditCardNumberEntry.Text = newNumber;
                }

                var cardNumber = creditCardNumberEntry.Text.Replace(" ", "");
                var lengthFrom = 12;
                var lengthTo = -1;
                var cardType = mAppServices.GetCreditCardType(cardNumber);
                cvvValidator.MinLength = 3;
                EntryCvv.Placeholder = AppResources.CVV;
                switch (cardType)
                {
                    case CreditCardType.Amex:
                        lengthFrom = 15;
                        lengthTo = 15;
                        EntryCvv.Placeholder = AppResources.CID;
                        cvvValidator.MinLength = 4;
                        break;
                    case CreditCardType.Discover:
                        EntryCvv.Placeholder = AppResources.CID;
                        lengthFrom = 16;
                        lengthTo = 16;
                        break;
                    case CreditCardType.Mastercard:
                        EntryCvv.Placeholder = AppResources.CVC2;
                        lengthFrom = 16;
                        lengthTo = 16;
                        break;
                    case CreditCardType.Jcb:
                        EntryCvv.Placeholder = AppResources.CVV;
                        lengthFrom = 16;
                        lengthTo = 19;
                        break;
                    case CreditCardType.Maestro:
                        EntryCvv.Placeholder = AppResources.CVV;
                        lengthFrom = 12;
                        lengthTo = 19;
                        break;
                    case CreditCardType.Visa:
                        EntryCvv.Placeholder = AppResources.CVV;
                        lengthFrom = 12;
                        lengthTo = 19;
                        break;
                }
                cvvValidator.FieldName = EntryCvv.Placeholder;

                if (cardNumber.Length >= lengthFrom && (lengthTo < 0 || cardNumber.Length <= lengthTo))
                {
                    LabelCreditCardNumberValidator.Text = " ";
                }
                else
                {
                    if (lengthTo > 0)
                    {
                        if (lengthTo == lengthFrom)
                        {
                            LabelCreditCardNumberValidator.Text = string.Format(AppResources.CreditCardNumberRequire, lengthFrom, lengthTo);
                        }
                        else
                        {
                            LabelCreditCardNumberValidator.Text = string.Format(AppResources.CreditCardNumberRequireMinMax, lengthFrom, lengthTo);
                        }
                    }
                    else
                    {
                        LabelCreditCardNumberValidator.Text = string.Format(AppResources.CreditCardNumberRequireMin, lengthFrom);
                    }
                }
            };

            EntryMMYY.TextChanged += (sender, e) =>
            {
                var length = EntryMMYY.Text.Length;
                if (length == 1)
                {
                    try
                    {
                        int month = int.Parse(EntryMMYY.Text);
                        if (month < 2)
                        {
                        }
                        else if (month >= 2)
                        {
                            EntryMMYY.Text = "0" + month + "/";
                        }
                    }
                    catch (Exception)
                    {
                        EntryMMYY.Text = e.OldTextValue;
                    }
                }
                if (length == 2)
                {
                    var oldLength = e.OldTextValue.Length;
                    if (oldLength == 3)
                    {
                        EntryMMYY.Text = EntryMMYY.Text.Substring(0, 1);
                    }
                    else
                    {
                        try
                        {
                            int month = int.Parse(EntryMMYY.Text);
                            if (month <= 12)
                            {
                                EntryMMYY.Text = EntryMMYY.Text + "/";
                            }
                            else
                            {
                                EntryMMYY.Text = e.OldTextValue;
                            }
                        }
                        catch (Exception)
                        {
                            EntryMMYY.Text = e.OldTextValue;
                        }
                    }
                }
                if (length > FormatDate.Length)
                {
                    EntryMMYY.Text = e.OldTextValue;
                }

                try
                {
                    if (EntryMMYY.Text.Length != FormatDate.Length)
                    {
                        LabelMMYY.Text = AppResources.ExpirationDateNotCorrect;
                    }
                    else
                    {
                        var date = DateTime.ParseExact(EntryMMYY.Text, FormatDate, null);
                        if (date.Date < DateTime.Now.AddDays(1 - DateTime.Now.Day).Date)
                        {
                            LabelMMYY.Text = AppResources.ExpirationDateIsExpired;
                        }
                        else
                        {
                            LabelMMYY.Text = " ";
                        }
                    }
                }
                catch (Exception)
                {
                    LabelMMYY.Text = AppResources.ExpirationDateNotCorrect;
                }
            };
        }

        async Task<CreditCardData> SetupCreditCard()
        {
            try
            {
                var token = Shared.APIs.IAccounts.GetPaymentGatewayToken();
                BrainTreeCard brainTreeCard = new BrainTreeCard();
                brainTreeCard.Token = token;
                brainTreeCard.CardNumber = creditCardNumberEntry.Text.Replace(" ", "");
                brainTreeCard.ExpirationDate = DateTime.ParseExact(EntryMMYY.Text, FormatDate, null);
                brainTreeCard.PostalCode = EntryPostalCode.Text;
                brainTreeCard.Cvv = EntryCvv.Text;

                var tcs = new TaskCompletionSource<CreditCardData>();

                mAppServices.GetNoncenBrainTree(brainTreeCard, async (obj) =>
                {
                    if (obj is string && !string.IsNullOrEmpty(obj as string))
                    {
                        try
                        {
                            var paymentNonce = obj as string;
                            CreditCardData paymentAccountData = new CreditCardData();
                            paymentAccountData.PaymentNonce = paymentNonce;
                            tcs.SetResult(paymentAccountData);
                        }
                        catch (Exception ex)
                        {
                            var notificator = DependencyService.Get<IToastNotificator>();
                            await notificator.Notify(ToastNotificationType.Error, AppResources.PaymentMethodTitle, ex.Message, TimeSpan.FromSeconds(5));
                            System.Diagnostics.Debug.WriteLine("Exception occured When Add Payment Method : " + ex.Message);
                            tcs.SetCanceled();
                        }
                    }
                    else
                    {
                        tcs.SetCanceled();
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                var notificator = DependencyService.Get<IToastNotificator>();
                await notificator.Notify(ToastNotificationType.Error, AppResources.PaymentMethodTitle, ex.Message, TimeSpan.FromSeconds(5));
                System.Diagnostics.Debug.WriteLine("Exception occured When Add Payment Method : " + ex.Message);
            }

           


            return null;
        }

        public PlaceOrderPage(string comment, RestaurantDetailPage restaurantDetailPage, ExtendedAddress serviceAddress)
        {
            RestaurantDetailPage = restaurantDetailPage;
            mServiceAddress = serviceAddress;

            InitializeComponent();

            mAppServices = DependencyService.Get<IAppServices>();

            this.Title = RestaurantDetailPage.Title;

            var listMenuItemViews = RestaurantDetailPage.ListMenuItemViews.Values.SelectMany(t => t);
            var listMenuGroupModifierItemSelected = listMenuItemViews.SelectMany(t => t.ListMenuGroupModifierItemSelected).ToList();

            SetupCreditCardFields();

            ButtonCheckout.Clicked += async (sender, e) =>
            {
                if (!cvvValidator.IsValid || !postalCodeValidator.IsValid || string.IsNullOrEmpty(creditCardNumberEntry.Text))
                {
                    return;
                }

                IsBusy = true;

                CreditCardData CardSelected = await SetupCreditCard();

                if (CardSelected != null)
                {
                    IsPauseTrigger = true;
                    var id = 0;
                    
                    this.ButtonCheckout.Text = AppResources.PlacingOrder;
                    var totalPrice = LabelTotal.Text;
                    var serviceFee = LabelServiceFee.Text;
                    var salesTax = LabelSalesTax.Text;

                    var scheduledService = new ColonyConcierge.APIData.Data.ScheduledRestaurantService();

                    await Task.Run(() =>
                    {
                        try
                        {
                            if (Services.FirstOrDefault() == null)
                            {
                                throw new Exception(AppResources.CanNotFoundService);
                            }

                            var paymentAccountData = CardSelected;
                            scheduledService.PaymentMethodID = paymentAccountData.ID;
                            scheduledService.Delivery = restaurantDetailPage.IsDeliverySelected;
                            scheduledService.FrontEndSubtotal = SubPrice;
                            scheduledService.FrontEndTaxes = SalesTax;
                            scheduledService.DiscountCodes = discountEntry.Text;

                            scheduledService.RestaurantLocationID = restaurantDetailPage.RestaurantVM.LocationId;
                            scheduledService.Tip = Tips;
                            scheduledService.SpecialInstructions = comment;
                            scheduledService.ServiceID = Services.FirstOrDefault().ID;

                            var menuIds = RestaurantDetailPage.ListMenuItemViews.Values.SelectMany(t => t)
                                                  .SelectMany(t => t.ListMenuGroupModifierItemSelected)
                                                  .Select(s => s.MenuItemView.RestaurantMenuItem.MenuId).Distinct().ToList();
                            var timeslots = restaurantDetailPage.RefreshMenuAvailables.Where((arg) => restaurantDetailPage.GetDateTimeRestaurantFromLocal(arg.StartTime.Time) == restaurantDetailPage.SelectedDate);
                            var timeslot = timeslots
                                        .Where(s =>
                                        {
                                            return !menuIds.Except(s.RelatedMenuIDs).Any();
                                        }).FirstOrDefault();

                            if (timeslot != null)
                            {
                                scheduledService.SlotID = timeslot.ID;
                            }

                            scheduledService.ServiceDate = new SimpleDate(restaurantDetailPage.SelectedDate.Year, restaurantDetailPage.SelectedDate.Month, restaurantDetailPage.SelectedDate.Day);
                            scheduledService.ServiceAddress = mServiceAddress;
                            if (restaurantDetailPage.GroupedDeliveryDestination != null)
                            {
                                scheduledService.DestinationID = restaurantDetailPage.GroupedDeliveryDestination.ID;
                            }

                            foreach (var menuGroupModifierItemSelected in listMenuGroupModifierItemSelected)
                            {
                                ScheduledRMenuItem scheduledRMenuItem = new ScheduledRMenuItem();
                                scheduledRMenuItem.Quantity = menuGroupModifierItemSelected.Quantity;
                                scheduledRMenuItem.DisplayName = menuGroupModifierItemSelected.MenuItemView.RestaurantMenuItem.DisplayName;
                                scheduledRMenuItem.FrontEndPrice = menuGroupModifierItemSelected.Price;
                                scheduledRMenuItem.RelatedMenuID = menuGroupModifierItemSelected.MenuItemView.RestaurantMenuItem.MenuId;
                                scheduledRMenuItem.RelatedMenuItemID = menuGroupModifierItemSelected.MenuItemView.RestaurantMenuItem.Id;
                                scheduledRMenuItem.SpecialInstructions = menuGroupModifierItemSelected.Comment;

                                foreach (var rMenuGroupModifierVM in menuGroupModifierItemSelected.ListRMenuGroupModifierVM)
                                {
                                    var menuModifiers = rMenuGroupModifierVM.MenuModifiers.Where(t => t.IsSelected).ToList();
                                    foreach (var menuModifier in menuModifiers)
                                    {
                                        var scheduledRMenuItemModifier = new ScheduledRMenuItemModifier();
                                        scheduledRMenuItemModifier.DisplayName = menuModifier.MenuModifier.DisplayName;
                                        scheduledRMenuItemModifier.Quantity = menuModifier.Quantity;
                                        scheduledRMenuItemModifier.RelatedModiferID = menuModifier.MenuModifier.ID;
                                        scheduledRMenuItem.AppliedModifiers.Add(scheduledRMenuItemModifier);
                                        var subMenuModifiers = menuModifier.SubMenuModifiers.Where(t => t.IsSelected).ToList();
                                        foreach (var subMenuModifier in subMenuModifiers)
                                        {
                                            var scheduledRMenuSubItemModifier = new ScheduledRMenuItemModifier();
                                            scheduledRMenuSubItemModifier.DisplayName = subMenuModifier.MenuModifier.DisplayName;
                                            scheduledRMenuSubItemModifier.Quantity = subMenuModifier.Quantity;
                                            scheduledRMenuSubItemModifier.RelatedModiferID = subMenuModifier.MenuModifier.ID;
                                            scheduledRMenuItem.AppliedModifiers.Add(scheduledRMenuSubItemModifier);
                                        }
                                    }
                                }
                                scheduledService.Items.Add(scheduledRMenuItem);
                            }

                            //id = Shared.APIs.IUsers.AddScheduledService(Shared.UserId, scheduledService);
                            Shared.APIs.IRestaurantBusiness.PlaceCustomOrder(Shared.RestaurantLocation, new CustomRestaurantOrderRequest
                            {
                                CustomerEmail = "tedlin01@gmail.com",
                                CustomerFirstName = "Ted",
                                CustomerLastName = "Lindström",
                                CustomerPhoneNumber = "555-555-1212",
                                WrappedServiceRequest = scheduledService
                            });
                        }
                        catch (Exception ex)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                var message = string.IsNullOrEmpty(ex.Message) ? AppResources.SomethingWentWrong : ex.Message;
                                await Utils.ShowErrorMessage(message, 5);
                            });
                        }
                    }).ContinueWith(t =>
                    {
                        if (id > 0)
                        {
                            var notificator = DependencyService.Get<IToastNotificator>();
                            notificator.Notify(ToastNotificationType.Success, AppResources.PlaceOrder, AppResources.YourOrderSuccessfully, TimeSpan.FromSeconds(2));
                            var pages = Navigation.NavigationStack.Reverse().Skip(1).ToList();
                            Navigation.PopAsync(true).ConfigureAwait(false);
                        }
                        else
                        {
                            IsPauseTrigger = false;
                        }
                        this.ButtonCheckout.Text = AppResources.PlaceOrder;
                        IsBusy = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                    IsBusy = false;
            };

            SubPrice = 0;
            foreach (var menuGroupModifierItemSelected in listMenuGroupModifierItemSelected)
            {
                menuGroupModifierItemSelected.UpdatePrice();
                SubPrice += menuGroupModifierItemSelected.Price;
            }
            LabelSubtotal.Text = "$" + SubPrice.ToString("0.00");

            calculateTax();

            PickerTips.SelectedIndexChanged += (sender, e) =>
            {
                if (PickerTips.SelectedIndex >= 0)
                {
                    if (PickerTips.Items[PickerTips.SelectedIndex] == AppResources.Other)
                    {
                        EntryServiceTips.IsEnabled = true;
                        this.EntryServiceTips.NeedShowKeyboard = true;
                        Tips = 0;
                        decimal.TryParse(this.EntryServiceTips.Text, out Tips);
                    }
                    else
                    {
                        decimal percent = 0;
                        decimal.TryParse(PickerTips.Items[PickerTips.SelectedIndex].TrimEnd(new char[] { '%', ' ' }), out percent);
                        Tips = SubPrice * percent / 100;
                        EntryServiceTips.Text = Math.Round(Tips, 2).ToString("0.00");
                        EntryServiceTips.IsEnabled = false;
                        //decimal.TryParse(this.EntryServiceTips.Text, out Tips);
                    }
                }

            };
            PickerTips.SelectedIndex = 0;

            EntryServiceTips.TextChanged += (sender, e) =>
            {
                if (PickerTips.SelectedIndex >= 0)
                {
                    if (PickerTips.Items[PickerTips.SelectedIndex] == AppResources.Other)
                    {
                        EntryServiceTips.IsEnabled = true;
                        this.EntryServiceTips.NeedShowKeyboard = true;
                        Tips = 0;
                        decimal.TryParse(this.EntryServiceTips.Text, out Tips);
                    }
                    else
                    {
                        decimal percent = 0;
                        decimal.TryParse(PickerTips.Items[PickerTips.SelectedIndex].TrimEnd(new char[] { '%', ' ' }), out percent);
                        Tips = SubPrice * percent / 100;
                        EntryServiceTips.Text = Math.Round(Tips, 2).ToString("0.00");
                        EntryServiceTips.IsEnabled = false;
                    }
                }
                UpdatePrice();
            };

            ButtonCheckout.Text = AppResources.PlaceOrder;
            LabelTotal.Text = AppResources.Calculating;
        }

       

        public void LoadCard()
        {
            UserFacade userFacade = new UserFacade();
            Device.BeginInvokeOnMainThread(() =>
            {
                this.IsBusy = true;
                Task.Run(() =>
                {
                    Utils.IReloadPageCurrent = this;
                    try
                    {
                        UserModel = Shared.APIs.IUsers.GetCurrentUser();
                        var zipCode = mServiceAddress.BasicAddress.ZipCode;
                        if (Shared.IsLoggedIn)
                        {
                            Services = Shared.APIs.IServices.GetAvailableServicesForUser(Shared.UserId, null, null, zipCode);
                        }
                        else
                        {
                            Services = Shared.APIs.IServices.GetAvailableServices(zipCode);
                        }
                        if (Services != null)
                        {
                            var serviceKind = ColonyConcierge.APIData.Data.ServiceKindCodes.Restaurant_Delivery;
                            if (RestaurantDetailPage.GroupedDeliveryDestination != null)
                            {
                                if (RestaurantDetailPage.GroupedService != null)
                                {
                                    Services = Services.Where(s => s.ServiceType == ColonyConcierge.APIData.Data.ServiceTypes.Restaurant)
                                                       .Where(s => s.ServiceKind == ServiceKindCodes.Restaurant_GroupedDelivery)
                                                       .Where(s => s.DisplayName == RestaurantDetailPage.GroupedService.DisplayName)
                                                         .ToList();
                                }
                                else
                                {
                                    Services = Services.Where(s => s.ServiceType == ColonyConcierge.APIData.Data.ServiceTypes.Restaurant)
                                                               .Where(s => s.ServiceKind == ServiceKindCodes.Restaurant_GroupedDelivery)
                                                               .ToList();
                                }
                            }
                            else
                            {
                                Services = Services.Where(s => s.ServiceType == ColonyConcierge.APIData.Data.ServiceTypes.Restaurant)
                                                     .Where(s => s.ServiceKind == serviceKind)
                                                     .ToList();
                            }
                            if (Services.FirstOrDefault() != null)
                            {
                                var service = Shared.APIs.IServices.GetService(Services.FirstOrDefault().ID);
                                if (service != null)
                                {
                                    Services.FirstOrDefault().AllowedTipRates = service.AllowedTipRates;
                                }
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
                    };
                }).ContinueWith((t) =>
                {
                    if (Services != null)
                    {
                        ServiceFee = Services.SelectMany(s => s.Fees).Sum(s => s.Amount);
                        LabelServiceFee.Text = "$" + Math.Round(ServiceFee, 2).ToString("0.00");

                        PickerTips.Items.Clear();
                        var service = Services.FirstOrDefault();
                        if (service != null
                            && service.AllowedTipRates != null
                            && service.AllowedTipRates.Count > 0)
                        {
                            PickerTips.IsVisible = true;
                            var allowedTipRates = service.AllowedTipRates.OrderByDescending((arg) => arg).ToList();
                            foreach (var tip in allowedTipRates)
                            {
                                if (tip == 0)
                                {
                                    PickerTips.Items.Add(AppResources.Other);
                                }
                                else
                                {
                                    PickerTips.Items.Add(Math.Round(tip * 100, 2).ToString("0.00") + "%");
                                }
                            }
                            if (PickerTips.Items.Count > 0)
                            {
                                PickerTips.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            PickerTips.IsVisible = false;
                            EntryServiceTips.IsEnabled = false;
                        }

                        UpdatePrice();
                    }

                    this.IsBusy = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            });
        }

        private void calculateTax()
        {
            var taxRate = RestaurantDetailPage.RestaurantVM.Location.TaxRate;
            SalesTax = Math.Round(taxRate * SubPrice, 2);
            LabelSalesTax.Text = "$" + Math.Round(SalesTax, 2).ToString("0.00");
            LabelSalesTaxName.Text = AppResources.SalesTax + " (" + Math.Round(taxRate * 100, 2).ToString("0.00") + "%)";
        }
        public void UpdatePrice()
        {
            TotalPrice = 0;

            TotalPrice += SubPrice;
            TotalPrice += SalesTax;
            TotalPrice += ServiceFee;
            TotalPrice += Tips;
            TotalPrice -= Discount;

            ButtonCheckout.Text = AppResources.PlaceOrder + " $" + Math.Round(TotalPrice, 2);
            ButtonCheckout.ForceLayout();

            LabelTotal.Text = "$" + Math.Round(TotalPrice, 2).ToString("0.00");
        }

        bool IsAppearing = false;
        bool timeSlotTrigger = false;
        bool timeSlotTriggerLocked = false;
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            IsAppearing = false;
            timeSlotTrigger = false;
        }

        public override void ReloadPage()
        {
            base.ReloadPage();
            LoadCard();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsAppearing = true;

            if (mFirstLoad)
            {
                mFirstLoad = false;
                LoadCard();
            }

            if (!this.IsBusy)
            {
                UpdatePrice();
            }

            if (!timeSlotTrigger)
            {
                timeSlotTrigger = true;
                Device.StartTimer(new TimeSpan(0, 0, 0, 2), () =>
                {
                    if (!IsPauseTrigger)
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
            if (!this.IsBusy && !this.IsErrorPage)
            {
                var currentDate = RestaurantDetailPage.GetDateTimeRestaurantFromUtc(DateTime.UtcNow);
                var selectedDate = RestaurantDetailPage.SelectedDate;
                if (currentDate.Date > RestaurantDetailPage.SelectedDate)
                {
                    CloseOrder();
                }
                else
                {
                    RMenuOrderingAvailableSlot menuOrderingAvailableSlot = RestaurantDetailPage.MenuOrderingAvailables
                            .Where(s => RestaurantDetailPage.GetDateTimeRestaurantFromLocal(s.CutoffTime.Time).Date == selectedDate.Date)
                            .Where(s => RestaurantDetailPage.GetDateTimeRestaurantFromLocal(s.CutoffTime.Time) > currentDate)
                            .FirstOrDefault();
                    if (menuOrderingAvailableSlot != null)
                    {
                        var minDate = RestaurantDetailPage.GetDateTimeRestaurantFromLocal(menuOrderingAvailableSlot.StartTime.Time);
                        if (minDate > RestaurantDetailPage.SelectedDate)
                        {
                            RestaurantDetailPage.SelectedDate = minDate;
                            if (CheckCurrentPage())
                            {
                                Utils.ShowSuccessMessage((RestaurantDetailPage.IsDeliverySelected ? AppResources.Delivery : AppResources.Pickup)
                                                         + " " + AppResources.TimeChanged + " " + RestaurantDetailPage.SelectedDate.ToString("h:mm tt") + ".", AppResources.Restaurant, 5);
                            }
                        }
                    }
                    else
                    {
                        CloseOrder();
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
                        break;
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
}
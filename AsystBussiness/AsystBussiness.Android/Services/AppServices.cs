using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsystBussiness.Droid.Services;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AsystBussiness.Facades;
using AsystBussiness.Core;
using Plugin.Settings;
using Android.Telephony;
using Java.Util;
using Android.Content.PM;
using Android;
using Android.Support.V4.App;
using ColonyConcierge.APIData.Data;
using CreditCardValidator;
using Com.Braintreepayments.Api;
using Com.Braintreepayments.Api.Models;
using Com.Braintreepayments.Cardform.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(AppServices))]
namespace AsystBussiness.Droid.Services
{
    public class AppServices : AppServicesBase
    {
        public readonly int RequestLocationPermissionsCode = 10090;

        public static MainActivity MainActivityInstance;
        public Context Context { get; set; }

        public List<Action<bool>> Actions = new List<Action<bool>>();
        public List<Action<bool>> LocationRequestActions = new List<Action<bool>>();

        public MainActivity MainActivity
        {
            get
            {
                return MainActivityInstance;
            }
        }

        public override double LastLatitude
        {
            get;
            set;
        }

        public override double LastLongitude
        {
            get;
            set;
        }

        public override string UniqueDeviceId
        {
            get
            {
                try
                {
                    var uniqueDeviceId = CrossSettings.Current.GetValueOrDefault("UniqueDeviceId", string.Empty);
                    if (string.IsNullOrEmpty(uniqueDeviceId))
                    {
                        var telephonyDeviceID = string.Empty;
                        var telephonySIMSerialNumber = string.Empty;
                        TelephonyManager telephonyManager = (TelephonyManager)Context.GetSystemService(Context.TelephonyService);
                        if (telephonyManager != null)
                        {
                            if (!string.IsNullOrEmpty(telephonyManager.DeviceId) && !string.IsNullOrEmpty(telephonyManager.DeviceId.Replace("0", "")))
                            {
                                telephonyDeviceID = telephonyManager.DeviceId;
                            }
                            else if (!string.IsNullOrEmpty(telephonyManager.SimSerialNumber))
                            {
                                telephonySIMSerialNumber = telephonyManager.SimSerialNumber;
                            }
                        }

                        var androidID = Android.Provider.Settings.Secure.GetString(Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                        var deviceId = ((long)telephonyDeviceID.GetHashCode() << 32) | (long)telephonySIMSerialNumber.GetHashCode();

                        using (var deviceUuid = new UUID(androidID.GetHashCode(), deviceId))
                        {
                            uniqueDeviceId = deviceUuid.ToString();
                        }

                        CrossSettings.Current.AddOrUpdateValue("UniqueDeviceId", uniqueDeviceId);
                    }
                    return uniqueDeviceId;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);

                    return string.Empty;
                }
            }
        }

        public override string AppVersion
        {
            get
            {
                try
                {
                    return Context.PackageManager.GetPackageInfo(
                        Context.PackageName, 0).VersionName;
                }
                catch (Exception e)
                {
                    return Application.Context.PackageManager.GetPackageInfo(
                        Application.Context.PackageName, PackageInfoFlags.MetaData).VersionName;

                    ExceptionHandler.Catch(e);
                }
            }
        }

        public override string GetRegistrationNotificationId()
        {
            return string.Empty;
        }

        public override bool IsCanShow
        {
            get
            {
                return MainActivity != null && !MainActivity.IsDestroyed
                                && !MainActivity.IsFinishing && !MainActivity.IsPaused;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return (System.IntPtr)new System.Random().Next();
            }
        }

        public AppServices() : this(Application.Context.ApplicationContext)
        {
        }

        public AppServices(Context context)
        {
            Context = context;
        }

        public override void Vibration(int milliseconds)
        {
            try
            {
                if (MainActivity != null && !MainActivity.IsDestroyed)
                {
                    Vibrator v = (Vibrator)this.MainActivity.Application.GetSystemService(Context.VibratorService);
                    v.Vibrate(milliseconds);
                }
            }
            catch (System.Exception)
            {
                //not support Vibration
            }
        }

        public override void CheckLocationPermission(Action<bool> locationRequestAction)
        {
            try
            {
                LocationRequestActions.Add(locationRequestAction);

                //var fineLocation = ContextCompat.CheckSelfPermission(MainActivity, Manifest.Permission.AccessFineLocation);
                //var coarseLocation = ContextCompat.CheckSelfPermission(MainActivity, Manifest.Permission.AccessCoarseLocation);
                List<string> permissions = new List<string>();

                //if (coarseLocation == Android.Content.PM.Permission.Denied)
                {
                    permissions.Add(Manifest.Permission.AccessCoarseLocation);
                }
                //if (fineLocation == Android.Content.PM.Permission.Denied)
                {
                    permissions.Add(Manifest.Permission.AccessFineLocation);
                }

                //if (permissions.Count > 0)
                {
                    ActivityCompat.RequestPermissions(MainActivity, permissions.ToArray(), RequestLocationPermissionsCode);
                }
                //else 
                //{
                //	InvokeLocationRequestActions(true);
                //}
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public void InvokeLocationRequestActions(bool isPermissions)
        {
            try
            {
                var actions = LocationRequestActions.ToList();
                LocationRequestActions.Clear();
                foreach (var action in actions)
                {
                    if (action != null)
                    {
                        action.Invoke(isPermissions);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public override void GetLocation(Action<bool> action, bool isRefresh = false)
        {
            try
            {
                Actions.Add(action);
                if (!isRefresh && (!LastLatitude.Equals(0) || !LastLongitude.Equals(0)))
                {
                    RaiseAppOnLocationChanged(true);
                }
                else
                {
                    MainActivity.GetLocation();
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public override void RaiseAppOnLocationChanged(bool isLocation)
        {
            try
            {
                var actions = Actions.ToList();
                Actions.Clear();
                foreach (var action in actions)
                {
                    if (action != null)
                    {
                        action.Invoke(isLocation);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }
    
        public override void GetNoncenBrainTree(BrainTreeCard brainTreeCard, Action<object> action)
        {
            try
            {
                BraintreeFragment mBraintreeFragment = BraintreeFragment.NewInstance(MainActivity, brainTreeCard.Token);
                PaymentMethodNonceListener paymentMethodNonceListener = new PaymentMethodNonceListener();
                paymentMethodNonceListener.Callback += (sender, e) =>
                {
                    if (e is PaymentMethodNonce)
                    {
                        string nouce = (e as PaymentMethodNonce).Nonce;
                        action(nouce);
                    }
                    else
                    {
                        if (action != null)
                        {
                            action(e);
                        }
                    }
                };

                mBraintreeFragment.AddListener(paymentMethodNonceListener);
                var cardBuilder = new CardBuilder()
                    .CardNumber(brainTreeCard.CardNumber)
                    .Cvv(brainTreeCard.Cvv)
                    .PostalCode(brainTreeCard.PostalCode)
                    .ExpirationDate(brainTreeCard.ExpirationDate.ToString("MM/yyyy"));
                cardBuilder.Validate(false);
                Card.Tokenize(mBraintreeFragment, cardBuilder);
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public override CreditCardType GetCreditCardType(string cardNumber)
        {
            try
            {
                var cardType = CardType.ForCardNumber(cardNumber);
                if (cardType == null || cardType == CardType.Unknown)
                {
                    if (!string.IsNullOrEmpty(cardNumber))
                    {
                        try
                        {
                            var creditCardBrand = cardNumber.CreditCardBrand();
                            if (creditCardBrand == CardIssuer.AmericanExpress)
                            {
                                return CreditCardType.Amex;
                            }
                            if (creditCardBrand == CardIssuer.DinersClub)
                            {
                                return CreditCardType.DinersClub;
                            }
                            if (creditCardBrand == CardIssuer.Discover)
                            {
                                return CreditCardType.Discover;
                            }
                            if (creditCardBrand == CardIssuer.JCB)
                            {
                                return CreditCardType.Jcb;
                            }
                            if (creditCardBrand == CardIssuer.Maestro)
                            {
                                return CreditCardType.Maestro;
                            }
                            if (creditCardBrand == CardIssuer.MasterCard)
                            {
                                return CreditCardType.Mastercard;
                            }
                            if (creditCardBrand == CardIssuer.Visa)
                            {
                                return CreditCardType.Visa;
                            }
                        }
                        catch (System.Exception)
                        {
                        }
                    }
                    return CreditCardType.Unknown;
                }
                if (cardType == CardType.Amex)
                {
                    return CreditCardType.Amex;
                }
                if (cardType == CardType.DinersClub)
                {
                    return CreditCardType.Amex;
                }
                if (cardType == CardType.Discover)
                {
                    return CreditCardType.Discover;
                }
                if (cardType == CardType.Jcb)
                {
                    return CreditCardType.Jcb;
                }
                if (cardType == CardType.Maestro)
                {
                    return CreditCardType.Maestro;
                }
                if (cardType == CardType.Mastercard)
                {
                    return CreditCardType.Mastercard;
                }
                if (cardType == CardType.UnionPay)
                {
                    return CreditCardType.UnionPay;
                }
                if (cardType == CardType.Visa)
                {
                    return CreditCardType.Visa;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
            return CreditCardType.Unknown;
        }

        public override TimeZoneInfo GetTimeZoneById(string id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        public override void SetNetworkBar(bool isVisible)
        {
            //nothing for Android
        }

        public override void SetShowStatus(bool isVisible)
        {
            try
            {
                if (MainActivity != null && !MainActivity.IsDestroyed)
                {
                    if (isVisible)
                    {
                        MainActivity.Window.ClearFlags(WindowManagerFlags.Fullscreen);
                    }
                    else
                    {
                        MainActivity.Window.AddFlags(WindowManagerFlags.Fullscreen);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public class DatabaseListener : Java.Lang.Object, Android.Gms.Tasks.IOnFailureListener, Android.Gms.Tasks.IOnSuccessListener
        {
            public Action<Java.Lang.Exception> ErrorAction
            {
                get;
                set;
            }

            public Action<Java.Lang.Object> SuccessAction
            {
                get;
                set;
            }

            public void OnFailure(Java.Lang.Exception e)
            {
                if (ErrorAction != null)
                {
                    ErrorAction(e);
                }
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                if (SuccessAction != null)
                {
                    SuccessAction(result);
                }
            }
        }

        public class PaymentMethodNonceListener : Java.Lang.Object,
                                        Com.Braintreepayments.Api.Interfaces.IPaymentMethodNonceCreatedListener,
                                        Com.Braintreepayments.Api.Interfaces.IBraintreeCancelListener,
                                        //Com.Braintreepayments.Api.Interfaces.IPaymentMethodNonceCallback,
                                        Com.Braintreepayments.Api.Interfaces.IBraintreeErrorListener
        {
            public event EventHandler<object> Callback;

            public void OnCancel(int p0)
            {
                try
                {
                    if (Callback != null)
                    {
                        Callback(this, p0);
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }

            public void OnError(Java.Lang.Exception p0)
            {
                try
                {
                    if (Callback != null)
                    {
                        Callback(this, p0);
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }

            public void OnPaymentMethodNonceCreated(PaymentMethodNonce p0)
            {
                try
                {
                    if (Callback != null)
                    {
                        Callback(this, p0);
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }
    }
}
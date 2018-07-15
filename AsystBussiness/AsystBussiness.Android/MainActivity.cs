using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using ColonyConcierge.Client.Droid;
using System.Net;
using ColonyConcierge.Client;
using System.Reflection;
using Xamarin.Forms;
using AsystBussiness.Core;
using AsystBussiness.Localization.Resx;
using System.Globalization;
using Plugin.Toasts;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using ColonyConcierge.APIData.Data.Logistics.NotificationData;
using AsystBussiness.Droid.Services;
using AsystBussiness.Facades;
using AsystBussiness.Core;
using Newtonsoft.Json;
using Android.Gms.Location;
using Android.Content;
using Android.Locations;
using ZXing.Mobile;
using FFImageLoading;
using Android.Support.V4.App;
using System.Collections.Generic;
using Android;

namespace AsystBussiness.Droid
{
    [Activity(Label = "AsystBussiness", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        private static bool IsGoogleServicesAvailability = false;
        public static readonly int InstallGooglePlayServicesId = 1000;
        private ErrorDialogFragment ErrorDialogFragment;
        AppServices appServices;
        private bool mRequestLocationPermission = false;
        LocationRequest mLocationRequest;
        protected const int REQUEST_CHECK_SETTINGS = 0x1;

        public readonly int RequestPermissionsCode = 10099;
        public static bool IsPermissionRequested = false;
        public LogisticsNotification LogisticsNotification { get; set; }
        public bool IsPaused { get; set; }
        public bool IsNeedRefresh { get; set; } = false;

        private GoogleApiClient mGoogleApiClient;
        public GoogleApiClient GoogleApiClient
        {
            get
            {
                return mGoogleApiClient;
            }
        }

        private Location mLastCurrentLocation;
        public Location LastCurrentLocation
        {
            get
            {
                return mLastCurrentLocation;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            ConfigureCultureInfo();

            Platform.Init();
            Appearance.Init();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            if (Intent != null && Intent.Extras != null && Intent.Extras.ContainsKey("logistics"))
            {
                try
                {
                    appServices = DependencyService.Get<IAppServices>() as AppServices;
                    if (appServices != null && appServices.MainActivity != null && !appServices.MainActivity.IsDestroyed)
                    {
                        string value = string.Empty;
                        try
                        {
                            value = Intent.Extras.GetString("logistics");
                            var logisticsNotification = JsonConvert.DeserializeObject<LogisticsNotification>(value);
                            appServices.MainActivity.IsNeedRefresh = true;
                            appServices.MainActivity.LogisticsNotification = logisticsNotification;
                            this.Finish();
                            return;
                        }
                        catch (Exception e1)
                        {
                            appServices.MainActivity.Finish();
                            ExceptionHandler.Catch(e1);
                        }
                    }
                }
                catch (Exception e2)
                {
                    //App is closed, wait app init and show popup below (see enf of method)
                    ExceptionHandler.Catch(e2);
                }
            }

            StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);

            MobileBarcodeScanner.Initialize(Application);
            FFImageLoading.Forms.Droid.CachedImageRenderer.Init();

            var config = new FFImageLoading.Config.Configuration()
            {
                VerboseLogging = false,
                VerbosePerformanceLogging = false,
                VerboseMemoryCacheLogging = false,
                VerboseLoadingCancelledLogging = false,
                Logger = new CustomLogger(),
            };
            ImageService.Instance.Initialize(config);

            AppServices.MainActivityInstance = this;

            global::Xamarin.Forms.Forms.Init(this, bundle);

            Xamarin.FormsMaps.Init(this, bundle);

            DependencyService.Register<ToastNotificatorImplementation>();
            ToastNotificatorImplementation.Init(this);

            LoadApplication(new App());

            ConfigurateServicePoint();

            appServices = DependencyService.Get<IAppServices>() as AppServices;

            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(Android.Gms.Location.LocationServices.API)
                .Build();

            mLocationRequest = LocationRequest.Create()
                    .SetPriority(LocationRequest.PriorityHighAccuracy)
                    .SetSmallestDisplacement(5)
                    .SetInterval(30 * 1000)
                    .SetFastestInterval(10 * 1000);

            GetLocation(false);

            TestIfGooglePlayServicesIsInstalled();

            if (!IsPermissionRequested)
            {
                IsPermissionRequested = true;
                Handler UIHandler = new Handler();
                UIHandler.PostDelayed(() =>
                {
                    try
                    {
                        List<string> permissions = new List<string>();
                        permissions.Add(Manifest.Permission.Camera);
                        permissions.Add(Manifest.Permission.ReadPhoneState);
                        if (permissions.Count > 0)
                        {
                            ActivityCompat.RequestPermissions(this, permissions.ToArray(), RequestPermissionsCode);
                        }
                    }
                    catch (Exception e3)
                    {
                        ExceptionHandler.Catch(e3);
                    }
                }, 2000);
            }
        }

        void ConfigureConnector()
        {
            Connector.RequestCompleted += (sender, eventArgs) =>
            {
                RestSharp.IRestResponse _realResponse;
                try
                {
                    var _realResponseField = eventArgs.RawResult.GetType().GetField("_realResponse", BindingFlags.NonPublic | BindingFlags.Instance);
                    _realResponse = _realResponseField.GetValue(eventArgs.RawResult) as RestSharp.IRestResponse;
                }
                catch (Exception)
                {
                    _realResponse = null;
                }

                //var request = _realResponse.Request;

                if (_realResponse != null && _realResponse.ErrorException != null)
                {
                    var webException = _realResponse.ErrorException as System.Net.WebException;
                    if (webException != null &&
                        (webException.Status == System.Net.WebExceptionStatus.NameResolutionFailure
                        || webException.Status == System.Net.WebExceptionStatus.ConnectFailure
                        || webException.Status == System.Net.WebExceptionStatus.ReceiveFailure
                        || webException.Status == System.Net.WebExceptionStatus.Timeout))
                    {
                        if (Utils.IReloadPageCurrent == null)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Utils.ShowErrorMessage(new CustomException(AppResources.PullDataFailMessage, webException), 4);
                            });
                        }
                        else
                        {
                            Utils.IReloadPageCurrent.ShowLoadErrorPage();
                        }
                    }
                    else if (_realResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                             || _realResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError
                             || _realResponse.StatusCode == HttpStatusCode.GatewayTimeout
                             || _realResponse.StatusCode == HttpStatusCode.HttpVersionNotSupported
                             || _realResponse.StatusCode == HttpStatusCode.GatewayTimeout
                             || _realResponse.StatusCode == HttpStatusCode.NotImplemented)
                    {
                        if (Utils.IReloadPageCurrent == null)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Utils.ShowErrorMessage(new CustomException(AppResources.SomethingWentWrong, webException));
                            });
                        }
                        else
                        {
                            Utils.IReloadPageCurrent.ShowLoadErrorPage();
                        }
                    }
                    //else
                    //{
                    //	Device.BeginInvokeOnMainThread(() =>
                    //	{
                    //		Utils.ShowErrorMessage(_realResponse.ErrorException);
                    //	});
                    //}
                }
            };
        }

        void ConfigureCultureInfo()
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name, true);
                cultureInfo.NumberFormat = NumberFormatInfo.InvariantInfo;
                cultureInfo.DateTimeFormat = DateTimeFormatInfo.InvariantInfo;
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            }
            catch(Exception e)
            {
                ExceptionHandler.Catch(e);
            }           
        }

        void ConfigurateServicePoint()
        {
            ServicePointManager.MaxServicePointIdleTime = 1000;
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                try
                {
                    if (sender is System.Net.HttpWebRequest)
                    {
                        var asystyouUrl = PCLAppConfig.ConfigurationManager.AppSettings["AsystyouUrl"];
                        System.Net.HttpWebRequest httpWebRequest = sender as System.Net.HttpWebRequest;
                        if (httpWebRequest.RequestUri.OriginalString.Contains(asystyouUrl) ||
                               httpWebRequest.RequestUri.OriginalString.Contains("firebase") ||
                            httpWebRequest.RequestUri.OriginalString.Contains("googleapis"))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return true;
                }               
            };
        }

        private bool TestIfGooglePlayServicesIsInstalled()
        {
            try
            {
                int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
                if (queryResult == ConnectionResult.Success)
                {
                    return true;
                }

                if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
                {
                    Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(this, queryResult, InstallGooglePlayServicesId);
                    if (ErrorDialogFragment != null)
                    {
                        ErrorDialogFragment.Dismiss();
                    }
                    ErrorDialogFragment = ErrorDialogFragment.NewInstance(errorDialog);
                    ErrorDialogFragment.Cancelable = false;
                    ErrorDialogFragment.Show(FragmentManager, "GooglePlayServicesDialog");
                }
            }
            catch(Exception e)
            {
                ExceptionHandler.Catch(e);
            }
            
            return false;
        }

        public void ShowNotificationAlert(LogisticsNotification logisticsNotification)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await appServices.ShowNotifitionAlert(logisticsNotification);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    this.IsNeedRefresh = true;
                    this.LogisticsNotification = logisticsNotification;
                }
            });
        }

        protected override void OnResume()
        {
            try
            {
                try
                {
                    base.OnResume();
                    IsPaused = false;

                    if (!IsGoogleServicesAvailability)
                    {
                        IsGoogleServicesAvailability = TestIfGooglePlayServicesIsInstalled();
                    }

                    if (LogisticsNotification != null)
                    {
                        var logisticsNotification = LogisticsNotification;
                        LogisticsNotification = null;
                        this.ShowNotificationAlert(logisticsNotification);
                    }

                    var needsPermissionRequest = ZXing.Net.Mobile.Android.PermissionsHandler.NeedsPermissionRequest(this);
                    if (needsPermissionRequest)
                    {
                        ZXing.Net.Mobile.Android.PermissionsHandler.RequestPermissionsAsync(this);
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
            catch (Exception)
            {
                IsPaused = false;
            }
        }

        public void StartLocationUpdates()
        {
            try
            {
                LocationServices.FusedLocationApi.RequestLocationUpdates(mGoogleApiClient, mLocationRequest, this);
            }
            catch (Exception)
            {
            }
        }

        public void GetLocation(bool requestLocationPermission = true)
        {
            mRequestLocationPermission = requestLocationPermission;
            if (GoogleApiClient.IsConnected)
            {
                if (mRequestLocationPermission)
                {
                    try
                    {
                        LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder().AddLocationRequest(mLocationRequest);
                        builder.SetAlwaysShow(true);
                        var result = LocationServices.SettingsApi.CheckLocationSettings(mGoogleApiClient, builder.Build());
                        result.SetResultCallback(new Android.Gms.Common.Apis.ResultCallback<LocationSettingsResult>((locationSettingsResult) =>
                        {
                            var status = locationSettingsResult.Status;
                            var state = locationSettingsResult.LocationSettingsStates;

                            switch (status.StatusCode)
                            {
                                case LocationSettingsStatusCodes.Success:
                                    // All location settings are satisfied. The client can initialize location requests here.
                                    StartLocationUpdates();
                                    break;
                                case LocationSettingsStatusCodes.ResolutionRequired:
                                    // Location settings are not satisfied. But could be fixed by showing the user a dialog.
                                    try
                                    {
                                        // Show the dialog by calling startResolutionForResult(), and check the result in onActivityResult().
                                        status.StartResolutionForResult(this, REQUEST_CHECK_SETTINGS);
                                    }
                                    catch (IntentSender.SendIntentException)
                                    {
                                        // Ignore the error.
                                        appServices.RaiseAppOnLocationChanged(false);
                                    }
                                    break;
                                case LocationSettingsStatusCodes.SettingsChangeUnavailable:
                                    // Location settings are not satisfied. However, we have no way to fix the
                                    // settings so we won't show the dialog.
                                    appServices.RaiseAppOnLocationChanged(false);
                                    break;
                            }
                        }));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Catch(e);
                    }
                }
                else
                {
                    StartLocationUpdates();
                }
            }
            else
            {
                GoogleApiClient.Connect();
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            GetLocation(mRequestLocationPermission);
        }

        public void OnConnectionSuspended(int cause)
        {
            
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            
        }

        public void OnLocationChanged(Location location)
        {
            try
            {
                mLastCurrentLocation = location;
                appServices.LastLatitude = location.Latitude;
                appServices.LastLongitude = location.Longitude;
                appServices.RaiseAppOnLocationChanged(true);
            }
            catch(Exception e)
            {
                ExceptionHandler.Catch(e);
            } 
        }

        protected override void OnPause()
        {
            base.OnPause();
            IsPaused = true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            try
            {
                int requestPermissionsCode = appServices.RequestLocationPermissionsCode;
                if (requestCode == requestPermissionsCode)
                {
                    // If request is cancelled, the result arrays are empty.
                    string message = "";

                    for (int i = 0; i < permissions.Length; i++)
                    {
                        if (grantResults[i] != Permission.Granted)
                        {
                            if (!string.IsNullOrEmpty(message))
                            {
                                message += "\n";
                            }
                            message += (permissions[i] + " is denied!");
                        }
                    }

                    if (string.IsNullOrEmpty(message))
                    {
                        appServices.InvokeLocationRequestActions(true);
                    }
                    else
                    {
                        appServices.InvokeLocationRequestActions(false);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case REQUEST_CHECK_SETTINGS:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            StartLocationUpdates();
                            break;
                        case Result.Canceled:
                            appServices.RaiseAppOnLocationChanged(false);
                            break;
                    }
                    break;
            }
        }
    }
}


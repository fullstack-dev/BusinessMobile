using AsystBussiness.Core.Extensions;
using ColonyConcierge.APIData.Data;
using ColonyConcierge.Client;
using Newtonsoft.Json;
using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Core
{
    class Shared
    {
        public static bool IsLoggedIn
        {
            get
            {
                try
                {
                    return !string.IsNullOrEmpty(LoginToken);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return false;
                }
            }
        }

        public static string LoginToken
        {
            get
            {
                try
                {
                    return CrossSettings.Current.GetValueOrDefault("Token", string.Empty);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    CrossSettings.Current.AddOrUpdateValue("Token", value);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static string RestaurantLocation
        {
            get
            {
                try
                {
                    return CrossSettings.Current.GetValueOrDefault("RestaurantLocation", string.Empty);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    CrossSettings.Current.AddOrUpdateValue("RestaurantLocation", value);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static string NotificationToken
        {
            get
            {
                try
                {
                    return CrossSettings.Current.GetValueOrDefault("NotificationToken", string.Empty);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    CrossSettings.Current.AddOrUpdateValue("NotificationToken", value);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static int UserId
        {
            get
            {
                try
                {
                    return CrossSettings.Current.GetValueOrDefault<int>("UserId", -1);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return 0;
                }
            }
            set
            {
                try
                {
                    CrossSettings.Current.AddOrUpdateValue<int>("UserId", value);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static void AddPreviouslyUsedAddress(ExtendedAddress value)
        {
            try
            {
                if (value != null)
                {
                    var previouslyUsedAddresses = Shared.PreviouslyUsedAddresses;
                    var firstOrDefault = previouslyUsedAddresses.FirstOrDefault(t => t.BasicAddress.ToAddressLine() == value.BasicAddress.ToAddressLine());
                    if (firstOrDefault == null)
                    {
                        previouslyUsedAddresses.Insert(0, value);
                    }
                    else
                    {
                        previouslyUsedAddresses.Remove(firstOrDefault);
                        previouslyUsedAddresses.Insert(0, value);
                    }
                    Shared.PreviouslyUsedAddresses = previouslyUsedAddresses.Take(10).ToList();
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        public static List<ExtendedAddress> PreviouslyUsedAddresses
        {
            get
            {

                try
                {
                    return JsonConvert.DeserializeObject<List<ExtendedAddress>>(CrossSettings.Current.GetValueOrDefault<string>("PreviouslyUsedAddresses", "[]"));
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
                return new List<ExtendedAddress>();
            }
            set
            {
                try
                {
                    CrossSettings.Current.AddOrUpdateValue("PreviouslyUsedAddresses", JsonConvert.SerializeObject(value));
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static ExtendedAddress LocalAddress
        {
            get
            {

                try
                {
                    var address = CrossSettings.Current.GetValueOrDefault<string>("LocalAddress", null);
                    return JsonConvert.DeserializeObject<ExtendedAddress>(address);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
                return null;
            }
            set
            {
                try
                {
                    AddPreviouslyUsedAddress(value);
                    CrossSettings.Current.AddOrUpdateValue("LocalAddress", JsonConvert.SerializeObject(value));
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static bool Firstlaunch
        {
            get
            {
                try
                {
                    return CrossSettings.Current.GetValueOrDefault<bool>("firstlaunch", true);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return true;
                }
            }
            set
            {
                try
                {
                    CrossSettings.Current.AddOrUpdateValue("firstlaunch", value);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                }
            }
        }

        public static APIs APIs
        {
            get
            {
                try
                {
                    var connector = new Connector();
                    var api = new APIs(connector);
                    api.LoginToken = LoginToken;
                    return api;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Catch(e);
                    return null;
                }
            }
        }

        public static void SendRegistrationToServer(string token, string platform, string uniqueDeviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(token) && Shared.IsLoggedIn && token != Shared.NotificationToken)
                {
                    var isDeviceTokenSet = Shared.APIs.IUsers.AddDeviceToken(Shared.UserId, platform, token, uniqueDeviceId);
                    if (isDeviceTokenSet)
                    {
                        Shared.NotificationToken = token;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
                System.Diagnostics.Debug.WriteLine("SendRegistrationToServer token: " + e.Message);
            }
        }
    }
}

using AsystBussiness.Core;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Facades
{
    public class SerivesFacade
    {
        public List<Service> GetAvailableServices(ExtendedAddress extendedAddress)
        {
            if (extendedAddress == null)
            {
                return null;
            }
            var zipCode = extendedAddress.BasicAddress.ZipCode;

            if (!String.IsNullOrEmpty(zipCode))
            {
                var serviceAvailables = Shared.APIs.IServices.GetAvailableServices(zipCode);
                return serviceAvailables;
            }
            else
            {
                return new List<Service>();
            }
        }

        public string CheckAvailableServices(ExtendedAddress extendedAddress, List<Service> serviceAvailables)
        {
            if (extendedAddress == null)
            {
                return null;
            }

            var zipCode = extendedAddress.BasicAddress.ZipCode;
            string selectedZipCode = string.Empty;
            if (!String.IsNullOrEmpty(zipCode))
            {
                if (serviceAvailables == null || serviceAvailables.Count == 0)
                {
                    selectedZipCode = string.Empty;
                    extendedAddress.BasicAddress.ZipCode = string.Empty;
                }
                else
                {
                    selectedZipCode = zipCode;
                }
            }
            return selectedZipCode;
        }

        public string CheckAvailableServices(ExtendedAddress extendedAddress)
        {
            if (extendedAddress == null)
            {
                return null;
            }

            var zipCode = extendedAddress.BasicAddress.ZipCode;
            string selectedZipCode = string.Empty;
            if (!String.IsNullOrEmpty(zipCode))
            {
                var serviceAvailables = Shared.APIs.IServices.GetAvailableServices(zipCode);
                if (serviceAvailables == null || serviceAvailables.Count == 0)
                {
                    selectedZipCode = string.Empty;
                    extendedAddress.BasicAddress.ZipCode = string.Empty;
                }
                else
                {
                    selectedZipCode = zipCode;
                }
            }
            return selectedZipCode;
        }
    }
}

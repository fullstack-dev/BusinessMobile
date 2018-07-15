using AsystBussiness.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace AsystBussiness.Controls
{
    public class AddressMap : Map
    {
        public event EventHandler<Position> RegionChanged;
        public CustomPin AddressPin { get; set; }
        public CustomPin CurrentPin { get; set; }
        public Position Center { get; set; }

        public AddressMap() : base()
        {
            this.IsShowingUser = false;
        }

        public void RaiseOnRegionChanged(Position position)
        {
            try
            {
                if (RegionChanged != null)
                {
                    RegionChanged(this, position);
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }
    }

    public class CustomPin
    {
        public Pin Pin { get; set; }
    }
}

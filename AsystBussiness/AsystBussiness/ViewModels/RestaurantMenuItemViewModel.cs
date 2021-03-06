﻿using AsystBussiness.Facades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.ViewModels
{
    public class RestaurantMenuItemViewModel
    {
        public object ForceUpdateSize { get; set; }
        public int Id { get; set; }
        public int MenuId { get; set; }
        public decimal? BasePrice { get; set; }
        public string DisplayPrice
        {
            get
            {
                if (BasePrice.HasValue && BasePrice.Value > 0) return "$" + BasePrice.Value.ToString("0.00");
                return string.Empty;
            }
        }

        public string DisplayName { get; set; }
        public string DetailedDescription { get; set; }
        public MenuItemVM MenuItemVM { get; set; }

        public RestaurantMenuItemViewModel()
        {
        }
    }
}

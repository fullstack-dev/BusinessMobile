﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Pages
{
    public interface IReloadPage
    {
        bool IsErrorPage { get; set; }

        void ReloadPage();
        void ShowLoadErrorPage();
    }
}

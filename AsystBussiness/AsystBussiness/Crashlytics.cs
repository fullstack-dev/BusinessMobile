using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness
{
    public static class Crashlytics
    {
        public static void Log(Exception e)
        {
            Debug.WriteLine(e.Message);

            if (Debugger.IsAttached)
                Debugger.Break();
            
        }
    }
}

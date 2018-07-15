using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Core
{
    public class ExceptionHandler
    {
        public static void Catch(Exception ex)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}

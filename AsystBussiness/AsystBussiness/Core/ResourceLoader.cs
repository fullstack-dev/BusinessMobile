using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Core
{
    class ResourceLoader
    {
        public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName)
        {
            try
            {
                var resourceNames = assembly.GetManifestResourceNames();

                var resourcePaths = resourceNames
                    .Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
                    .ToArray();

                if (!resourcePaths.Any())
                {
                    throw new Exception(string.Format("Resource ending with {0} not found.", resourceFileName));
                }

                if (resourcePaths.Count() > 1)
                {
                    throw new Exception(string.Format("Multiple resources ending with {0} found: {1}{2}", resourceFileName, Environment.NewLine, string.Join(Environment.NewLine, resourcePaths)));
                }

                return assembly.GetManifestResourceStream(resourcePaths.Single());
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
                return null;               
            }
        }
    }
}

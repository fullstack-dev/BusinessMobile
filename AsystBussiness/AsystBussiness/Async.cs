using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness
{
    public static class Async
    {
        static Dictionary<string, Guid> operations = new Dictionary<string, Guid>();
        static List<string> lockedOperations = new List<string>();

        public static Guid GenerateGUID([CallerMemberName] string method = "",
            [CallerFilePath] string file = "")
        {
            try
            {
                string id = method + file.ToString();
                if (operations.ContainsKey(id))
                    operations.Remove(id);

                operations.Add(id, Guid.NewGuid());
                return operations[id];
            }
            catch (Exception e)
            {
                Crashlytics.Log(e);
            }

            return Guid.Empty;
        }

        public static void CancelMethod(string methodName, [CallerFilePath] string file = "")
        {
            try
            {
                string id = methodName + file.ToString();
                if (operations.ContainsKey(id))
                    operations.Remove(id);
            }
            catch (Exception e)
            {
                Crashlytics.Log(e);
            }
        }

        public static bool CompareGUID(Guid guid, [CallerMemberName] string method = "",
            [CallerFilePath] string file = "")
        {
            try
            {
                string id = method + file.ToString();
                return operations.ContainsKey(id) && operations[id] == guid;
            }
            catch (Exception e)
            {
                Crashlytics.Log(e);
            }

            return false;
        }

        public static bool LockMethod([CallerMemberName] string method = "",
            [CallerFilePath] string file = "")
        {
            try
            {
                string id = method + file.ToString();
                if (lockedOperations.Contains(id))
                    return false;

                lockedOperations.Add(id);
                return true;
            }
            catch (Exception e)
            {
                Crashlytics.Log(e);
            }

            return false;
        }

        public static void UnlockMethod([CallerMemberName] string method = "",
            [CallerFilePath] string file = "")
        {
            try
            {
                string id = method + file.ToString();

                lockedOperations.Remove(id);
            }
            catch (Exception e)
            {
                Crashlytics.Log(e);
            }
        }
    }
}

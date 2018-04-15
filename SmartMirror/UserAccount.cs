using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartMirror
{
    class UserAccount
    {
        public UserAccount() {}

        public static Object getSetting(string setting)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values[setting] != null)
            {
                return localSettings.Values[setting];
            }
            return null;
        }

        public static Object saveSetting(string setting, Object value)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[setting] = value;
            return value;
        }
    }
}

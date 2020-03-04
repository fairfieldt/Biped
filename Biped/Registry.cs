using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biped
{
    class SettingsStorage
    {
        private const string userRoot = "HKEY_CURRENT_USER";
        private const string subkey = "biped";
        private const string keyName = userRoot + "\\" + subkey;


        public void SaveToRegistry(string key, uint value)
        {

            // An int value can be stored without specifying the
            // registry data type, but long values will be stored
            // as strings unless you specify the type. Note that
            // the int is stored in the default name/value
            // pair.
            Registry.SetValue(keyName, key, value);
        }

        public uint GetFromRegistry(string key)
        {
            var value =  Registry.GetValue(keyName, key, 0);
            if (value != null)
            {
                return Convert.ToUInt32(value);
            }
            return 0;

        }
    }
}

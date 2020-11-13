using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public static class LanguageHandler
    {
        private static CultureInfo resourceCulture;
        private static ResourceManager resManager;

        public static void Init()
        {
            string culture = ConfigUtilities.GetLanguage();
            resourceCulture = CultureInfo.GetCultureInfo(culture);
            resManager = new ResourceManager("RevitInventorExchange.Resources.Language", Assembly.GetExecutingAssembly());
        }

        public static string GetString(string key)
        {
            string translatedValue = resManager.GetString(key, resourceCulture);
            return translatedValue;
        }
    }
}

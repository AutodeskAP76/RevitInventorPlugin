using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace RevitInventorExchange
{
    public class RevitInventorExchangeApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string starterTab = "Offsite";

            //Create Tab
            //----------
            application.CreateRibbonTab(starterTab);
            
            RibbonPanel panel = application.CreateRibbonPanel(starterTab, "Offsite panel");

            PushButton buttonSelFromView = panel.AddItem(new PushButtonData("CP_Offsite_SelectFromView", "Select from View", assemblyPath, "RevitInventorExchange.RevitInventorExchangeCommandFromView")) as PushButton;

            //Uri uriImage = new Uri($"pack://application:,,,/RevitInventorPlugin;component/Resources/Offsite_DH.png", UriKind.Absolute);
            Uri uriImage = new Uri("/Resources/Offsite_DH.png", UriKind.Relative);
            BitmapImage image = new BitmapImage(uriImage);
            buttonSelFromView.LargeImage = image;
            buttonSelFromView.ToolTip = "Select from view";


            PushButton buttonSelFromFilter = panel.AddItem(new PushButtonData("CP_Offsite_SelectFromFilter", "Select from Filter", assemblyPath, "RevitInventorExchange.RevitInventorExchangeCommandFromFilter")) as PushButton;

            //Uri uriImage = new Uri($"pack://application:,,,/RevitInventorPlugin;component/Resources/Offsite_DH.png", UriKind.Absolute);
            uriImage = new Uri("/Resources/Offsite_DH.png", UriKind.Relative);
            image = new BitmapImage(uriImage);
            buttonSelFromView.LargeImage = image;
            buttonSelFromView.ToolTip = "Select from Filter";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

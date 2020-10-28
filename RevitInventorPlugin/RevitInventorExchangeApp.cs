using Autodesk.Revit.UI;
using System;
using System.Reflection;
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

            PushButton buttonSelFromView = panel.AddItem(new PushButtonData("CP_Offsite_SelectFromView", "Selection \r\n from View", assemblyPath, "RevitInventorExchange.RevitInventorExchangeCommandFromView")) as PushButton;

            Uri uriImage = new Uri($"pack://application:,,,/RevitInventorPlugin;component/Resources/Offsite_DH.png", UriKind.Absolute);
            BitmapImage image = new BitmapImage(uriImage);
            buttonSelFromView.LargeImage = image;
            buttonSelFromView.ToolTip = "Select from view";


            PushButton buttonSelFromFilter = panel.AddItem(new PushButtonData("CP_Offsite_SelectFromFilter", "Selection \r\n from Filter", assemblyPath, "RevitInventorExchange.RevitInventorExchangeCommandFromFilter")) as PushButton;

            Uri uriImage1 = new Uri($"pack://application:,,,/RevitInventorPlugin;component/Resources/Offsite_DH_Filter.png", UriKind.Absolute);
            BitmapImage image1 = new BitmapImage(uriImage1);
            buttonSelFromFilter.LargeImage = image1;
            buttonSelFromFilter.ToolTip = "Select from Filter";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

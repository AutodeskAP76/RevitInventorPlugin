using System;
using WinForm = System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitInventorExchange.WindowsFormUI;
using RevitInventorExchange.CoreBusinessLayer;
using System.Linq.Dynamic;
using NLog.Targets;
using NLog;
using RevitInventorExchange;

namespace RevitInventorExchange
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RevitInventorExchangeCommand : IExternalCommand
    {
        RevitElementsHandler revElementHandler;
        RevitFiltersHandler revFilterHandler;
        UIApplication uiapp = null;
        Document doc = null;

        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            NLogger.Initialize();

            NLogger.LogText("Entered Execute method");

            uiapp = commandData.Application;
            doc = uiapp.ActiveUIDocument.Document;

            revElementHandler = new RevitElementsHandler(uiapp);
            revFilterHandler = new RevitFiltersHandler();
            List<Element> selectedElements = new List<Element>();

            //  Handle multi-element selection
            Selection sel = uiapp.ActiveUIDocument.Selection;

            try
            {
                NLogger.LogText("Selection performed in Revit");

                var pickedrefs = sel.PickObjects(ObjectType.Element, "Please select an element");

                foreach (var pickedref in pickedrefs)
                {
                    selectedElements.Add(doc.GetElement(pickedref));
                }

                //  Handle selected elements info extraction
                var elStructureList = revElementHandler.ProcessElements(selectedElements);
                var filteredElStrList = revFilterHandler.FilterElements(elStructureList);

                //  Pass elements info to the opened form
                //var popupWindow = new PropertiesCollectorForm(elStructureList);
                var elementWindow = new OffsiteForm(filteredElStrList);
                elementWindow.Show();

                NLogger.LogText("Exit Execute method with Success");
            }
            catch(Exception ex)
            {
                NLogger.LogError(ex);
                NLogger.LogText("Exit Execute method with Error");
            }            

            return Result.Succeeded;
        }





        //public Result Execute_LL(ExternalCommandData commandData, ref string message, ElementSet elements)
        //{
        //    uiapp = commandData.Application;
        //    doc = uiapp.ActiveUIDocument.Document;

        //    Settings documentSettings = doc.Settings;

        //    // Get all categories of current document
        //    Categories groups = documentSettings.Categories;

        //    var gg = groups.get_Item(BuiltInCategory.OST_Walls);

        //    string s = "";


        //    foreach (var cat in groups)
        //    {
        //        s = s + cat.ToString() + "\r\n";

               
        //    }

        //    TaskDialog.Show("Basic Element Info", s);

        //    return Result.Succeeded;
        //}
    }
}

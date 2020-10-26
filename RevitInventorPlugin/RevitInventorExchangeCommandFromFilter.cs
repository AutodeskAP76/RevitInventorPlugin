using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange.WindowsFormUI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RevitInventorExchange
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RevitInventorExchangeCommandFromFilter : IExternalCommand
    {
        RevitElementsHandler revElementHandler;
        RevitFiltersHandler revFilterHandler;
        UIApplication uiapp = null;
        Document doc = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            NLogger.Initialize();

            NLogger.LogText("Entered Execute method");

            uiapp = commandData.Application;
            doc = uiapp.ActiveUIDocument.Document;

            revElementHandler = new RevitElementsHandler(uiapp);
            revFilterHandler = new RevitFiltersHandler();
            IList<Element> selectedElements = new List<Element>();

            try
            {                
                var elementWindow = new OffsiteForm(null, uiapp);
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
    }
}

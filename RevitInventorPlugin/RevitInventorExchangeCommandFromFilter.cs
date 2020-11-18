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
        System.Windows.Forms.Form elementWindow = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            NLogger.Initialize();

            NLogger.LogText("Entered Execute method");

            ConfigUtilities.LoadConfig();
            LanguageHandler.Init();

            uiapp = commandData.Application;
            doc = uiapp.ActiveUIDocument.Document;

            revElementHandler = new RevitElementsHandler(uiapp);
            revFilterHandler = new RevitFiltersHandler();
            IList<Element> selectedElements = new List<Element>();

            try
            {
                var win = CheckOpened("Offsite Panel");

                if (win != null)
                {
                    //elementWindow = win;
                    //elementWindow.Show();
                    //elementWindow.Focus();

                    win.Close();
                }

                elementWindow = new OffsiteForm(selectedElements, uiapp, RevitElementSelectionMode.FromFilters);
                elementWindow.Show();

                NLogger.LogText("Exit Execute method with Success");
            }
            catch (Exception ex)
            {
                NLogger.LogError(ex);
                NLogger.LogText("Exit Execute method with Error");
            }

            return Result.Succeeded;
        }

        private System.Windows.Forms.Form CheckOpened(string name)
        {
            FormCollection fc = Application.OpenForms;

            foreach (System.Windows.Forms.Form frm in fc)
            {
                if (frm.Text == name)
                {
                    return frm;
                }
            }
            return null;
        }
    }
}

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
    public class RevitInventorExchangeCommandFromView : IExternalCommand
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

            uiapp = commandData.Application;
            doc = uiapp.ActiveUIDocument.Document;

            revElementHandler = new RevitElementsHandler(uiapp);
            revFilterHandler = new RevitFiltersHandler();
            IList<Element> selectedElements = new List<Element>();

            //  Handle multi-element selection
            Selection sel = uiapp.ActiveUIDocument.Selection;

            try
            {
                NLogger.LogText("Performing selection in Revit");

                var pickedrefs = sel.PickObjects(ObjectType.Element, "Please select an element");

                NLogger.LogText("Selection performed in Revit");

                foreach (var pickedref in pickedrefs)
                {
                    selectedElements.Add(doc.GetElement(pickedref));
                }

                //  TODO: DETERMINE IF SAME FAMILY OR NOT
                //  Handle selected elements info extraction
                var elStructureList = revElementHandler.ProcessElements(selectedElements);
                var filteredElStrList = revFilterHandler.FilterElements(elStructureList);

                //  Extract all Revit Families for selected Revit elements
                var fileredElStrRevitFamilies = Utilities.GetFamilyTypes(filteredElStrList);

                if (fileredElStrRevitFamilies.Count > 1)
                {
                    MessageBox.Show("Selected elements belong to more than one family. They have to be part of a unique family.");
                }
                else
                {
                    var win = CheckOpened("Offsite Panel");

                    if (win != null)
                    {
                        elementWindow = win;
                        elementWindow.Show();
                        elementWindow.Focus();
                    }
                    else
                    {
                        //  Pass elements info to the opened form
                        elementWindow = new OffsiteForm(selectedElements, uiapp);
                        elementWindow.Show();
                    }                    
                }

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

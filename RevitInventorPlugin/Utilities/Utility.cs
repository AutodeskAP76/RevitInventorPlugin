using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange.CoreDataStructures;

namespace RevitInventorExchange.Utilities
{
    public static class Utility
    {
        /// <summary>
        /// Helper function: return a string form of a given parameter.
        /// </summary>
        public static string ParameterToString(Document doc, Parameter param)
        {
            dynamic val = "none";

            if (param == null)
            {
                return val;
            }

            // To get to the parameter value, we need to pause it depending on its storage type
            switch (param.StorageType)
            {
                case StorageType.Double:
                    //  Extract the value and convert it from Revit internal uom to metric
                    val = param.AsValueString();
                    break;
                case StorageType.Integer:

                    if (ParameterType.YesNo == param.Definition.ParameterType)
                    {
                        if (param.AsInteger() == 0)
                        {
                            val = "False";
                        }
                        else
                        {
                            val = "True";
                        }
                    }
                    else
                    {
                        int iVal = param.AsInteger();
                        val = iVal.ToString();
                    }
                    break;
                case StorageType.String:
                    string sVal = param.AsString();
                    val = sVal;
                    break;
                case StorageType.ElementId:
                    ElementId idVal = param.AsElementId();
                    string stVal = "null";

                    if (idVal.IntegerValue >= 0)
                    {
                        stVal = doc.GetElement(idVal).Name;
                    }

                    val = stVal;

                    break;
                case StorageType.None:
                    break;
            }
            return val;
        }

        /// <summary>
        /// Returns the Desktop Connector BIM360 root path
        /// </summary>
        /// <returns></returns>
        public static string GetBIM360RootPath()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + @"\BIM 360\";
        }

        /// <summary>
        ///  Converts Revit family type string to OS compliant folder name string
        /// </summary>
        /// <param name="originalString"></param>
        /// <returns></returns>
        public static string GetStringForFolderName(string originalString)
        {
            string modifiedString = originalString.Replace(":", "____");
            return modifiedString;
        }

        public static string GetFilterOnPropFile(ElementStructure el) 
        {
            return Utility.GetStringForFolderName(GetFamilyType(el));
        }

        /// <summary>
        /// Given a Revit element, returns its family Type
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static string GetFamilyType(ElementStructure el)
        {
            return el.ElementTypeSingleParameters.SingleOrDefault(p => p.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").ParameterValue;
        }

        /// <summary>
        /// Given a a list of revit elements, returns their family Type
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static IList<string> GetFamilyTypes(IList<ElementStructure> elStruct)
        {
            return elStruct.SelectMany(p => p.ElementTypeSingleParameters.Where(l => l.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").Select( m => m.ParameterValue)).Distinct().ToList();
        }

        /// <summary>
        /// Returns all elements given a specified Family type
        /// </summary>
        /// <returns></returns>
        public static List<ElementStructure> GetElementsOnFamilyType(IList<ElementStructure> elStruct, string famType)
        {
            var filteredElements = elStruct.Where(o => o.ElementTypeSingleParameters.Any(l => l.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM" && l.ParameterValue == famType)).ToList();
            return filteredElements;
        }

        /// <summary>
        /// Returns two levels up the foldr where assembly is running
        /// </summary>
        /// <returns></returns>
        public static string GetFolderBaseline()
        {
            var assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            var TwoUp = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(assemblyFolder));

            return TwoUp;
        }

        /// <summary>
        /// Returns the BIM360 configured Project folder
        /// </summary>
        /// <returns></returns>
        public static string GetProjectFolder()
        {
            return Utility.GetBIM360RootPath() + ConfigUtilities.GetHub() + "\\" + ConfigUtilities.GetProject() + "\\";
        }

        /// <summary>
        /// Returns the BIM360 Inventor Template folder
        /// </summary>
        /// <returns></returns>
        public static string GetInventorTemplateFolder()
        {
            return Utility.GetProjectFolder() + ConfigUtilities.GetInventorTemplateFolder() + "\\";
        }

        /// <summary>
        /// Enables / disables list of controls based on DEV / Runtime environment
        /// </summary>
        /// <param name="controls"></param>
        public static void SetRuntimeBehaviour(List<System.Windows.Forms.Control> controls)
        {
            var DevEnabled = Convert.ToBoolean(ConfigUtilities.GetDevMode());            

            if (!DevEnabled)
            {
                foreach (var control in controls)
                {
                    control.Visible = false;
                }
            }
            else
            {
                foreach (var control in controls)
                {
                    control.Visible = true;
                }
            }
        }

        public static void HandleErrorInForgeResponse(string messageParam, ForgeRestResponse res)
        {
            NLogger.LogText($"Exit {messageParam} with Error");
            var ex = new Exception($"Following error occurred: Response Content: {res.ResponseContent}; Response Error: {res.RetResponse.ErrorMessage}");

            NLogger.LogError(ex);

            throw ex;
        }

        //public static string GetSubstringBetween(string Text, string FirstString, string LastString)
        //{
        //    string STR = Text;
        //    string STRFirst = FirstString;
        //    string STRLast = LastString;
        //    string FinalString;
        //    string TempString;

        //    int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
        //    int Pos2 = STR.IndexOf(LastString);

        //    FinalString = STR.Substring(Pos1, Pos2 - Pos1);

        //    return FinalString;
        //}

        public static string HideTokenInJson(string inputJson)
        {
            string retJson = "";

            JObject json = JObject.Parse(inputJson);

            try
            {
                if (json["arguments"]["InventorDoc"] != null)
                {
                    json["arguments"]["InventorDoc"]["Headers"]["Authorization"] = "***";
                }

                if (json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputIpt()] != null)
                {
                    json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputIpt()]["Headers"]["Authorization"] = "***";
                }

                if (json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputIam()] != null)
                {
                    json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputIam()]["Headers"]["Authorization"] = "***";
                }

                if (json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputDwg()] != null)
                {
                    json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputDwg()]["Headers"]["Authorization"] = "***";
                }

                if (json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputZip()] != null)
                {
                    json["arguments"][ConfigUtilities.GetDAWorkItemParamsOutputZip()]["Headers"]["Authorization"] = "***";
                }

                retJson = json.ToString();
            }
            catch
            {
                retJson = inputJson;
            }
           
            return retJson;
        }
    }
}

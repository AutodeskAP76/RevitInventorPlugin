using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreDataStructures
{
    /// <summary>
    /// The class contains properties and relevant information of elements selected in Revit model
    /// </summary>
    public class ElementStructure
    {
        private List<ElementOrderedParameter> elementOrderedParameters;
        private List<ElementOrderedParameter> elementTypeOrderedParameters;
        private List<ElementOrderedParameter> elementSingleParameters;
        private List<ElementOrderedParameter> elementTypeSingleParameters;

        private Element element;
        private ElementType elementType;

        public List<ElementOrderedParameter> ElementOrderedParameters { get => elementOrderedParameters; set => elementOrderedParameters = value; }
       
        public Element Element { get => element; set => element = value; }
        public ElementType ElementType { get => elementType; set => elementType = value; }
        public List<ElementOrderedParameter> ElementTypeOrderedParameters { get => elementTypeOrderedParameters; set => elementTypeOrderedParameters = value; }
        public List<ElementOrderedParameter> ElementSingleParameters { get => elementSingleParameters; set => elementSingleParameters = value; }
        public List<ElementOrderedParameter> ElementTypeSingleParameters { get => elementTypeSingleParameters; set => elementTypeSingleParameters = value; }
    }

    public class ElementOrderedParameter
    {
        private Int32 parameterId;
        private string parameterName;
        private string parameterValue;
        private bool transferParam;

        public string ParameterName { get => parameterName; set => parameterName = value; }
        public string ParameterValue { get => parameterValue; set => parameterValue = value; }
        public bool TransferParam { get => transferParam; set => transferParam = value; }
        public int ParameterId { get => parameterId; set => parameterId = value; }
    }
}

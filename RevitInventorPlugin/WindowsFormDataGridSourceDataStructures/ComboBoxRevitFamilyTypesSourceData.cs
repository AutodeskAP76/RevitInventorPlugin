using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.Data
{
    public class ComboBoxRevitFamilyTypesSourceData
    {
        private string familyTypeName;
        private Type targetType;
        private ElementId idType;
        private Nullable<BuiltInCategory> targetCategory;
        private string familyTypeInstance;

        public Type TargetType { get => targetType; set => targetType = value; }
        public ElementId IdType { get => idType; set => idType = value; }
        public Nullable<BuiltInCategory> TargetCategory { get => targetCategory; set => targetCategory = value; }
        public string FamilyTypeName { get => familyTypeName; set => familyTypeName = value; }
        public string FamilyTypeInstance { get => familyTypeInstance; set => familyTypeInstance = value; }
    }
}

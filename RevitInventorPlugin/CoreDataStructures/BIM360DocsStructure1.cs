using RevitInventorExchange.CoreBusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreDataStructures
{
    public class BIM360DocsStructure1
    {
        private List<BIM360DocsRowStructure> BIM360DataRows = null;

        public List<BIM360DocsRowStructure> BIM360DataRows1 { get => BIM360DataRows; set => BIM360DataRows = value; }
    }

    public class BIM360DocsRowStructure
    {
        private string parentId;
        private string id;
        private string name;
        private BIM360Type type;

        public string ParentId { get => parentId; set => parentId = value; }
        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public BIM360Type Type { get => type; set => type = value; }
    }


}

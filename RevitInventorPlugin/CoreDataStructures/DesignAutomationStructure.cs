using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreDataStructures
{
    /// <summary>
    /// This class contains all information about input Inventor file, and its corresponding output files, relevant for Forge APIs in the output files creation process
    /// </summary>
    public class DesignAutomationStructure
    {
        private List<DesignAutomationFileStructure> filesStructure;

        public List<DesignAutomationFileStructure> FilesStructure { get => filesStructure; set => filesStructure = value; }
    }

    public class DesignAutomationFileStructure
    {
        private string inputFilename;
        private string paramValues;
        private string inputLink;
        private List<DesignAutomationOutFileStructure> outputFileStructurelist;

        public string InputFilename { get => inputFilename; set => inputFilename = value; }
        public string InputLink { get => inputLink; set => inputLink = value; }
        public string ParamValues { get => paramValues; set => paramValues = value; }
        public List<DesignAutomationOutFileStructure> OutputFileStructurelist { get => outputFileStructurelist; set => outputFileStructurelist = value; }
    }

    public class DesignAutomationOutFileStructure
    {
        private string outFileName;
        private string outFileFolder;
        private string outFileStorageobject;

        public string OutFileName { get => outFileName; set => outFileName = value; }
        public string OutFileFolder { get => outFileFolder; set => outFileFolder = value; }
        public string OutFileStorageobject { get => outFileStorageobject; set => outFileStorageobject = value; }
    }
}

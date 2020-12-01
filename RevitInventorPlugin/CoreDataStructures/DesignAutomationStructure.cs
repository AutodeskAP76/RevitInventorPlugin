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
    
    /*  Original data structure class */
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
        private string outZipFileName;
        private string outZipFileStorageobject;

        public string OutFileName { get => outFileName; set => outFileName = value; }
        public string OutFileFolder { get => outFileFolder; set => outFileFolder = value; }
        public string OutFileStorageobject { get => outFileStorageobject; set => outFileStorageobject = value; }
        public string OutZipFileName { get => outZipFileName; set => outZipFileName = value; }
        public string OutZipFileStorageobject { get => outZipFileStorageobject; set => outZipFileStorageobject = value; }
    }

    /*  New data structure class */

    public enum OutputFileCategory
    {
        Part,
        Assembly,
        Zip,
        Drawing,
        BOM
    }


    /// This class contains all information about input Inventor file, and its corresponding output files, relevant for Forge APIs in the output files creation process
    /// </summary>
    public class DesignAutomationStructure1
    {
        private List<DesignAutomationFileStructure1> filesStructure;

        public List<DesignAutomationFileStructure1> FilesStructure { get => filesStructure; set => filesStructure = value; }
    }

    public class DesignAutomationFileStructure1
    {
        private string inputFilename;
        private string paramValues;
        private string inputLink;
        private List<DesignAutomationOutMainFileStructure1> outputFileStructurelist;

        public string InputFilename { get => inputFilename; set => inputFilename = value; }
        public string InputLink { get => inputLink; set => inputLink = value; }
        public string ParamValues { get => paramValues; set => paramValues = value; }
        public List<DesignAutomationOutMainFileStructure1> OutputFileStructurelist { get => outputFileStructurelist; set => outputFileStructurelist = value; }
    }

    public class DesignAutomationOutMainFileStructure1
    {
        private string outMainFileName;
        private List<DesignAutomationOutFileStructure1> outFileStructure;

        public string OutMainFileName { get => outMainFileName; set => outMainFileName = value; }

        public List<DesignAutomationOutFileStructure1> OutFileStructure { get => outFileStructure; set => outFileStructure = value; }        
    }

    public class DesignAutomationOutFileStructure1
    {
        private string outFileName;
        private string outFileFolder;
        private string outFileStorageobject;
        private OutputFileCategory outFileCategory;

        public string OutFileName { get => outFileName; set => outFileName = value; }
        public string OutFileFolder { get => outFileFolder; set => outFileFolder = value; }
        public string OutFileStorageobject { get => outFileStorageobject; set => outFileStorageobject = value; }
        public OutputFileCategory OutFileCategory { get => outFileCategory; set => outFileCategory = value; }
    }
}

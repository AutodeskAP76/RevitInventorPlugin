using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RevitInventorExchange
{
    public static class NLogger
    {
        private static readonly Logger logger = new LogFactory().GetCurrentClassLogger();
       
        public static void Initialize()
        {


            //var logFolder = Path.GetDirectoryName(AppExternal.UiControlledApp.ControlledApplication.RecordingJournalFilename);
            //if (string.IsNullOrEmpty(logFolder))
            //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //var logFile = Path.Combine(logFolder, "MDM_Revit.log");
            //var fileName = string.Format(logFile);
            
            var logFilePath = "${specialfolder:folder=ApplicationData}/Program/RevitInventorLogFile.txt";
            
            var fileTarget = new FileTarget
            {
                DeleteOldFileOnStartup = false,
                FileName = logFilePath,
                Layout = @"${longdate} ${level} ${message}",
                ArchiveAboveSize = 1024 * 1024, // 1MB
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 20,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                ArchiveFileName = logFilePath + ".{##}"
            };

            var bufferTarget = new BufferingTargetWrapper
            {
                BufferSize = 100,
                WrappedTarget = fileTarget
            };

            var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);

            var config = new LoggingConfiguration();
            config.AddTarget("bufferTarget", fileTarget);
            config.LoggingRules.Add(rule);

            //LogManager.Configuration = config;
            logger.Factory.Configuration = config;
        }

        public static void LogText(string text)
        {
            var stack = new StackFrame(1).GetMethod();
            var methodName = stack.Name;
            var className = stack.DeclaringType.Name;

            var fullText = ($"{className}: {methodName} - {text}");

            logger?.Info(fullText);
        }

        public static void LogText(string text, string argument)
        {
            var stack = new StackFrame(1).GetMethod();
            var methodName = stack.Name;
            var className = stack.DeclaringType.Name;

            var fullText = ($"{className}: {methodName} - {text}");

            logger.Info(fullText, argument);
        }

        public static void LogError(Exception ex)
        {
            logger?.Error(ex);          
        }
    }
}

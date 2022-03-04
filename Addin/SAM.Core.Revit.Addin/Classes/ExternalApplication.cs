using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core.Revit.Addin
{
    public class ExternalApplication : IExternalApplication
    {
        public static string TabName { get; } = "SAM";

        public static Windows.WindowHandle WindowHandle { get; } = new Windows.WindowHandle(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

        public Autodesk.Revit.UI.Result OnShutdown(UIControlledApplication uIControlledApplication)
        {
            return Autodesk.Revit.UI.Result.Succeeded;
        }

        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication uIControlledApplication)
        {
            uIControlledApplication.CreateRibbonTab(TabName);

            List<SAMExternalCommand> sAMExternalCommands = null;
            string directory = GetAssemblyDirectory();
            if(!string.IsNullOrWhiteSpace(directory) && System.IO.Directory.Exists(directory))
            {
                string[] paths = System.IO.Directory.GetFiles(directory, "*.Revit.Addin.dll");
                if(paths != null)
                {
                    sAMExternalCommands = new List<SAMExternalCommand>();

                    foreach(string path in paths)
                    {
                        Assembly assembly = Assembly.LoadFrom(path);
                        if(assembly == null)
                        {
                            continue;
                        }

                        Type[] types = assembly.GetExportedTypes();
                        if(types == null)
                        {
                            continue;
                        }

                        foreach(Type type in types)
                        {
                            if(!typeof(SAMExternalCommand).IsAssignableFrom(type) || type.IsAbstract)
                            {
                                continue;
                            }

                            SAMExternalCommand sAMExternalCommand = Activator.CreateInstance(type) as SAMExternalCommand;
                            if(sAMExternalCommand == null)
                            {
                                continue;
                            }

                            sAMExternalCommands.Add(sAMExternalCommand);
                        }
                    }
                }
            }

            if(sAMExternalCommands != null)
            {
                foreach (SAMExternalCommand sAMExternalCommand in sAMExternalCommands)
                {
                    string ribbonPanelName = sAMExternalCommand.RibbonPanelName;
                    if(string.IsNullOrWhiteSpace(ribbonPanelName))
                    {
                        ribbonPanelName = "General";
                    }

                    RibbonPanel ribbonPanel = uIControlledApplication.GetRibbonPanels(TabName)?.Find(x => x.Name == ribbonPanelName);
                    if(ribbonPanel == null)
                    {
                        ribbonPanel = uIControlledApplication.CreateRibbonPanel(TabName, ribbonPanelName);
                    }

                    sAMExternalCommand.Create(ribbonPanel);
                }
            }

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        public static string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly()?.Location;
        }

        public string GetAssemblyDirectory()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
        }
    }
}

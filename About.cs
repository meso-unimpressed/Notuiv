using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Interfaces;
using mp.pddn;
using Notui;
using VVVV.PluginInterfaces.V2;

namespace Notuiv
{
    [Startable]
    public class VersionWriter : IStartable
    {
        public static string VersionPath { get; private set; }
        public static string VvvvDir { get; private set; }

        public void Start()
        {
            var ver = typeof(VersionWriter).Assembly.GetName().Version.ToString();
            VvvvDir = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (VvvvDir != null)
            {
                VersionPath = Path.Combine(VvvvDir, "packs", "Notuiv", "version.info");
                File.WriteAllText(VersionPath, ver);
            }
            else
            {
                VersionPath = "null";
            }
        }

        public void Shutdown() { Start(); }
    }

    [PluginInfo(
        Name = "About",
        Category = "Notuiv",
        Author = "microdee"
    )]
    public class AboutNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Output("Notuiv Version")] public ISpread<string> FVer;
        [Output("Notui Version")] public ISpread<string> FNotuiVer;
        [Output("md.stdl Version")] public ISpread<string> FMdStdlVer;
        [Output("mp.pddn Version")] public ISpread<string> FMpPddnVer;

        [Output("Version File Path")] public ISpread<string> FVerPath;
        [Output("VVVV Dir")] public ISpread<string> FVDir;

        public void Evaluate(int SpreadMax) { }
        public void OnImportsSatisfied()
        {
            FVer[0] = typeof(AboutNode).Assembly.GetName().Version.ToString();
            FNotuiVer[0] = typeof(NotuiContext).Assembly.GetName().Version.ToString();
            FMdStdlVer[0] = typeof(IMainlooping).Assembly.GetName().Version.ToString();
            FMpPddnVer[0] = typeof(SpreadWrapper).Assembly.GetName().Version.ToString();

            FVDir[0] = VersionWriter.VvvvDir;
            FVerPath[0] = VersionWriter.VersionPath;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Notuiv.Factories
{
    public static class AssemblyLoader
    {
        public static readonly string[] LoadAssemblies = { "Notuiv.dll" };

        public static void InitializeAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                string assemblyFile = (args.Name.Contains(','))
                    ? args.Name.Substring(0, args.Name.IndexOf(','))
                    : args.Name;

                assemblyFile += ".dll";

                // Forbid non handled dll's
                if (!LoadAssemblies.Contains(assemblyFile))
                {
                    return null;
                }

                var directoryInfo = new FileInfo((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).Directory;
                if (directoryInfo != null)
                {
                    string absoluteFolder = directoryInfo.FullName;
                    string targetPath = Path.Combine(absoluteFolder, "..\\nodes\\plugins", assemblyFile);

                    try
                    {
                        return Assembly.LoadFile(targetPath);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return null;
            };
        }

        static AssemblyLoader()
        {
            InitializeAssembly();
        }
    }
}

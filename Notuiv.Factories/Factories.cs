using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using mp.pddn;
using Notui;
using VVVV.PluginInterfaces.V2;

namespace Notuiv.Factories
{
    public static class AssemblyLoader
    {
        public static readonly string ThisAssemblyDir = Path.GetDirectoryName(typeof(AssemblyLoader).Assembly.Location);

        public static Assembly Load(string path)
        {
            var res = Assembly.LoadFile(Path.Combine(ThisAssemblyDir, path));
            AppDomain.CurrentDomain.Load(res.GetName());
            return res;
        }
    }

    [Export(typeof(IAddonFactory))]
    [Export(typeof(MemberNodeFactory))]
    [ComVisible(false)]
    public class MemberNodeFactory : ObjectMemberNodeFactory
    {
        [ImportingConstructor()]
        public MemberNodeFactory(CompositionContainer parentContainer) : base(parentContainer)
        {
            //AssemblyLoader.Load("..\\..\\dx11\\core\\VVVV.DX11.Core.dll");
            //AssemblyLoader.Load("..\\..\\dx11\\core\\VVVV.DX11.Lib.dll");
            //AssemblyLoader.Load("..\\..\\dx11\\nodes\\plugins\\base\\VVVV.DX11.Nodes.dll");

            var notuiv = AssemblyLoader.Load("..\\nodes\\plugins\\Notuiv.dll");
            var splittertype = notuiv.GetType("Notuiv.ElementSplitterNode");

            ObjectSplitters = new List<TypeSplitter>
            {
                new TypeSplitter("Element", typeof(NotuiElement), splittertype)
            };
        }
        
        public MemberNodeFactory(CompositionContainer parentContainer, string fileExtension) : base(parentContainer, fileExtension) { }

        public override List<TypeSplitter> ObjectSplitters { get; }

        protected override bool IsFilenameValid(string filename)
        {
            return filename.Contains("plugins\\Notuiv.dll");
        }
    }
}

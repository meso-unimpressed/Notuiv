using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notui;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace Notuiv.Generic
{
    [PluginInfo(Name = "Cons",
                Category = "Notui.Context",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class NotuiContextConsNode : Cons<NotuiContext> { }

    [PluginInfo(Name = "CAR",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiContextCARBinNode : CARBin<NotuiContext> { }

    [PluginInfo(Name = "CDR",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiContextCDRBinNode : CDRBin<NotuiContext> { }

    [PluginInfo(Name = "Reverse",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "Reverses the order of the slices in the Spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiContextReverseBinNode : ReverseBin<NotuiContext> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Context",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop"
               )]
    public class NotuiContextSelectNode : Select<NotuiContext> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop",
                Author = "woei"
            )]
    public class NotuiContextSelectBinNode : SelectBin<NotuiContext> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Context",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.",
                Tags = "split, generic, spreadop"
               )]
    public class NotuiContextUnzipNode : Unzip<NotuiContext> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.",
                Tags = "split, generic, spreadop"
               )]
    public class NotuiContextUnzipBinNode : Unzip<IInStream<NotuiContext>> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Context",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class NotuiContextZipNode : Zip<NotuiContext> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class NotuiContextZipBinNode : Zip<IInStream<NotuiContext>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "Notui.Context",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class NotuiContextGetSpreadNode : GetSpreadAdvanced<NotuiContext> { }
}

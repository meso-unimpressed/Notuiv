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
                Category = "Notui.Transformation",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class ElementTransformationConsNode : Cons<ElementTransformation> { }

    [PluginInfo(Name = "CAR",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationCARBinNode : CARBin<ElementTransformation> { }

    [PluginInfo(Name = "CDR",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationCDRBinNode : CDRBin<ElementTransformation> { }

    [PluginInfo(Name = "Reverse",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Reverses the order of the slices in the Spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationReverseBinNode : ReverseBin<ElementTransformation> { }

    [PluginInfo(Name = "Shift",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Shifts the slices in the Spread by the given Phase.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationShiftBinNode : ShiftBin<ElementTransformation> { }

    [PluginInfo(Name = "SetSlice",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationSetSliceNode : SetSlice<ElementTransformation> { }

    [PluginInfo(Name = "DeleteSlice",
                Category = "Notui.Transformation",
                Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
                Tags = "remove, generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationDeleteSliceNode : DeleteSlice<ElementTransformation> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Transformation",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop"
               )]
    public class ElementTransformationSelectNode : Select<ElementTransformation> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop",
                Author = "woei"
            )]
    public class ElementTransformationSelectBinNode : SelectBin<ElementTransformation> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Transformation",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.",
                Tags = "split, generic, spreadop"
               )]
    public class ElementTransformationUnzipNode : Unzip<ElementTransformation> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.",
                Tags = "split, generic, spreadop"
               )]
    public class ElementTransformationUnzipBinNode : Unzip<IInStream<ElementTransformation>> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Transformation",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class ElementTransformationZipNode : Zip<ElementTransformation> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class ElementTransformationZipBinNode : Zip<IInStream<ElementTransformation>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class ElementTransformationGetSpreadNode : GetSpreadAdvanced<ElementTransformation> { }

    [PluginInfo(Name = "SetSpread",
                Category = "Notui.Transformation",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ElementTransformationSetSpreadNode : SetSpread<ElementTransformation> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Notui.Transformation",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class ElementTransformationSplitAtNode : SplitAtNode<ElementTransformation> { }
}

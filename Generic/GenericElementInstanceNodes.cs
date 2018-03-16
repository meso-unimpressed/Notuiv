using System;
using Notui;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace Notuiv.Generic
{
    #region SingleValue

    [PluginInfo(Name = "Cast",
                Category = "Notui.Element",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class NotuiElementCastNode : Cons<NotuiElement> { }

    #endregion SingleValue

    #region SpreadOps

    [PluginInfo(Name = "Cons",
                Category = "Notui.Element",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class NotuiElementConsNode : Cons<NotuiElement> { }

    [PluginInfo(Name = "CAR",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementCARBinNode : CARBin<NotuiElement> { }

    [PluginInfo(Name = "CDR",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementCDRBinNode : CDRBin<NotuiElement> { }

    [PluginInfo(Name = "Reverse",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Reverses the order of the slices in the Spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementReverseBinNode : ReverseBin<NotuiElement> { }

    [PluginInfo(Name = "Shift",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Shifts the slices in the Spread by the given Phase.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementShiftBinNode : ShiftBin<NotuiElement> { }

    [PluginInfo(Name = "SetSlice",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementSetSliceNode : SetSlice<NotuiElement> { }

    [PluginInfo(Name = "DeleteSlice",
                Category = "Notui.Element",
                Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
                Tags = "remove, generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementDeleteSliceNode : DeleteSlice<NotuiElement> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Element",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop"
               )]
    public class NotuiElementSelectNode : Select<NotuiElement> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop",
                Author = "woei"
            )]
    public class NotuiElementSelectBinNode : SelectBin<NotuiElement> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Element",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.",
                Tags = "split, generic, spreadop"
               )]
    public class NotuiElementUnzipNode : Unzip<NotuiElement> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.",
                Tags = "split, generic, spreadop"
               )]
    public class NotuiElementUnzipBinNode : Unzip<IInStream<NotuiElement>> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Element",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class NotuiElementZipNode : Zip<NotuiElement> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class NotuiElementZipBinNode : Zip<IInStream<NotuiElement>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class NotuiElementGetSpreadNode : GetSpreadAdvanced<NotuiElement> { }

    [PluginInfo(Name = "SetSpread",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class NotuiElementSetSpreadNode : SetSpread<NotuiElement> { }

    [PluginInfo(Name = "Pairwise",
                Category = "Notui.Element",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class NotuiElementPairwiseNode : Pairwise<NotuiElement> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Notui.Element",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class NotuiElementSplitAtNode : SplitAtNode<NotuiElement> { }

    #endregion SpreadOps
}


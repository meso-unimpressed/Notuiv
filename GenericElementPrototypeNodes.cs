using System;
using Notui;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace Notuiv
{
    #region SingleValue

    [PluginInfo(Name = "Cast",
                Category = "Notui.Element",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class ElementPrototypeCastNode : Cons<ElementPrototype> { }

    #endregion SingleValue

    #region SpreadOps

    [PluginInfo(Name = "Cons",
                Category = "Notui.Element",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class ElementPrototypeConsNode : Cons<ElementPrototype> { }

    [PluginInfo(Name = "CAR",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeCARBinNode : CARBin<ElementPrototype> { }

    [PluginInfo(Name = "CDR",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeCDRBinNode : CDRBin<ElementPrototype> { }

    [PluginInfo(Name = "Reverse",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Reverses the order of the slices in the Spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeReverseBinNode : ReverseBin<ElementPrototype> { }

    [PluginInfo(Name = "Shift",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Shifts the slices in the Spread by the given Phase.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeShiftBinNode : ShiftBin<ElementPrototype> { }

    [PluginInfo(Name = "SetSlice",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeSetSliceNode : SetSlice<ElementPrototype> { }

    [PluginInfo(Name = "DeleteSlice",
                Category = "Notui.Element",
                Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
                Tags = "remove, generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeDeleteSliceNode : DeleteSlice<ElementPrototype> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Element",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop"
               )]
    public class ElementPrototypeSelectNode : Select<ElementPrototype> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop",
                Author = "woei"
            )]
    public class ElementPrototypeSelectBinNode : SelectBin<ElementPrototype> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Element",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.",
                Tags = "split, generic, spreadop"
               )]
    public class ElementPrototypeUnzipNode : Unzip<ElementPrototype> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.",
                Tags = "split, generic, spreadop"
               )]
    public class ElementPrototypeUnzipBinNode : Unzip<IInStream<ElementPrototype>> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Element",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class ElementPrototypeZipNode : Zip<ElementPrototype> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class ElementPrototypeZipBinNode : Zip<IInStream<ElementPrototype>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class ElementPrototypeGetSpreadNode : GetSpreadAdvanced<ElementPrototype> { }

    [PluginInfo(Name = "SetSpread",
                Category = "Notui.Element",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class ElementPrototypeSetSpreadNode : SetSpread<ElementPrototype> { }

    [PluginInfo(Name = "Pairwise",
                Category = "Notui.Element",
                Help = "Returns all combinations of pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class ElementPrototypePairwiseNode : Pairwise<ElementPrototype> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Notui.Element",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class ElementPrototypeSplitAtNode : SplitAtNode<ElementPrototype> { }

    #endregion SpreadOps

    #region Collections

    public class ElementCopier : Copier<ElementPrototype>
    {
        public override ElementPrototype Copy(ElementPrototype value)
        {
            var res = new ElementPrototype(value.InstanceType, parent: value.Parent);
            var rid = res.Id;
            res.UpdateCommon(value, ApplyTransformMode.All);
            res.Id = rid;
            return res;
        }
    }

    [PluginInfo(Name = "Buffer",
        Category = "Notui.Element",
        Help = "Inserts the input at the given index.",
        Tags = "generic, spreadop, collection",
        AutoEvaluate = true
    )]
    public class ElementPrototypeBufferNode : BufferNode<ElementPrototype>
    {
        public ElementPrototypeBufferNode() : base(new ElementCopier()) { }
    }

    [PluginInfo(Name = "Queue",
                Category = "Notui.Element",
                Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class ElementPrototypeQueueNode : QueueNode<ElementPrototype>
    {
        public ElementPrototypeQueueNode() : base(new ElementCopier()) { }
    }

    [PluginInfo(Name = "RingBuffer",
                Category = "Notui.Element",
                Help = "Inserts the input at the ringbuffer position.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class ElementPrototypeRingBufferNode : RingBufferNode<ElementPrototype>
    {
        public ElementPrototypeRingBufferNode() : base(new ElementCopier()) { }
    }

    [PluginInfo(Name = "Store",
                Category = "Notui.Element",
                Help = "Stores a spread and sets/removes/inserts slices.",
                Tags = "add, insert, remove, generic, spreadop, collection",
                Author = "woei",
                AutoEvaluate = true
               )]
    public class ElementPrototypeStoreNode : Store<ElementPrototype> { }

    [PluginInfo(Name = "Stack",
                Category = "Notui.Element",
                Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
                Tags = "generic, spreadop, collection",
                Author = "vux"
                )]
    public class ElementPrototypeStackNode : StackNode<ElementPrototype> { }

    [PluginInfo(
           Name = "QueueStore",
           Category = "Notui.Element",
           Help = "Stores a series of queues",
           Tags = "append, remove, generic, spreadop, collection",
           Author = "motzi"
    )]
    public class ElementPrototypeQueueStoreNodes : QueueStore<ElementPrototype>
    {
        public ElementPrototypeQueueStoreNodes() : base(new ElementCopier()) { }
    }

    #endregion Collections

}


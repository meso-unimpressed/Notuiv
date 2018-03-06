using System;
using Notui;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace Notuiv
{
    #region SingleValue

    [PluginInfo(Name = "Cast",
                Category = "Notui.Behavior",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class InteractionBehaviorCastNode : Cons<InteractionBehavior> { }

    #endregion SingleValue

    #region SpreadOps

    [PluginInfo(Name = "Cons",
                Category = "Notui.Behavior",
                Help = "Concatenates all Input spreads.",
                Tags = "generic, spreadop"
                )]
    public class InteractionBehaviorConsNode : Cons<InteractionBehavior> { }

    [PluginInfo(Name = "CAR",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorCARBinNode : CARBin<InteractionBehavior> { }

    [PluginInfo(Name = "CDR",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorCDRBinNode : CDRBin<InteractionBehavior> { }

    [PluginInfo(Name = "Reverse",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Reverses the order of the slices in the Spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorReverseBinNode : ReverseBin<InteractionBehavior> { }

    [PluginInfo(Name = "Shift",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Shifts the slices in the Spread by the given Phase.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorShiftBinNode : ShiftBin<InteractionBehavior> { }

    [PluginInfo(Name = "SetSlice",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorSetSliceNode : SetSlice<InteractionBehavior> { }

    [PluginInfo(Name = "DeleteSlice",
                Category = "Notui.Behavior",
                Help = "Removes slices from the Spread at the positions addressed by the Index pin.",
                Tags = "remove, generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorDeleteSliceNode : DeleteSlice<InteractionBehavior> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Behavior",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop"
               )]
    public class InteractionBehaviorSelectNode : Select<InteractionBehavior> { }

    [PluginInfo(Name = "Select",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted. ",
                Tags = "repeat, resample, duplicate, spreadop",
                Author = "woei"
            )]
    public class InteractionBehaviorSelectBinNode : SelectBin<InteractionBehavior> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Behavior",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it.",
                Tags = "split, generic, spreadop"
               )]
    public class InteractionBehaviorUnzipNode : Unzip<InteractionBehavior> { }

    [PluginInfo(Name = "Unzip",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "The inverse of Zip. Interprets the Input spread as being interleaved and untangles it. With Bin Size.",
                Tags = "split, generic, spreadop"
               )]
    public class InteractionBehaviorUnzipBinNode : Unzip<IInStream<InteractionBehavior>> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Behavior",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class InteractionBehaviorZipNode : Zip<InteractionBehavior> { }

    [PluginInfo(Name = "Zip",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Interleaves all Input spreads.",
                Tags = "interleave, join, generic, spreadop"
               )]
    public class InteractionBehaviorZipBinNode : Zip<IInStream<InteractionBehavior>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class InteractionBehaviorGetSpreadNode : GetSpreadAdvanced<InteractionBehavior> { }

    [PluginInfo(Name = "SetSpread",
                Category = "Notui.Behavior",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class InteractionBehaviorSetSpreadNode : SetSpread<InteractionBehavior> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Notui.Behavior",
                Help = "Splits the Input spread in two at the specified Index.",
                Tags = "generic, spreadop"
                )]
    public class InteractionBehaviorSplitAtNode : SplitAtNode<InteractionBehavior> { }

    #endregion SpreadOps

    #region Collections
    /*
    public class BehaviorCopier : Copier<InteractionBehavior>
    {
        public override InteractionBehavior Copy(InteractionBehavior value)
        {
            return value.Copy();
        }
    }

    [PluginInfo(Name = "Buffer",
        Category = "Notui.Behavior",
        Help = "Inserts the input at the given index.",
        Tags = "generic, spreadop, collection",
        AutoEvaluate = true
    )]
    public class InteractionBehaviorBufferNode : BufferNode<InteractionBehavior>
    {
        public InteractionBehaviorBufferNode() : base(new BehaviorCopier()) { }
    }

    [PluginInfo(Name = "Queue",
                Category = "Notui.Behavior",
                Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class InteractionBehaviorQueueNode : QueueNode<InteractionBehavior>
    {
        public InteractionBehaviorQueueNode() : base(new BehaviorCopier()) { }
    }

    [PluginInfo(Name = "RingBuffer",
                Category = "Notui.Behavior",
                Help = "Inserts the input at the ringbuffer position.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class InteractionBehaviorRingBufferNode : RingBufferNode<InteractionBehavior>
    {
        public InteractionBehaviorRingBufferNode() : base(new BehaviorCopier()) { }
    }

    [PluginInfo(Name = "Store",
                Category = "Notui.Behavior",
                Help = "Stores a spread and sets/removes/inserts slices.",
                Tags = "add, insert, remove, generic, spreadop, collection",
                Author = "woei",
                AutoEvaluate = true
               )]
    public class InteractionBehaviorStoreNode : Store<InteractionBehavior> { }

    [PluginInfo(Name = "Stack",
                Category = "Notui.Behavior",
                Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
                Tags = "generic, spreadop, collection",
                Author = "vux"
                )]
    public class InteractionBehaviorStackNode : StackNode<InteractionBehavior> { }

    [PluginInfo(
           Name = "QueueStore",
           Category = "Notui.Behavior",
           Help = "Stores a series of queues",
           Tags = "append, remove, generic, spreadop, collection",
           Author = "motzi"
    )]
    public class InteractionBehaviorQueueStoreNodes : QueueStore<InteractionBehavior>
    {
        public InteractionBehaviorQueueStoreNodes() : base(new BehaviorCopier()) { }
    }
    */
    #endregion Collections

}


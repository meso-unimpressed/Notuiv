using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using md.stdl.Coding;
using md.stdl.Interaction;
using Notui;
using mp.pddn;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;

namespace Notuiv
{
    [PluginInfo(
        Name = "AttachedValues",
        Category = "Notui",
        Version = "Split",
        Author = "microdee"
    )]
    public class AttachedValuesSplitNodes : ObjectSplitNode<AttachedValues> { }

    [PluginInfo(
        Name = "IntersectionPoint",
        Category = "Notui",
        Version = "Split",
        Author = "microdee"
    )]
    public class IntersectionPointSplitNodes : ObjectSplitNode<IntersectionPoint>
    {
        [ImportingConstructor]
        public IntersectionPointSplitNodes()
        {
            UseObjectCache = false;
        }
    }

    [PluginInfo(
        Name = "PrototypeInfo",
        Category = "Notui.ElementPrototype",
        Version = "Split",
        Author = "microdee"
    )]
    public class PrototypeInfoSplitNodes : ObjectSplitNode<ElementPrototype>
    {
        public override bool StopWatchToSeconds => true;
    }

    [PluginInfo(
        Name = "MouseSplit",
        Category = "Mouse",
        Version = "Accumulated",
        Author = "microdee"
    )]
    public class AccumulatedMouseSplitNodes : ObjectSplitNode<AccumulatingMouseObserver>
    {
        public override bool StopWatchToSeconds => true;
    }

    [PluginInfo(
        Name = "ElementSplitter",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee",
        Ignore = true
    )]
    public class ElementSplitterNode : ObjectSplitNode<NotuiElement>
    {
        public override bool StopWatchToSeconds => true;
        public override bool ManualInit => true;

        [ImportingConstructor]
        public ElementSplitterNode() { }
    }

    [PluginInfo(
        Name = "InstanceInfo",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee"
    )]
    public class GuiElementInfoNode : IPluginEvaluate
    {
        [Input("Element")] public Pin<NotuiElement> ElementIn;

        [Output("Element", Order = 0)]
        public ISpread<NotuiElement> ElementOut;

        [Output(
            "Type",
            Visibility = PinVisibility.OnlyInspector,
            Order = 101
        )]
        public ISpread<string> TypeOut;

        [Output("Display Transformation Out", Order = 2)]
        public ISpread<Matrix4x4> DisplayTrOut;
        [Output("Local Display Transformation", Visibility = PinVisibility.Hidden, Order = 2)]
        public ISpread<ElementTransformation> LocDisplayOut;

        [Output("Name Out", Order = 3)]
        public ISpread<string> NameOut;
        [Output("ID", Visibility = PinVisibility.Hidden, Order = 4)]
        public ISpread<string> IdOut;
        [Output("Active Out", Visibility = PinVisibility.Hidden, Order = 5)]
        public ISpread<bool> ActiveOut;

        [Output(
            "Transparent Out",
            Visibility = PinVisibility.OnlyInspector,
            Order = 106
        )]
        public ISpread<bool> TransparentOut;
        [Output("Hit", Order = 7)]
        public ISpread<bool> HitOut;
        [Output("Touched", Order = 8)]
        public ISpread<bool> TouchedOut;
        [Output("Dying", Order = 9)]
        public ISpread<bool> DyingOut;
        [Output("Fading State", Visibility = PinVisibility.Hidden, Order = 10)]
        public ISpread<ElementFadeState> FadeStateOut;
        [Output("Fade Progress", Order = 11)]
        public ISpread<double> ElementFadeOut;

        [Output(
            "Age", 
            Order = 112,
            Visibility = PinVisibility.OnlyInspector
        )]
        public ISpread<double> AgeOut;

        [Output("Interacting Touches", Order = 13, BinOrder = 14)]
        public ISpread<ISpread<Touch>> TouchesOut;

        [Output(
            "Are Interacting Touches Hitting",
            Visibility = PinVisibility.OnlyInspector,
            BinVisibility = PinVisibility.OnlyInspector,
            Order = 115,
            BinOrder = 116
        )]
        public ISpread<ISpread<bool>> TouchesHittingOut;

        [Output(
            "Touching Intersections",
            Visibility = PinVisibility.Hidden,
            BinVisibility = PinVisibility.Hidden,
            Order = 17,
            BinOrder = 18
        )]
        public ISpread<ISpread<IntersectionPoint>> TouchingIntersectionsOut;

        [Output(
            "Hitting Touches",
            Visibility = PinVisibility.OnlyInspector,
            BinVisibility = PinVisibility.OnlyInspector,
            Order = 119,
            BinOrder = 120
        )]
        public ISpread<ISpread<Touch>> HittingTouchesOut;

        [Output(
            "Hitting Intersections",
            Visibility = PinVisibility.OnlyInspector,
            BinVisibility = PinVisibility.OnlyInspector,
            Order = 121,
            BinOrder = 122
        )]
        public ISpread<ISpread<IntersectionPoint>> HittingIntersectionsOut;

        [Output(
            "Mice",
            Visibility = PinVisibility.OnlyInspector,
            BinVisibility = PinVisibility.OnlyInspector,
            Order = 123,
            BinOrder = 124
        )]
        public ISpread<ISpread<Mouse>> MiceOut;

        [Output("Children Out", Order = 25, BinOrder = 26)]
        public ISpread<ISpread<NotuiElement>> ChildrenOut;

        [Output(
            "Behaviors Out",
            Visibility = PinVisibility.OnlyInspector,
            BinVisibility = PinVisibility.OnlyInspector,
            Order = 127,
            BinOrder = 128
        )]
        public ISpread<ISpread<InteractionBehavior>> BehavsOut;

        [Output(
            "Parent",
            Visibility = PinVisibility.Hidden,
            BinVisibility = PinVisibility.Hidden,
            Order = 29,
            BinOrder = 30
        )]
        public ISpread<ISpread<NotuiElement>> ParentOut;

        [Output("Prototype", Visibility = PinVisibility.Hidden, Order = 31)]
        public ISpread<ElementPrototype> PrototypeOut;

        [Output("Context", Visibility = PinVisibility.OnlyInspector, Order = 132)]
        public ISpread<NotuiContext> ContextOut;

        protected void AssignElementOutputs(NotuiElement element, int i)
        {
            if (element.EnvironmentObject is VEnvironmentData venvdat)
            {
                TouchesOut[i] = venvdat.Touches;
                TouchesHittingOut[i] = venvdat.TouchesHitting;
                TouchingIntersectionsOut[i] = venvdat.TouchingIntersections;
                HittingTouchesOut[i] = venvdat.HittingTouches;
                HittingIntersectionsOut[i] = venvdat.HittingIntersections;
                MiceOut[i] = venvdat.Mice;
                ChildrenOut[i] = venvdat.Children;
                BehavsOut[i] = venvdat.Behaviors;
                ParentOut[i] = venvdat.Parent;
                DisplayTrOut[i] = venvdat.VDispTr;
                TypeOut[i] = venvdat.TypeCSharpName;
            }
            ElementOut[i] = element;
            NameOut[i] = element.Name;
            PrototypeOut[i] = element.Prototype;
            
            IdOut[i] = element.Id;
            HitOut[i] = element.Hit;
            TouchedOut[i] = element.Touched;
            ActiveOut[i] = element.Active;
            TransparentOut[i] = element.Transparent;
            FadeStateOut[i] = element.FadingState;
            ElementFadeOut[i] = element.ElementFade;
            AgeOut[i] = element.Age.Elapsed.TotalSeconds;
            DyingOut[i] = element.Dying;
            LocDisplayOut[i] = element.DisplayTransformation;
            ContextOut[i] = element.Context;
        }

        protected int _prevSliceCount = -1;

        public void Evaluate(int SpreadMax)
        {
            if (ElementIn.IsConnected && ElementIn.SliceCount > 0 && ElementIn[0] != null)
            {
                if (_prevSliceCount != ElementIn.SliceCount)
                {
                    this.SetSliceCountForAllOutput(ElementIn.SliceCount);
                }
                _prevSliceCount = ElementIn.SliceCount;

                for (int i = 0; i < ElementIn.SliceCount; i++)
                {
                    AssignElementOutputs(ElementIn[i], i);
                }
            }
            else
            {
                if(_prevSliceCount != 0)
                    this.SetSliceCountForAllOutput(0);
                _prevSliceCount = 0;
            }
        }
    }
}

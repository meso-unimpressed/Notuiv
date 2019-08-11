using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Boolean;
using md.stdl.Interaction;
using Notui;
using md.stdl.Mathematics;
using mp.pddn;
using VVVV.DX11.Nodes.Renderers.Graphics.Touch;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VMatrix = VVVV.Utils.VMath.Matrix4x4;
using SMatrix = System.Numerics.Matrix4x4;


namespace Notuiv
{
    [PluginInfo(
        Name = "SubContextOptions",
        Category = "Notui",
        Author = "microdee",
        Version = "Join",
        AutoEvaluate = true
    )]
    public class SubContextOptionsNode : IPluginEvaluate
    {
        [Input("Include Hitting Touches Too")]
        public IDiffSpread<bool> FIncludeHitting;
        [Input("Touch Coordinate Source")]
        public IDiffSpread<SubContextOptions.IntersectionSpaceSelection> FTouchCoordSrc;

        [Output("Output")]
        public ISpread<SubContextOptions> FOut;

        public void Evaluate(int SpreadMax)
        {
            FOut.Stream.IsChanged = false;
            if (FIncludeHitting.IsChanged || FTouchCoordSrc.IsChanged)
            {
                FOut.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FOut[i] == null)
                        FOut[i] = new SubContextOptions();
                    FOut[i].IncludeHitting = FIncludeHitting[i];
                    FOut[i].TouchSpaceSource = FTouchCoordSrc[i];
                    FOut[i].UpdateOnlyChangeFlagged = true;
                }
                FOut.Stream.IsChanged = true;
            }
        }
    }

    [PluginInfo(
        Name = "SubContext",
        Category = "Notui",
        Author = "microdee",
        Version = "Split",
        AutoEvaluate = true
    )]
    public class SubContextNode : IPluginEvaluate, IPluginFeedbackLoop
    {
        [Import] public IPluginHost2 PluginHost;
        [Import] public IHDEHost Host;

        [Input("Hosting Element")]
        public Pin<NotuiElement> FHostElements;
        [Input("Element Prototypes")]
        public IDiffSpread<ISpread<ElementPrototype>> FElements;

        private Spread<Spread<ElementPrototype>> _prevElements = new Spread<Spread<ElementPrototype>>();

        [Input("Force Update Elements", IsBang = true, Visibility = PinVisibility.Hidden)]
        public ISpread<bool> FUpdateElements;
        [Input("Auto Update Elements", DefaultBoolean = true, Visibility = PinVisibility.Hidden)]
        public ISpread<bool> FAutoUpdateElements;

        [Input("Minimum Force for Interaction", DefaultValue = 0.5)]
        public IDiffSpread<float> FMinForce;
        [Input("Consider Touch New Before", DefaultValue = 1)]
        public ISpread<int> FConsiderNew;
        [Input("Consider Touch Released After", DefaultValue = 1)]
        public ISpread<int> FConsiderReleased;

        [Input("View")]
        public Pin<VMatrix> FViewTr;
        [Input("Projection")]
        public Pin<VMatrix> FProjTr;
        [Input("Aspect Ratio")]
        public Pin<VMatrix> FAspTr;

        [Output("Context")]
        public ISpread<ISpread<NotuiContext>> FContext;
        [Output("Hierarchical Elements")]
        public ISpread<ISpread<NotuiElement>> FElementsOut;
        [Output("Flat Elements")]
        public ISpread<ISpread<NotuiElement>> FFlatElements;

        [Output("Touches")]
        public ISpread<ISpread<Touch>> FTouches;

        private int _areElementsChanged = 0;

        public void Evaluate(int SpreadMax)
        {
            if (FHostElements.IsConnected && FHostElements.SliceCount > 0 && FHostElements[0] != null)
            {
                if (FElements.IsChanged) _areElementsChanged = 2;
                _prevElements.SliceCount = FHostElements.SliceCount;
                FContext.SliceCount = FHostElements.SliceCount;
                FElementsOut.SliceCount = FHostElements.SliceCount;
                FFlatElements.SliceCount = FHostElements.SliceCount;
                FTouches.SliceCount = FHostElements.SliceCount;

                for (int i = 0; i < FHostElements.SliceCount; i++)
                {
                    var hostel = FHostElements[i];
                    if (hostel.SubContext == null)
                    {
                        FContext[i].SliceCount = 0;
                        FElementsOut[i].SliceCount = 0;
                        FFlatElements[i].SliceCount = 0;
                        FTouches[i].SliceCount = 0;
                        continue;
                    }
                    
                    var context = hostel.SubContext.Context;
                    context.View = FViewTr[i].AsSystemMatrix4X4();
                    context.Projection = FProjTr[i].AsSystemMatrix4X4();
                    context.AspectRatio = FAspTr[i].AsSystemMatrix4X4();


                    for (int j = 0; j < FElements[i].SliceCount; j++)
                    {
                        if (FElements[i][j] == null) continue;
                        if (_prevElements[i]?.Contains(FElements[i][j]) ?? true) continue;
                        FElements[i][j].IsChanged = ElementNodeUtils.ChangedFrames;
                    }

                    if(_prevElements[i] == null) _prevElements[i] = new Spread<ElementPrototype>();
                    _prevElements[i].AssignFrom(FElements[i]);

                    if (_areElementsChanged > 0 && FAutoUpdateElements[i] || FUpdateElements[i])
                    {
                        context.AddOrUpdateElements(true, FElements[i].Where(el => el != null).ToArray());
                    }

                    FContext[i].SliceCount = 1;
                    FContext[i][0] = context;

                    FFlatElements[i].SliceCount = context.FlatElements.Count;
                    for (int j = 0; j < context.FlatElements.Count; j++)
                    {
                        var element = context.FlatElements[j];
                        FFlatElements[i][j] = element;
                        switch (element.EnvironmentObject)
                        {
                            case null:
                                element.EnvironmentObject = new VEnvironmentData(element);
                                break;
                            case VEnvironmentData venvdat:
                                if (venvdat.FlattenedEvents != null) continue;
                                venvdat.FlattenedEvents = new ElementEventFlattener(Host);
                                venvdat.FlattenedEvents.Subscribe(element);
                                break;
                        }
                    }

                    FElementsOut[i].AssignFrom(context.RootElements.Values);
                    FTouches[i].AssignFrom(context.Touches.Values);
                }
            }
            else
            {

                FContext.SliceCount = 0;
                FElementsOut.SliceCount = 0;
                FFlatElements.SliceCount = 0;
                FTouches.SliceCount = 0;
            }

            if(_areElementsChanged > 0) _areElementsChanged--;
        }

        public bool OutputRequiresInputEvaluation(IPluginIO inputPin, IPluginIO outputPin)
        {
            return false;
        }
    }
}

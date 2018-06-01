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
        Name = "Context",
        Category = "Notui",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class ContextNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IPluginFeedbackLoop
    {
        [Import] public IPluginHost2 PluginHost;
        [Import] public IHDEHost Host;

        [Config("Thread Count", DefaultValue = 4)]
        public IDiffSpread<int> FThreads;

        [Input("Element Prototypes")]
        public Pin<ElementPrototype> FElements;

        [Input("Mouse")]
        public Pin<Mouse> FMouse;
        [Input("Mouse Touch ID", DefaultValue = -1)]
        public ISpread<int> FMouseId;
        [Input("Mouse Always Present")]
        public ISpread<bool> FAlwaysPresent;
        [Input("Mouse Pressed Force", DefaultValue = 1.0)]
        public ISpread<float> FMouseForce;

        [Input("DX11 Touches")]
        public Pin<TouchData> FDx11Touches;
        [Input("DX11 Touch Force", DefaultValue = 1.0)]
        public ISpread<float> FDx11TouchForce;
        [Input("Aux Touches ", DimensionNames = new []{"X", "Y", "F", "Id"})]
        public IDiffSpread<Vector4D> FAuxTouches;

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
        public ISpread<NotuiContext> FContext;
        [Output("Hierarchical Elements")]
        public ISpread<NotuiElement> FElementsOut;
        [Output("Flat Elements")]
        public ISpread<NotuiElement> FFlatElements;
        
        [Output("Touches")]
        public ISpread<Touch> FTouches;

        public NotuiContext Context = new NotuiContext();

        private double _prevFrameTime = 0;
        private bool _initMo = true;
        private int _areElementsChanged = 0;

        public bool IsTouchDefault()
        {
            bool def = FAuxTouches.SliceCount == 1;
            def = def && FAuxTouches[0].xy.Length < 0.00001;
            def = def && Math.Abs(FAuxTouches[0].w) < 0.00001;
            def = def && Math.Abs(FAuxTouches[0].z) < 0.00001;
            return def;
        }

        public void OnImportsSatisfied()
        {
            FElements.Disconnected += (sender, args) =>
            {
                Context.AddOrUpdateElements(true); // this is basically asking all elements to request their deletion
            };
        }

        /* Button order in rawinput apparently:
         * Right, Left, X1, X2, Middle
         * 2, 0, 3, 4, 1
         * 1, 4, 0, 2, 3
         */

        public void Evaluate(int SpreadMax)
        {

            Context.ConsiderNewBefore = FConsiderNew[0];
            Context.ConsiderReleasedAfter = FConsiderReleased[0];
            Context.MinimumForce = FMinForce[0];

            if (FThreads.IsChanged) Context.ParallelThreads = FThreads[0];

            var touchcount = FAuxTouches.SliceCount;
            var touches = Enumerable.Empty<(Vector2, int, float)>();

            touches = touches.Concat(Enumerable.Range(0, FDx11Touches.SliceCount).Where(i => FDx11Touches[i] != null).Select(i =>
            {
                var touch = FDx11Touches[i];
                var pos = new Vector2(touch.Pos.X, touch.Pos.Y);
                return (pos, touch.Id, FDx11TouchForce[i]);
            }));

            if (!IsTouchDefault())
            {
                touches = touches.Concat(Enumerable.Range(0, touchcount).Select(i =>
                    (FAuxTouches[i].xy.AsSystemVector(), (int)FAuxTouches[i].w, (float)FAuxTouches[i].z)));
            }

            Context.MouseAlwaysPresent = FAlwaysPresent[0];
            Context.MouseTouchForce = FMouseForce[0];
            Context.MouseTouchId = FMouseId[0];

            if (FMouse.IsConnected && FMouse.SliceCount > 0)
            {
                if (FMouse[0] != null)
                {
                    if (_initMo)
                    {
                        Context.SubmitMouse(FMouse[0]);
                        _initMo = false;
                    }
                }
                else
                {
                    if (!_initMo)
                    {
                        Context.DetachMouse();
                        _initMo = true;
                    }
                }
            }
            else
            {
                if (!_initMo)
                {
                    Context.DetachMouse();
                    _initMo = true;
                }
            }

            Context.SubmitTouches(touches);

            var dt = Host.FrameTime - _prevFrameTime;
            if (_prevFrameTime <= 0.00001) dt = 0;

            if(FViewTr.IsConnected && FViewTr.SliceCount > 0)
                Context.View = FViewTr[0].AsSystemMatrix4X4();

            if (FProjTr.IsConnected && FProjTr.SliceCount > 0)
                Context.Projection = FProjTr[0].AsSystemMatrix4X4();

            if (FAspTr.IsConnected && FAspTr.SliceCount > 0)
                Context.AspectRatio = FAspTr[0].AsSystemMatrix4X4();

            if (FElements.IsChanged && FElements.IsConnected)
                _areElementsChanged = 2;
            if (_areElementsChanged > 0)
            {
                Context.AddOrUpdateElements(true, FElements.Where(el => el != null).ToArray());
                _areElementsChanged--;
            }

            Context.Mainloop((float)dt);

            FContext[0] = Context;

            FFlatElements.SliceCount = Context.FlatElements.Count;

            for (int i = 0; i < Context.FlatElements.Count; i++)
            {
                var element = Context.FlatElements[i];
                FFlatElements[i] = element;
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
            
            FElementsOut.AssignFrom(Context.RootElements.Values);
            FTouches.AssignFrom(Context.Touches.Values);

            _prevFrameTime = Host.FrameTime;
        }

        public bool OutputRequiresInputEvaluation(IPluginIO inputPin, IPluginIO outputPin)
        {
            return false;
        }
    }

    [PluginInfo(
        Name = "Context",
        Category = "Notui",
        Version = "Split",
        Author = "microdee"
    )]
    public class ContextSplitNode : ObjectSplitNode<NotuiContext>
    {
        public override Type TransformType(Type original, MemberInfo member)
        {
            return MiscExtensions.MapSystemNumericsTypeToVVVV(original);
        }

        public override object TransformOutput(object obj, MemberInfo member, int i)
        {
            return MiscExtensions.MapSystemNumericsValueToVVVV(obj);
        }
    }

    [PluginInfo(
        Name = "Touch",
        Category = "Notui",
        Version = "Split",
        Author = "microdee"
    )]
    public class TouchSplitNode : ObjectSplitNode<Touch>
    {
        public override Type TransformType(Type original, MemberInfo member)
        {
            return MiscExtensions.MapSystemNumericsTypeToVVVV(original);
        }

        public override object TransformOutput(object obj, MemberInfo member, int i)
        {
            return MiscExtensions.MapSystemNumericsValueToVVVV(obj);
        }
    }
}

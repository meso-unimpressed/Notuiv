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

        [Config("Thread Count", DefaultValue = 4, IsSingle = true)]
        public IDiffSpread<int> ThreadsIn;

        [Input("Element Prototypes")]
        public Pin<ElementPrototype> ElementsIn;
        [Input("Force Update Elements", IsSingle = true, IsBang = true)]
        public ISpread<bool> ForceUpdateIn;

        private Spread<ElementPrototype> _prevElements = new Spread<ElementPrototype>();

        [Input("Mouse", IsSingle = true)]
        public Pin<Mouse> MouseIn;
        [Input("Mouse Touch ID", DefaultValue = -1, IsSingle = true)]
        public ISpread<int> MouseIdIn;
        [Input("Mouse Always Present", IsSingle = true)]
        public ISpread<bool> AlwaysPresentIn;
        [Input("Mouse Pressed Force", DefaultValue = 1.0, IsSingle = true)]
        public ISpread<float> MouseForceIn;

        [Input("DX11 Touches")]
        public Pin<TouchData> Dx11TouchesIn;
        [Input("DX11 Touch Force", DefaultValue = 1.0, IsSingle = true)]
        public ISpread<float> Dx11TouchForceIn;
        [Input("Aux Touches ", DimensionNames = new []{"X", "Y", "F", "Id"})]
        public IDiffSpread<Vector4D> AuxTouchesIn;

        [Input("Minimum Force for Interaction", DefaultValue = 0.5, IsSingle = true)]
        public IDiffSpread<float> MinForceIn;
        [Input("Consider Touch New Before", DefaultValue = 1, IsSingle = true)]
        public ISpread<int> ConsiderNewIn;
        [Input("Consider Touch Released After", DefaultValue = 1, IsSingle = true)]
        public ISpread<int> ConsiderReleasedIn;

        [Input("View", IsSingle = true)]
        public Pin<VMatrix> ViewTrIn;
        [Input("Projection", IsSingle = true)]
        public Pin<VMatrix> ProjTrIn;
        [Input("Aspect Ratio", IsSingle = true)]
        public Pin<VMatrix> AspTrIn;

        [Output("Context")]
        public ISpread<NotuiContext> ContextOut;
        [Output("Hierarchical Elements")]
        public ISpread<NotuiElement> ElementsOut;
        [Output("Flat Elements")]
        public ISpread<NotuiElement> FlatElementsOut;
        
        [Output("Touches")]
        public ISpread<Touch> TouchesOut;

        public NotuiContext Context = new NotuiContext
        {
            UpdateOnlyChangeFlagged = true
        };

        private double _prevFrameTime = 0;
        private bool _initMo = true;
        private int _areElementsChanged = 0;

        public bool IsTouchDefault()
        {
            bool def = AuxTouchesIn.SliceCount == 1;
            def = def && AuxTouchesIn[0].xy.Length < 0.00001;
            def = def && Math.Abs(AuxTouchesIn[0].w) < 0.00001;
            def = def && Math.Abs(AuxTouchesIn[0].z) < 0.00001;
            return def;
        }

        public void OnImportsSatisfied()
        {
            //FElements.Disconnected += (sender, args) =>
            //{
            //    Context.AddOrUpdateElements(true); // this is basically asking all elements to request their deletion
            //};
        }

        /* Button order in rawinput apparently:
         * Right, Left, X1, X2, Middle
         * 2, 0, 3, 4, 1
         * 1, 4, 0, 2, 3
         */

        public void Evaluate(int SpreadMax)
        {

            Context.ConsiderNewBefore = ConsiderNewIn.TryGetSlice(0);
            Context.ConsiderReleasedAfter = ConsiderReleasedIn.TryGetSlice(0);
            Context.MinimumForce = MinForceIn.TryGetSlice(0);

            if (ThreadsIn.IsChanged) Context.ParallelThreads = ThreadsIn.TryGetSlice(0);

            var touchcount = AuxTouchesIn.SliceCount;
            var touches = Enumerable.Empty<(Vector2, int, float)>();

            touches = touches.Concat(Enumerable.Range(0, Dx11TouchesIn.SliceCount).Where(i => Dx11TouchesIn[i] != null).Select(i =>
            {
                var touch = Dx11TouchesIn[i];
                var pos = new Vector2(touch.Pos.X, touch.Pos.Y);
                return (pos, touch.Id, Dx11TouchForceIn[i]);
            }));

            if (!IsTouchDefault())
            {
                touches = touches.Concat(Enumerable.Range(0, touchcount).Select(i =>
                    (AuxTouchesIn[i].xy.AsSystemVector(), (int)AuxTouchesIn[i].w, (float)AuxTouchesIn[i].z)));
            }

            Context.MouseAlwaysPresent = AlwaysPresentIn.TryGetSlice(0);
            Context.MouseTouchForce = MouseForceIn.TryGetSlice(0);
            Context.MouseTouchId = MouseIdIn.TryGetSlice(0);

            if (MouseIn.IsConnected && MouseIn.SliceCount > 0)
            {
                if (MouseIn[0] != null)
                {
                    if (_initMo)
                    {
                        Context.SubmitMouse(MouseIn[0]);
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

            if(ViewTrIn.IsConnected && ViewTrIn.SliceCount > 0)
                Context.View = ViewTrIn[0].AsSystemMatrix4X4();
            else Context.View = SMatrix.Identity;

            if (ProjTrIn.IsConnected && ProjTrIn.SliceCount > 0)
                Context.Projection = ProjTrIn[0].AsSystemMatrix4X4();
            else Context.Projection = SMatrix.Identity;

            if (AspTrIn.IsConnected && AspTrIn.SliceCount > 0)
                Context.AspectRatio = AspTrIn[0].AsSystemMatrix4X4();
            else Context.AspectRatio = SMatrix.Identity;

            if (ElementsIn.IsChanged)
            {
                for (int i = 0; i < ElementsIn.SliceCount; i++)
                {
                    if (ElementsIn[i] == null) continue;
                    if (_prevElements.Contains(ElementsIn[i])) continue;
                    ElementsIn[i].IsChanged = ElementNodeUtils.ChangedFrames;
                }

                _prevElements.AssignFrom(ElementsIn);
                _areElementsChanged = 2;
            }
            
            if (_areElementsChanged > 0 || ForceUpdateIn.TryGetSlice(0))
            {
                Context.AddOrUpdateElements(true, ElementsIn.Where(el => el != null).ToArray());
                _areElementsChanged--;
            }

            Context.Mainloop((float)dt);

            ContextOut[0] = Context;

            FlatElementsOut.SliceCount = Context.FlatElements.Count;

            for (int i = 0; i < Context.FlatElements.Count; i++)
            {
                var element = Context.FlatElements[i];
                FlatElementsOut[i] = element;
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
            
            ElementsOut.AssignFrom(Context.RootElements.Values);
            TouchesOut.AssignFrom(Context.Touches.Values);

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

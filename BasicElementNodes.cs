using System.Linq;
using System.Numerics;
using md.stdl.Mathematics;
using Notui;
using Notui.Elements;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace Notuiv
{

    [PluginInfo(
        Name = "Void",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class VoidElementNode : AbstractElementNode<VoidElementPrototype>
    {
        protected override VoidElementPrototype ConstructPrototype(int i, string id)
        {
            return new VoidElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id);
        }
    }

    [PluginInfo(
        Name = "Plane",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class PlaneElementNode : AbstractElementNode<InfinitePlaneElementPrototype>
    {
        protected override InfinitePlaneElementPrototype ConstructPrototype(int i, string id)
        {
            return new InfinitePlaneElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id);
        }
    }

    [PluginInfo(
        Name = "Rectangle",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class RectangleElementNode : AbstractElementNode<RectangleElementPrototype>
    {
        protected override RectangleElementPrototype ConstructPrototype(int i, string id)
        {
            return new RectangleElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id);
        }
    }

    [PluginInfo(
        Name = "Circle",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class CircleElementNode : AbstractElementNode<CircleElementPrototype>
    {
        protected override CircleElementPrototype ConstructPrototype(int i, string id)
        {
            return new CircleElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id);
        }
    }

    [PluginInfo(
        Name = "Segment",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class SegmentElementNode : AbstractElementNode<SegmentElementPrototype>
    {
        [Input("Inner Radius")]
        public IDiffSpread<float> FInnerRadius;

        [Input("Cycles")]
        public IDiffSpread<float> FCycles;

        [Input("Phase")]
        public IDiffSpread<float> FPhase;

        protected override SegmentElementPrototype ConstructPrototype(int i, string id)
        {
            return new SegmentElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id)
            {
                HoleRadius = FInnerRadius[i],
                Cycles = FCycles[i],
                Phase = FPhase[i]
            };
        }

        protected override void FillElementAuxData(SegmentElementPrototype el, int i)
        {
            el.Cycles = FCycles[i];
            el.HoleRadius = FInnerRadius[i];
            el.Phase = FPhase[i];
        }

        protected override bool AuxDataChanged()
        {
            return FInnerRadius.IsChanged || FCycles.IsChanged || FPhase.IsChanged;
        }
    }

    [PluginInfo(
        Name = "GetSegmentParams",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee"
    )]
    public class GetSegmentParamsNode : GetExtraParamsNodeBase<SegmentElement>
    {
        [Output("Inner Radius")]
        public ISpread<float> FInnerRad;

        [Output("Cycles")]
        public ISpread<float> FCycles;

        [Output("Phase")]
        public ISpread<float> FPhase;

        public override void GetExtraParameters(int i, SegmentElement element)
        {
            FInnerRad[i] = element.HoleRadius;
            FCycles[i] = element.Cycles;
            FPhase[i] = element.Phase;
        }

        public override void SetSliceCounts()
        {
            FCycles.SliceCount = FInnerRad.SliceCount = FPhase.SliceCount = FIn.SliceCount;
        }

        public override void EmptySpreads()
        {
            FCycles.SliceCount = FInnerRad.SliceCount = FPhase.SliceCount = 0;
        }
    }

    [PluginInfo(
        Name = "SetSegmentParams",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetSegmentParamsNode : SetExtraParamsNodeBase<SegmentElement>
    {
        [Input("Inner Radius")]
        public IDiffSpread<float> FInnerRad;

        [Input("Cycles")]
        public IDiffSpread<float> FCycles;

        [Input("Phase")]
        public IDiffSpread<float> FPhase;

        public override void SetExtraParameters(int i, SegmentElement element)
        {
            element.HoleRadius = FInnerRad[i];
            element.Cycles = FCycles[i];
            element.Phase = FPhase[i];
        }
    }

    [PluginInfo(
        Name = "Polygon2D",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class PolygonElementNode : AbstractElementNode<PolygonElementPrototype>
    {
        [Input("Vertices")]
        public IDiffSpread<ISpread<Vector2D>> FVerts;

        protected override PolygonElementPrototype ConstructPrototype(int i, string id)
        {
            var res = new PolygonElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id);
            res.Vertices.Clear();
            res.Vertices.AddRange(FVerts[i].Select(v => v.AsSystemVector()));
            return res;
        }

        protected override void FillElementAuxData(PolygonElementPrototype el, int i)
        {
            el.Vertices.Clear();
            el.Vertices.AddRange(FVerts[i].Select(v => v.AsSystemVector()));
        }

        protected override bool AuxDataChanged()
        {
            return FVerts.IsChanged;
        }
    }

    [PluginInfo(
        Name = "GetPolygon2DParams",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee"
    )]
    public class GetPolygon2DParamsNode : GetExtraParamsNodeBase<PolygonElement>
    {
        [Output("Vertices")]
        public ISpread<ISpread<Vector2D>> FVerts;

        public override void GetExtraParameters(int i, PolygonElement element)
        {
            FVerts[i].AssignFrom(element.Vertices.Select(v => v.AsVVector()));
        }

        public override void SetSliceCounts()
        {
            FVerts.SliceCount = FIn.SliceCount;
        }

        public override void EmptySpreads()
        {
            FVerts.SliceCount = 0;
        }
    }

    [PluginInfo(
        Name = "SetPolygon2DParams",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetPolygon2DParamsNode : SetExtraParamsNodeBase<PolygonElement>
    {
        [Input("Vertices")]
        public IDiffSpread<ISpread<Vector2D>> FVerts;

        public override void SetExtraParameters(int i, PolygonElement element)
        {
            element.Vertices.Clear();
            element.Vertices.AddRange(FVerts[i].Select(v => v.AsSystemVector()));
        }
    }

    [PluginInfo(
        Name = "Sphere",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class SphereElementNode : AbstractElementNode<SphereElementPrototype>
    {
        protected override SphereElementPrototype ConstructPrototype(int i, string id)
        {
            return new SphereElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id);
        }
    }

    [PluginInfo(
        Name = "Box",
        Category = "Notui.ElementPrototype",
        Version = "Join",
        Author = "microdee"
    )]
    public class BoxElementNode : AbstractElementNode<BoxElementPrototype>
    {
        [Input("Size", DefaultValues = new [] {1.0, 1.0, 1.0})]
        public IDiffSpread<Vector3D> FSize;

        protected override BoxElementPrototype ConstructPrototype(int i, string id)
        {
            return new BoxElementPrototype(string.IsNullOrWhiteSpace(id) ? null : id)
            {
                Size = FSize[i].AsSystemVector()
            };
        }

        protected override void FillElementAuxData(BoxElementPrototype el, int i)
        {
            el.Size = FSize[i].AsSystemVector();
        }

        protected override bool AuxDataChanged()
        {
            return FSize.IsChanged;
        }
    }

    [PluginInfo(
        Name = "GetBoxParams",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee"
    )]
    public class GetBoxParamsNode : GetExtraParamsNodeBase<BoxElement>
    {
        [Output("Size")]
        public ISpread<Vector3D> FSize;

        public override void GetExtraParameters(int i, BoxElement element)
        {
            FSize[i] = element.Size.AsVVector();
        }

        public override void SetSliceCounts()
        {
            FSize.SliceCount = FIn.SliceCount;
        }

        public override void EmptySpreads()
        {
            FSize.SliceCount = 0;
        }
    }

    [PluginInfo(
        Name = "SetBoxParams",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetBoxParamsNode : SetExtraParamsNodeBase<BoxElement>
    {
        [Input("Size", DefaultValues = new[] { 1.0, 1.0, 1.0 })]
        public IDiffSpread<Vector3D> FSize;

        public override void SetExtraParameters(int i, BoxElement element)
        {
            element.Size = FSize[i].AsSystemVector();
        }
    }
}

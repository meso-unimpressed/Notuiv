using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Mathematics;
using mp.pddn;
using Notui;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace Notuiv
{
    [PluginInfo(
        Name = "ElementTransformation",
        Category = "Notui",
        Version = "Split",
        Author = "microdee"
    )]
    public class ElementTransformationSplitNodes : ObjectSplitNode<ElementTransformation>
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
        Name = "ElementTransformation",
        Category = "Notui",
        Version = "Join",
        Author = "microdee"
    )]
    public class JoinElementTransformationNode : IPluginEvaluate
    {
        [Input("Translate")]
        public IDiffSpread<Vector3D> FTrans;

        [Input("Quaternion", DefaultValues = new[] { 0.0, 0.0, 0.0, 1.0 })]
        public IDiffSpread<Vector4D> FRot;

        [Input("Scale", DefaultValues = new [] {1.0, 1.0, 1.0})]
        public IDiffSpread<Vector3D> FScale;

        [Output("Element Transform", AutoFlush = false)]
        public ISpread<ElementTransformation> FTransform;

        public void Evaluate(int SpreadMax)
        {
            FTransform.Stream.IsChanged = false;
            if (FTrans.IsChanged || FRot.IsChanged || FScale.IsChanged)
            {
                FTransform.SliceCount = SpreadMax;
                for (int i = SpreadMax - 1; i >=0; i--)
                {
                    if(FTransform[i] == null)
                        FTransform[i] = new ElementTransformation();
                    else if(i > 0)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (FTransform[i] != FTransform[j]) continue;
                            FTransform[i] = new ElementTransformation();
                            break;
                        }
                    }

                    FTransform[i].Position = FTrans[i].AsSystemVector();
                    FTransform[i].Rotation = FRot[i].AsSystemQuaternion();
                    FTransform[i].Scale = FScale[i].AsSystemVector();
                }
                FTransform.Flush(true);
                FTransform.Stream.IsChanged = true;
            }
        }
    }

    [PluginInfo(
        Name = "SetTransformation",
        Category = "Notui.Element",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetElementTransformation : IPluginEvaluate
    {
        [Input("Element")] public Pin<NotuiElement> FElement;
        [Input("Display")] public Pin<ElementTransformation> FDisp;
        [Input("Set Display")] public ISpread<bool> FSetDisp;
        [Input("Target")] public Pin<ElementTransformation> FTarget;
        [Input("Set Target")] public ISpread<bool> FSetTarget;

        [Input("Transformation Update Mode", DefaultEnumEntry = "All")]
        public ISpread<ISpread<ApplyTransformMode>> FTrUpdateMode;

        [Output("Element Out")] public ISpread<NotuiElement> FElementOut;

        public void Evaluate(int SpreadMax)
        {
            if (FElement.IsConnected)
            {
                FElementOut.SliceCount = FElement.SliceCount;
                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    var trupdmode = FTrUpdateMode[i].Aggregate(ApplyTransformMode.None, (current, mode) => current | mode);
                    FElementOut[i] = FElement[i];
                    if (FSetDisp[i] && FDisp.IsConnected && FDisp.SliceCount > 0)
                    {
                        if(FDisp[i] != null)
                            FElement[i].DisplayTransformation.UpdateFrom(FDisp[i], trupdmode);
                    }
                    if (FSetTarget[i] && FTarget.IsConnected && FTarget.SliceCount > 0)
                    {
                        if (FTarget[i] != null)
                            FElement[i].TargetTransformation.UpdateFrom(FTarget[i], trupdmode);
                    }
                }
            }
            else
            {
                FElementOut.SliceCount = 0;
            }
        }
    }

    [PluginInfo(
        Name = "SetTransformation",
        Category = "Notui.Element",
        Version = "Matrix",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetElementTransformationFromMatrix : IPluginEvaluate
    {
        [Input("Element")] public Pin<NotuiElement> FElement;
        [Input("Display")] public ISpread<Matrix4x4> FDisp;
        [Input("Set Display")] public ISpread<bool> FSetDisp;
        [Input("Target")] public ISpread<Matrix4x4> FTarget;
        [Input("Set Target")] public ISpread<bool> FSetTarget;

        [Output("Element Out")] public ISpread<NotuiElement> FElementOut;

        public void Evaluate(int SpreadMax)
        {
            if (FElement.IsConnected)
            {
                FElementOut.SliceCount = FElement.SliceCount;
                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    if (FSetDisp[i] && FDisp.SliceCount > 0)
                    {
                        FDisp[i].Decompose(out var scale, out Vector4D rotation, out var pos);
                        var tr = FElement[i].DisplayTransformation;
                        tr.Position = pos.AsSystemVector();
                        tr.Rotation = rotation.AsSystemQuaternion();
                        tr.Scale = scale.AsSystemVector();
                    }
                    if (FSetTarget[i] && FTarget.SliceCount > 0)
                    {
                        FTarget[i].Decompose(out var scale, out Vector4D rotation, out var pos);
                        var tr = FElement[i].TargetTransformation;
                        tr.Position = pos.AsSystemVector();
                        tr.Rotation = rotation.AsSystemQuaternion();
                        tr.Scale = scale.AsSystemVector();
                    }
                    FElementOut[i] = FElement[i];
                }
            }
            else
            {
                FElementOut.SliceCount = 0;
            }
        }
    }

}

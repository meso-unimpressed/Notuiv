using System;
using System.Diagnostics;
using System.Reflection;
using md.stdl.Coding;
using md.stdl.Interaction;
using Notui;
using mp.pddn;
using VVVV.PluginInterfaces.V2;
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
        Name = "PrototypeInfo",
        Category = "Notui.ElementPrototype",
        Version = "Split",
        Author = "microdee"
    )]
    public class PrototypeInfoSplitNodes : ObjectSplitNode<ElementPrototype>
    {
        public override Type TransformType(Type original, MemberInfo member)
        {
            if (original.Is(typeof(Stopwatch)))
            {
                return typeof(double);
            }
            return MiscExtensions.MapSystemNumericsTypeToVVVV(original);
        }
        public override object TransformOutput(object obj, MemberInfo member, int i)
        {
            if (obj is Stopwatch s)
            {
                return s.Elapsed.TotalSeconds;
            }
            return MiscExtensions.MapSystemNumericsValueToVVVV(obj);
        }
    }

    [PluginInfo(
        Name = "InstanceInfo",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee"
    )]
    public class GuiElementInfoNode : IPluginEvaluate
    {
        [Input("Element")] public Pin<NotuiElement> FElement;

        [Output("Element")] public ISpread<NotuiElement> FElementOut;
        [Output("Type")] public ISpread<string> FType;

        [Output("Name Out")] public ISpread<string> FNameOut;
        [Output("ID")] public ISpread<string> FId;
        [Output("Hit")] public ISpread<bool> FHit;
        [Output("Touched")] public ISpread<bool> FTouched;
        [Output("Active Out")] public ISpread<bool> FActiveOut;
        [Output("Transparent Out")] public ISpread<bool> FTransparentOut;
        [Output("Fade Out Duration")] public ISpread<float> FFadeOutDur;
        [Output("Fade In Duration")] public ISpread<float> FFadeInDur;
        [Output("Fade Progress")] public ISpread<float> FElementFade;
        [Output("Age")] public ISpread<double> FAge;
        [Output("Dethklok")] public ISpread<double> FDethklok;
        [Output("Dying")] public ISpread<bool> FDying;

        [Output("Interacting Touches")] public ISpread<ISpread<Touch>> FTouches;
        [Output("Are Interacting Touches Hitting")] public ISpread<ISpread<bool>> FTouchesHitting;
        [Output("Touching Intersections")] public ISpread<ISpread<IntersectionPoint>> FTouchingIntersections;
        [Output("Hitting Touches")] public ISpread<ISpread<Touch>> FHittingTouches;
        [Output("Hitting Intersections")] public ISpread<ISpread<IntersectionPoint>> FHittingIntersections;
        [Output("Children Out")] public ISpread<ISpread<NotuiElement>> FChildrenOut;
        [Output("Behaviors Out")] public ISpread<ISpread<InteractionBehavior>> FBehavsOut;
        [Output("Parent")] public ISpread<ISpread<NotuiElement>> FParent;
        [Output("Context")] public ISpread<NotuiContext> FContext;
        
        [Output("Interaction Transformation Out")] public ISpread<Matrix4x4> FInterTrOut;
        [Output("Display Transformation Out")] public ISpread<Matrix4x4> FDisplayTrOut;
        
        protected void AssignElementOutputs(NotuiElement element, int i)
        {
            if (element.EnvironmentObject is VEnvironmentData venvdat)
            {
                FTouches[i] = venvdat.Touches;
                FTouchesHitting[i] = venvdat.TouchesHitting;
                FTouchingIntersections[i] = venvdat.TouchingIntersections;
                FHittingTouches[i] = venvdat.HittingTouches;
                FHittingIntersections[i] = venvdat.HittingIntersections;
                FChildrenOut[i] = venvdat.Children;
                FBehavsOut[i] = venvdat.Behaviors;
                FParent[i] = venvdat.Parent;
                FInterTrOut[i] = venvdat.VInterTr;
                FDisplayTrOut[i] = venvdat.VDispTr;
                FType[i] = venvdat.TypeCSharpName;
            }
            FElementOut[i] = element;
            FNameOut[i] = element.Name;
            
            FId[i] = element.Id;
            FHit[i] = element.Hit;
            FTouched[i] = element.Touched;
            FActiveOut[i] = element.Active;
            FTransparentOut[i] = element.Transparent;
            FFadeOutDur[i] = element.FadeOutTime;
            FFadeInDur[i] = element.FadeInTime;
            FElementFade[i] = element.ElementFade;
            FAge[i] = element.Age.Elapsed.TotalSeconds;
            FDying[i] = element.Dying;
            FDethklok[i] = element.Dethklok.Elapsed.TotalSeconds;
        }

        protected int _prevSliceCount;

        public void Evaluate(int SpreadMax)
        {
            if (FElement.IsConnected && FElement.SliceCount > 0 && FElement[0] != null)
            {
                if (_prevSliceCount != FElement.SliceCount)
                {
                    this.SetSliceCountForAllOutput(FElement.SliceCount);
                }
                _prevSliceCount = FElement.SliceCount;

                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    AssignElementOutputs(FElement[i], i);
                }
            }
            else
            {
                _prevSliceCount = 0;
                this.SetSliceCountForAllOutput(0);
            }
        }
    }
}

using System.ComponentModel.Composition;
using System.Linq;
using Notui;
using md.stdl.Mathematics;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VMatrix = VVVV.Utils.VMath.Matrix4x4;
using SMatrix = System.Numerics.Matrix4x4;

namespace Notuiv
{
    public abstract class AbstractElementNode<TPrototype> : IPluginEvaluate where TPrototype : ElementPrototype
    {
        [Import] public IPluginHost2 PluginHost;
        [Import] public IHDEHost Host;

        protected abstract TPrototype ConstructPrototype(int i, string id);
        
        [Input("Children")]
        public ISpread<ISpread<ElementPrototype>> FChildren;
        [Input("Name", DefaultString = "callmenames")]
        public IDiffSpread<string> FName;
        [Input("Id", DefaultString = "")]
        public IDiffSpread<string> FId;
        [Input("Fade In Time")]
        public IDiffSpread<float> FFadeIn;
        [Input("Fade Out Time")]
        public IDiffSpread<float> FFadeOut;
        [Input("Behaviors")]
        public ISpread<ISpread<InteractionBehavior>> FBehaviors;
        [Input("Transparent")]
        public IDiffSpread<bool> FTransparent;
        [Input("Active", DefaultBoolean = true)]
        public IDiffSpread<bool> FActive;

        [Input("Display Transform")]
        public IDiffSpread<VMatrix> FDispTr;
        [Input("Interaction Transform")]
        public IDiffSpread<VMatrix> FInterTr;
        [Input("Separate Interaction from Display")]
        public IDiffSpread<bool> FSeparateInter;
        [Input("Transformation Update Mode", DefaultEnumEntry = "All")]
        public ISpread<ISpread<ApplyTransformMode>> FTrUpdateMode;

        [Output("Element Prototype")]
        public ISpread<TPrototype> FElementProt;

        [Output("Element Id")]
        public ISpread<string> FElementId;

        protected virtual void FillElementAuxData(TPrototype el, int i) { }

        protected TPrototype FillElement(TPrototype el, int i)
        {
            FDispTr[i].Decompose(out var scale, out Vector4D rotation, out var pos);
            el.Name = FName[i];
            el.FadeInTime = FFadeIn[i];
            el.FadeOutTime = FFadeOut[i];
            el.Active = FActive[i];
            el.Transparent = FTransparent[i];
            if (FBehaviors[i].All(chel => chel != null))
            {
                el.Behaviors = FBehaviors[i].ToList();
            }
            el.DisplayTransformation.Position = pos.AsSystemVector();
            el.DisplayTransformation.Rotation = rotation.AsSystemQuaternion();
            el.DisplayTransformation.Scale = scale.AsSystemVector();
            if (FSeparateInter[i])
            {
                FInterTr[i].Decompose(out var iscale, out Vector4D irotation, out var ipos);
                el.InteractionTransformation.Position = ipos.AsSystemVector();
                el.InteractionTransformation.Rotation = irotation.AsSystemQuaternion();
                el.InteractionTransformation.Scale = iscale.AsSystemVector();
            }
            else
            {
                el.InteractionTransformation.UpdateFrom(el.DisplayTransformation);
            }

            var trupdmode = FTrUpdateMode[i].Aggregate(ApplyTransformMode.None, (current, mode) => current | mode);
            el.TransformApplication = trupdmode;
            if(FChildren[i].All(chel => chel != null))
            {
                el.Children.Clear();
                foreach (var child in FChildren[i])
                {
                    var cc = child.Parent = el;
                    el.Children.Add(child.Id, child);
                }
            }
            FillElementAuxData(el, i);

            FElementId[i] = el.Id;

            return el;
        }

        private int init = 0;

        public void Evaluate(int SpreadMax)
        {
            bool changewochildren = FName.IsChanged || FFadeIn.IsChanged || FFadeOut.IsChanged ||
                                    FBehaviors.IsChanged || FDispTr.IsChanged || FInterTr.IsChanged || FSeparateInter.IsChanged ||
                                    FTransparent.IsChanged || FActive.IsChanged || FId.IsChanged;
            bool changed = FChildren.IsChanged || changewochildren;

            int sprmax = SpreadUtils.SpreadMax(FChildren, FName, FBehaviors, FDispTr, FInterTr);

            FElementProt.Stream.IsChanged = false;

            if (changed || init < 2)
            {
                var filternilchange = true;
                if (init < 1) FElementProt.SliceCount = 0;
                else
                {
                    FElementId.SliceCount = FElementProt.SliceCount;
                    for (int i = 0; i < FElementProt.SliceCount; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(FId[i]))
                        {
                            FElementProt[i].Id = FId[i];
                        }

                        var filternilchangeslice = FChildren.SliceCount > 0 ||
                                              FChildren.SliceCount == 0 && FElementProt[i].Children.Count > 0 ||
                                              changewochildren;

                        filternilchange = filternilchange && filternilchangeslice;
                        if (filternilchangeslice)
                            FillElement(FElementProt[i], i);
                    }
                }

                var prevsc = FElementProt.SliceCount;
                FElementProt.ResizeAndDismiss(sprmax, i =>
                {
                    var res = ConstructPrototype(i, FId[i]);
                    return FillElement(res, i);
                });
                if(filternilchange || FElementProt.SliceCount != prevsc)
                    FElementProt.Stream.IsChanged = true;
            }
            init++;
        }
    }
}

using System.Collections.Generic;
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
        [Input("Manual Id")]
        public IDiffSpread<bool> FManId;
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
        [Input("Transformation Update Mode", DefaultEnumEntry = "All")]
        public ISpread<ISpread<ApplyTransformMode>> FTrUpdateMode;

        [Output("Element Prototype")]
        public ISpread<TPrototype> FElementProt;

        [Output("Element Id")]
        public ISpread<string> FElementId;

        protected virtual void FillElementAuxData(TPrototype el, int i) { }

        protected TPrototype FillElement(TPrototype el, int i, bool isnew)
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
            el.InteractionTransformation.UpdateFrom(el.DisplayTransformation);

            var trupdmode = isnew ?
                ApplyTransformMode.All :
                FTrUpdateMode[i].Aggregate(ApplyTransformMode.None, (current, mode) => current | mode);

            el.TransformApplication = trupdmode;
            if(FChildren[i].All(chel => chel != null))
            {
                el.Children.Clear();
                foreach (var child in FChildren[i])
                {
                    child.Parent = el;
                    el.Children.Add(child.Id, child);
                }
            }
            FillElementAuxData(el, i);

            FElementId[i] = el.Id;

            return el;
        }

        private int init = 0;
        private Dictionary<string, TPrototype> _manualIdElements = new Dictionary<string, TPrototype>();

        public void Evaluate(int SpreadMax)
        {
            bool changewochildren = FName.IsChanged || FFadeIn.IsChanged || FFadeOut.IsChanged ||
                                    FBehaviors.IsChanged || FDispTr.IsChanged || FManId.IsChanged ||
                                    FTransparent.IsChanged || FActive.IsChanged || FId.IsChanged;
            bool changed = FChildren.IsChanged || changewochildren;

            int sprmax = SpreadUtils.SpreadMax(FChildren, FName, FBehaviors, FDispTr);

            FElementProt.Stream.IsChanged = false;

            if (changed || init < 2)
            {
                if (FManId[0])
                {
                    for (int i = 0; i < FId.SliceCount; i++)
                    {
                        if(string.IsNullOrWhiteSpace(FId[i])) continue;
                        if (_manualIdElements.ContainsKey(FId[i]))
                        {
                            FillElement(_manualIdElements[FId[i]], i, false);
                        }
                        else
                        {
                            var prot = FillElement(ConstructPrototype(i, FId[i]), i, true);
                            _manualIdElements.Add(prot.Id, prot);
                        }
                    }

                    foreach (var k in _manualIdElements.Keys.ToArray())
                    {
                        if(FId.Contains(k)) continue;
                        _manualIdElements.Remove(k);
                    }

                    FElementProt.AssignFrom(_manualIdElements.Values);
                    FElementId.AssignFrom(_manualIdElements.Keys);
                }
                else
                {
                    FElementProt.ResizeAndDismiss(sprmax, i => FillElement(ConstructPrototype(i, null), i, true));
                    FElementId.SliceCount = sprmax;
                    for (int i = 0; i < FElementProt.SliceCount; i++)
                    {
                        FillElement(FElementProt[i], i, false);
                        FElementId[i] = FElementProt[i].Id;
                    }
                }
                FElementProt.Stream.IsChanged = true;
            }
            init++;
        }
    }
}

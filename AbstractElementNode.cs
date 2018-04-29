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
    public abstract class GetExtraParamsNodeBase<T> : IPluginEvaluate where T : NotuiElement
    {
        [Input("Input")]
        public Pin<NotuiElement> FIn;
        [Output("Is Valid")]
        public ISpread<bool> FValid;

        public abstract void GetExtraParameters(int i, T element);
        public abstract void SetSliceCounts();
        public abstract void EmptySpreads();

        public void Evaluate(int SpreadMax)
        {
            if (FIn.IsConnected && FIn.SliceCount > 0 && FIn[0] != null)
            {
                FValid.SliceCount = FIn.SliceCount;
                SetSliceCounts();
                for (int i = 0; i < FIn.SliceCount; i++)
                {
                    if (FIn[i] is T element)
                    {
                        GetExtraParameters(i, element);
                        FValid[i] = true;
                    }
                    else
                    {
                        FValid[i] = false;
                    }
                }
            }
            else
            {
                FValid.SliceCount = 1;
                FValid[0] = false;
                EmptySpreads();
            }
        }
    }

    public abstract class SetExtraParamsNodeBase<T> : IPluginEvaluate where T : NotuiElement
    {
        [Input("Element In")]
        public Pin<NotuiElement> FIn;
        [Input("Set", IsBang = true)]
        public ISpread<bool> FSet;

        [Output("Thru")]
        public ISpread<NotuiElement> FOut;
        [Output("Is Valid")]
        public ISpread<bool> FValid;

        public abstract void SetExtraParameters(int i, T element);

        public void Evaluate(int SpreadMax)
        {
            if (FIn.IsConnected && FIn.SliceCount > 0 && FIn[0] != null)
            {
                FValid.SliceCount = FOut.SliceCount = FIn.SliceCount;
                for (int i = 0; i < FIn.SliceCount; i++)
                {
                    FOut[i] = FIn[i];
                    if (FIn[i] is T element)
                    {
                        if(FSet[i]) SetExtraParameters(i, element);
                        FValid[i] = true;
                    }
                    else
                    {
                        FValid[i] = false;
                    }
                }
            }
            else
            {
                FValid.SliceCount = 1;
                FValid[0] = false;
                FOut.SliceCount = 0;
            }
        }
    }

    public abstract class AbstractElementNode<TPrototype> : IPluginEvaluate where TPrototype : ElementPrototype
    {
        [Import] public IPluginHost2 PluginHost;
        [Import] public IHDEHost Host;

        protected abstract TPrototype ConstructPrototype(int i, string id);
        
        [Input("Children")]
        public IDiffSpread<ISpread<ElementPrototype>> FChildren;

        [Input("Name", DefaultString = "callmenames")]
        public IDiffSpread<string> FName;
        [Input("Manual Id", Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> FManId;
        [Input("Id", DefaultString = "", Visibility = PinVisibility.Hidden)]
        public IDiffSpread<string> FId;

        [Input("Fade In Time")]
        public IDiffSpread<float> FFadeIn;
        [Input("Fade Out Time")]
        public IDiffSpread<float> FFadeOut;
        [Input("Fade In Delay")]
        public IDiffSpread<float> FFadeInDel;
        [Input("Fade Out Delay")]
        public IDiffSpread<float> FFadeOutDel;
        [Input("Transformation Follow Time")]
        public IDiffSpread<float> FTransFollowTime;

        [Input("Behaviors")]
        public IDiffSpread<ISpread<InteractionBehavior>> FBehaviors;
        [Input("Transparent")]
        public IDiffSpread<bool> FTransparent;
        [Input("Active", DefaultBoolean = true)]
        public IDiffSpread<bool> FActive;
        [Input("Clip Hitting Through Parent")]
        public IDiffSpread<bool> FClipParentHitting;

        [Input("Attached Values", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<float>> FAttVals;
        [Input("Attached Texts", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<string>> FAttTexts;
        [Input("Attached Auxiliary Objects", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<AuxiliaryObject>> FAttAux;
        [Input("Attached Auxiliary Keys", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<string>> FAttAuxKeys;

        [Input("Set Attached Values")]
        public IDiffSpread<bool> FSetAttVals;

        [Input("Display Transform")]
        public IDiffSpread<VMatrix> FDispTr;
        [Input("Transformation Update Mode", DefaultEnumEntry = "All")]
        public IDiffSpread<ISpread<ApplyTransformMode>> FTrUpdateMode;

        [Input("Sub Context", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<SubContextOptions> FSubContext;

        [Output("Element Prototype")]
        public ISpread<TPrototype> FElementProt;

        [Output("Element Id")]
        public ISpread<string> FElementId;

        protected virtual void FillElementAuxData(TPrototype el, int i) { }
        protected virtual bool AuxDataChanged() { return false; }

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
            el.TransformationFollowTime = FTransFollowTime[i];
            el.FadeInDelay = FFadeInDel[i];
            el.FadeOutDelay = FFadeOutDel[i];
            el.SubContextOptions = FSubContext[i];
            el.OnlyHitIfParentIsHit = FClipParentHitting[i];
            var setvals = FSetAttVals[i] || isnew;
            el.SetAttachedValues = setvals;

            if (setvals && (FAttAux[i].SliceCount > 0 || FAttTexts[i].SliceCount > 0 || FAttVals[i].SliceCount > 0))
            {
                if(el.Value == null)
                    el.Value = new AttachedValues();
                el.Value.Values = FAttVals[i].ToArray();
                el.Value.Texts = FAttTexts[i].ToArray();

                for (int j = 0; j < FAttAuxKeys[i].SliceCount; j++)
                {
                    if (el.Value.Auxiliary.ContainsKey(FAttAuxKeys[i][j]))
                        el.Value.Auxiliary[FAttAuxKeys[i][j]] = FAttAux[i][j];
                    else el.Value.Auxiliary.Add(FAttAuxKeys[i][j], FAttAux[i][j]);
                }
            }

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

            return el;
        }

        private int init = 0;
        private readonly Dictionary<string, TPrototype> _manualIdElements = new Dictionary<string, TPrototype>();

        private bool BinSizedSpreadChanged<T>(ISpread<ISpread<T>> spread)
        {
            if (spread.SliceCount == 0) return spread.IsChanged;
            if (spread.SliceCount > 1) return spread.IsChanged;
            return spread.IsChanged && spread[0].SliceCount > 0;
        }

        private bool AttachedChanged()
        {
            return BinSizedSpreadChanged(FAttVals) || BinSizedSpreadChanged(FAttTexts) ||
                   BinSizedSpreadChanged(FAttAux) || BinSizedSpreadChanged(FAttAuxKeys);
        }

        public void Evaluate(int SpreadMax)
        {
            bool changewochildren = FName.IsChanged || FManId.IsChanged || FId.IsChanged ||
                                    FFadeIn.IsChanged || FFadeOut.IsChanged || FFadeInDel.IsChanged || FFadeOutDel.IsChanged ||
                                    FTransFollowTime.IsChanged || FBehaviors.IsChanged || FTransparent.IsChanged || FActive.IsChanged ||
                                    FDispTr.IsChanged || FTrUpdateMode.IsChanged || FClipParentHitting.IsChanged || FSubContext.IsChanged ||
                                    FSetAttVals.IsChanged || AttachedChanged() || AuxDataChanged();
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

                    FElementProt.SliceCount = FElementId.SliceCount = _manualIdElements.Count;
                    int ii = 0;
                    foreach (var id in FId.Where(id => !string.IsNullOrWhiteSpace(id)))
                    {
                        FElementProt[ii] = _manualIdElements[id];
                        FElementId[ii] = FElementProt[ii].Id;
                        ii++;
                    }
                }
                else
                {
                    FElementProt.ResizeAndDismiss(sprmax, i => FillElement(ConstructPrototype(i, null), i, true));
                    FElementId.SliceCount = sprmax;
                    for (int i = 0; i < FElementProt.SliceCount; i++)
                    {
                        var isnew = false;
                        if (FElementProt[i] == null)
                        {
                            FElementProt[i] = ConstructPrototype(i, null);
                            isnew = true;
                        }
                        FillElement(FElementProt[i], i, isnew);
                        FElementId[i] = FElementProt[i].Id;
                    }
                }
                //FElementProt.Flush();
                FElementProt.Stream.IsChanged = true;
            }
            init++;
        }
    }
}

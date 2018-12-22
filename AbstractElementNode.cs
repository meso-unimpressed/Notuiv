using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using md.stdl.Coding;
using Notui;
using md.stdl.Mathematics;
using mp.pddn;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VMatrix = VVVV.Utils.VMath.Matrix4x4;
using SMatrix = System.Numerics.Matrix4x4;

namespace Notuiv
{
    public abstract class GetExtraParamsNodeBase<TInst, TProt> : IPluginEvaluate
        where TInst : NotuiElement
        where TProt : ElementPrototype
    {
        [Input("Input")]
        public Pin<IElementCommon> In;
        [Output("Is Valid")]
        public ISpread<bool> Valid;

        public abstract void GetExtraProtParameters(int i, TProt element);
        public abstract void GetExtraInstParameters(int i, TInst element);
        public abstract void SetSliceCounts();
        public abstract void EmptySpreads();

        public void Evaluate(int SpreadMax)
        {
            if (In.IsConnected && In.SliceCount > 0 && In[0] != null)
            {
                Valid.SliceCount = In.SliceCount;
                SetSliceCounts();
                for (int i = 0; i < In.SliceCount; i++)
                {
                    switch (In[i])
                    {
                        case TInst element:
                            GetExtraInstParameters(i, element);
                            Valid[i] = true;
                            break;
                        case TProt element:
                            GetExtraProtParameters(i, element);
                            Valid[i] = true;
                            break;
                        default:
                            Valid[i] = false;
                            break;
                    }
                }
            }
            else
            {
                Valid.SliceCount = 1;
                Valid[0] = false;
                EmptySpreads();
            }
        }
    }

    public abstract class SetExtraParamsNodeBase<T> : IPluginEvaluate where T : NotuiElement
    {
        [Input("Element In")]
        public Pin<NotuiElement> In;
        [Input("Set", IsBang = true)]
        public ISpread<bool> Set;

        [Output("Thru")]
        public ISpread<NotuiElement> Out;
        [Output("Is Valid")]
        public ISpread<bool> Valid;

        public abstract void SetExtraParameters(int i, T element);

        public void Evaluate(int SpreadMax)
        {
            if (In.IsConnected && In.SliceCount > 0 && In[0] != null)
            {
                Valid.SliceCount = Out.SliceCount = In.SliceCount;
                for (int i = 0; i < In.SliceCount; i++)
                {
                    Out[i] = In[i];
                    if (In[i] is T element)
                    {
                        if(Set.TryGetSlice(i)) SetExtraParameters(i, element);
                        Valid[i] = true;
                    }
                    else
                    {
                        Valid[i] = false;
                    }
                }
            }
            else
            {
                Valid.SliceCount = 1;
                Valid[0] = false;
                Out.SliceCount = 0;
            }
        }
    }

    public static class ElementNodeUtils
    {
        public const int ChangedFrames = 2;
    }

    public abstract class AbstractElementNode<TPrototype> : IPluginEvaluate where TPrototype : ElementPrototype
    {
        [Import] public IPluginHost2 PluginHost;
        [Import] public IHDEHost Host;

        protected abstract TPrototype ConstructPrototype(int i, string id);

        [Input("Set External", Visibility = PinVisibility.OnlyInspector)]
        public Pin<ElementPrototype> ExternalIn;

        [Input("Children")]
        public IDiffSpread<ISpread<ElementPrototype>> ChildrenIn;

        [Input("Name", DefaultString = "callmenames")]
        public IDiffSpread<string> NameIn;
        [Input("Manual Id", Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> ManIdIn;
        [Input("Id", DefaultString = "", Visibility = PinVisibility.Hidden)]
        public IDiffSpread<string> IdIn;

        [Input("Fade In Time")]
        public IDiffSpread<float> FadeInIn;
        [Input("Fade Out Time")]
        public IDiffSpread<float> FadeOutIn;
        [Input("Fade In Delay")]
        public IDiffSpread<float> FadeInDelIn;
        [Input("Fade Out Delay")]
        public IDiffSpread<float> FadeOutDelIn;
        [Input("Transformation Follow Time")]
        public IDiffSpread<float> TransFollowTimeIn;

        [Input("Behaviors")]
        public IDiffSpread<ISpread<InteractionBehavior>> BehaviorsIn;
        [Input("Transparent")]
        public IDiffSpread<bool> TransparentIn;
        [Input("Active", DefaultBoolean = true)]
        public IDiffSpread<bool> ActiveIn;
        [Input("Clip Hitting Through Parent")]
        public IDiffSpread<bool> ClipParentHittingIn;

        [Input("Attached Values", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<float>> AttValsIn;
        [Input("Attached Texts", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<string>> AttTextsIn;
        [Input("Attached Auxiliary Objects", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<IAuxiliaryObject>> AttAuxIn;
        [Input("Attached Auxiliary Keys", BinSize = 0, Visibility = PinVisibility.Hidden, BinVisibility = PinVisibility.Hidden)]
        public IDiffSpread<ISpread<string>> AttAuxKeysIn;

        [Input("Set Attached Values")]
        public IDiffSpread<bool> SetAttValsIn;

        [Input("Display Transform")]
        public IDiffSpread<VMatrix> DispTrIn;
        [Input("Transformation Update Mode", DefaultEnumEntry = "All")]
        public IDiffSpread<ISpread<ApplyTransformMode>> TrUpdateModeIn;

        [Input("Automatic Update", DefaultBoolean = true)]
        public IDiffSpread<bool> AutoUpdateIn;
        [Input("Force Update", IsBang = true)]
        public IDiffSpread<bool> ForceUpdateIn;

        [Input("Sub Context", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<SubContextOptions> SubContextIn;
        
        [Output("Element Prototype")]
        public ISpread<TPrototype> ElementProtOut;

        [Output("Element Id")]
        public ISpread<string> ElementIdOut;

        protected virtual void FillElementAuxData(TPrototype el, int i) { }
        protected virtual bool AuxDataChanged() { return false; }

        protected bool UseExternal(int i, out TPrototype exprot)
        {
            if (ExternalIn.IsConnected &&
                ExternalIn.SliceCount > 0 &&
                ExternalIn[i] != null &&
                ExternalIn[i] is TPrototype tprot)
            {
                exprot = tprot;
                return true;
            }
            exprot = null;
            return false;
        }

        protected TPrototype ProvidePrototype(int i, string id)
        {
            if (UseExternal(i, out var tprot))
            {
                return tprot;
            }

            return ConstructPrototype(i, id);
        }

        protected TPrototype FillElement(TPrototype el, int i, bool isnew)
        {
            DispTrIn[i].Decompose(out var scale, out Vector4D rotation, out var pos);
            el.Name = NameIn[i];
            el.FadeInTime = FadeInIn[i];
            el.FadeOutTime = FadeOutIn[i];
            el.Active = ActiveIn[i];
            el.Transparent = TransparentIn[i];
            if (BehaviorsIn[i].All(chel => chel != null))
            {
                el.Behaviors = BehaviorsIn[i].ToList();
            }
            el.DisplayTransformation.Position = pos.AsSystemVector();
            el.DisplayTransformation.Rotation = rotation.AsSystemQuaternion();
            el.DisplayTransformation.Scale = scale.AsSystemVector();
            el.TransformationFollowTime = TransFollowTimeIn[i];
            el.FadeInDelay = FadeInDelIn[i];
            el.FadeOutDelay = FadeOutDelIn[i];
            el.SubContextOptions = SubContextIn[i];
            el.OnlyHitIfParentIsHit = ClipParentHittingIn[i];
            var setvals = SetAttValsIn.TryGetSlice(i) || isnew;
            el.SetAttachedValues = setvals;

            if (setvals)
            {
                if(el.Value == null)
                    el.Value = new AttachedValues();
                el.Value.Values = AttValsIn[i].ToArray();
                el.Value.Texts = AttTextsIn[i].ToArray();

                el.Value.Auxiliary.Clear();
                for (int j = 0; j < AttAuxKeysIn[i].SliceCount; j++)
                {
                    if(string.IsNullOrWhiteSpace(AttAuxKeysIn[i].TryGetSlice(j))) continue;
                    el.Value.Auxiliary.UpdateGeneric(AttAuxKeysIn[i].TryGetSlice(j), AttAuxIn[i].TryGetSlice(j));
                }
            }

            var trupdmode = isnew ?
                ApplyTransformMode.All :
                TrUpdateModeIn[i].Aggregate(ApplyTransformMode.None, (current, mode) => current | mode);

            el.TransformApplication = trupdmode;
            el.Children.Clear();
            foreach (var child in ChildrenIn[i].Where(ch => ch != null))
            {
                child.Parent = el;
                el.Children.Add(child.Id, child);
            }
            FillElementAuxData(el, i);
            el.IsChanged = ElementNodeUtils.ChangedFrames;

            return el;
        }

        private int init = 0;

        private bool BinSizedSpreadChanged<T>(ISpread<ISpread<T>> spread)
        {
            if (spread.SliceCount == 0) return spread.IsChanged;
            if (spread.SliceCount > 1) return spread.IsChanged;
            return spread.IsChanged && spread[0].SliceCount > 0;
        }

        private bool AttachedChanged()
        {
            return BinSizedSpreadChanged(AttValsIn) || BinSizedSpreadChanged(AttTextsIn) ||
                   BinSizedSpreadChanged(AttAuxIn) || BinSizedSpreadChanged(AttAuxKeysIn);
        }

        public void Evaluate(int SpreadMax)
        {
            bool changewochildren = (SpreadUtils.AnyChanged(
                NameIn,
                ManIdIn,
                IdIn,
                FadeInIn,
                FadeOutIn,
                FadeInDelIn,
                FadeOutDelIn,
                TransFollowTimeIn,
                BehaviorsIn,
                TransparentIn,
                ActiveIn,
                DispTrIn,
                TrUpdateModeIn,
                ClipParentHittingIn,
                SubContextIn,
                SetAttValsIn
            ) || AttachedChanged() || AuxDataChanged()) && AutoUpdateIn.TryGetSlice(0) || ForceUpdateIn.TryGetSlice(0);

            bool childrenchanged = AutoUpdateIn.TryGetSlice(0) && ChildrenIn.IsChanged || ForceUpdateIn.TryGetSlice(0);

            bool changed = childrenchanged || changewochildren || SetAttValsIn.TryGetSlice(0);

            int sprmax = SpreadUtils.SpreadMax(
                ChildrenIn,
                NameIn,
                BehaviorsIn,
                DispTrIn,
                ActiveIn,
                AttAuxKeysIn,
                AttTextsIn,
                AttValsIn,
                IdIn
            );

            ElementProtOut.Stream.IsChanged = false;

            for (int i = 0; i < ElementProtOut.SliceCount; i++)
            {
                if(ElementProtOut[i] == null) continue;
                ElementProtOut[i].IsChanged--;
            }
            if (changed || init < 2)
            {
                if (ExternalIn.IsChanged)
                    ElementProtOut.SliceCount = 0;

                ElementProtOut.ResizeAndDismiss(sprmax, i => FillElement(ProvidePrototype(i, null), i, true));
                ElementIdOut.SliceCount = ElementProtOut.SliceCount;
                var gchildrenchanged = false;

                for (int i = 0; i < ElementProtOut.SliceCount; i++)
                {
                    var isnew = false;
                    if (ElementProtOut[i] == null)
                    {
                        ElementProtOut[i] = ProvidePrototype(i, ManIdIn[i] ? IdIn[i] : null);
                        isnew = true;
                    }

                    bool lchildrenchanged = false;
                    if (!changewochildren && childrenchanged)
                    {
                        if (ChildrenIn[i].SliceCount > 0)
                        {
                            for (int j = 0; j < ChildrenIn[i].SliceCount; j++)
                            {
                                if (ChildrenIn[i][j] == null) continue;
                                if (ChildrenIn[i][j].IsChanged > 0) lchildrenchanged = gchildrenchanged = true;
                                if (ElementProtOut[i].Children.ContainsKey(ChildrenIn[i][j].Id)) continue;
                                ChildrenIn[i][j].IsChanged = ElementNodeUtils.ChangedFrames;
                                lchildrenchanged = gchildrenchanged = true;
                            }
                        }
                        else
                        {
                            lchildrenchanged = ElementProtOut[i].Children.Count > 0;
                            gchildrenchanged = gchildrenchanged || lchildrenchanged;
                        }
                    }

                    if(changewochildren || lchildrenchanged)
                        FillElement(ElementProtOut[i], i, isnew || init < 2);

                    if (!UseExternal(i, out var tprot))
                    {
                        if (ManIdIn[i]) ElementProtOut[i].Id = IdIn[i];
                    }
                    ElementIdOut[i] = ElementProtOut[i].Id;
                }
                //FElementProt.Flush();
                if(changewochildren || gchildrenchanged)
                    ElementProtOut.Stream.IsChanged = true;
            }
            init++;
        }
    }
}

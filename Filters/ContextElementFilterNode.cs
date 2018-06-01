using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Coding;
using Notui;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Reflection;

namespace Notuiv.Filters
{
    public abstract class ContextElementFilterNode : IPluginEvaluate
    {
        [Input("Context")]
        public Pin<NotuiContext> FContext;

        private int _prevContextHash = 0;
        private NotuiContext _prevContext;

        [Input("Query")]
        public IDiffSpread<string> FQuery;
        
        [Output("Output")]
        public ISpread<ISpread<NotuiElement>> FOut;

        protected abstract void Filter(int i);

        private int _elementsChanged = 0;

        public void Evaluate(int SpreadMax)
        {
            FOut.Stream.IsChanged = false;
            if (FContext.IsConnected && FContext.SliceCount > 0 && FContext[0] != null && FQuery.SliceCount > 0)
            {
                var currContextHash = FContext[0].GetHashCode();
                FOut.SliceCount = FQuery.SliceCount;
                if (currContextHash != _prevContextHash)
                {
                    if (_prevContext != null)
                    {
                        _prevContext.OnElementsDeleted -= HandleElementChange;
                        _prevContext.OnElementsUpdated -= HandleElementChange;
                    }
                    FContext[0].OnElementsDeleted += HandleElementChange;
                    FContext[0].OnElementsUpdated += HandleElementChange;
                }
                if(FQuery.IsChanged) HandleElementChange(this, EventArgs.Empty);
                _prevContext = FContext[0];
                _prevContextHash = currContextHash;
            }
            else
            {
                FOut.SliceCount = 0;
                _prevContextHash = 0;
                if (_prevContext != null)
                {
                    _prevContext.OnElementsDeleted -= HandleElementChange;
                    _prevContext.OnElementsUpdated -= HandleElementChange;
                }
                _prevContext = null;
            }

            if (_elementsChanged > 0)
            {
                for (int i = 0; i < FQuery.SliceCount; i++)
                    Filter(i);
                FOut.Stream.IsChanged = true;
                _elementsChanged--;
            }
        }

        protected void HandleElementChange(object sender, EventArgs args)
        {
            _elementsChanged = 2;
        }
    }

    [PluginInfo(
        Name = "Sift",
        Category = "Notui.Context",
        Version = "String",
        Author = "microdee"
    )]
    public class StringContextElementFilterNode : ContextElementFilterNode, IPartImportsSatisfiedNotification
    {
        [Input("Query Flattened", Order = 100)]
        public IDiffSpread<bool> FQueryFlattened;
        [Input("Contains", Order = 101)]
        public IDiffSpread<bool> FContains;
        [Input("Use Name", Order = 102, DefaultBoolean = true)]
        public IDiffSpread<bool> FUseName;

        public void OnImportsSatisfied()
        {
            FQueryFlattened.Changed += spread => HandleElementChange(this, EventArgs.Empty);
            FContains.Changed += spread => HandleElementChange(this, EventArgs.Empty);
            FUseName.Changed += spread => HandleElementChange(this, EventArgs.Empty);
        }

        protected override void Filter(int i)
        {
            bool CompareType(NotuiElement el)
            {
                if (FUseName[i])
                    return FContains[i] ? el.Name.Contains(FQuery[i]) : el.Name == FQuery[i];
                return FContains[i] ? el.Id.Contains(FQuery[i]) : el.Id == FQuery[i];
            }

            if (string.IsNullOrWhiteSpace(FQuery[i]) || FQueryFlattened.SliceCount == 0 || FContains.SliceCount == 0 || FUseName.SliceCount == 0)
                FOut[i].SliceCount = 0;
            else
            {
                if(FOut.SliceCount == 0) return;
                FOut[i].AssignFrom(FQueryFlattened[i]
                    ? FContext[0].FlatElements.Where(CompareType)
                    : FContext[0].RootElements.Values.Where(CompareType));
            }
        }
    }

    [PluginInfo(
        Name = "Sift",
        Category = "Notui.Context",
        Version = "Type",
        Author = "microdee"
    )]
    public class TypeContextElementFilterNode : ContextElementFilterNode, IPartImportsSatisfiedNotification
    {
        [Input("Query Flattened", Order = 100)]
        public IDiffSpread<bool> FQueryFlattened;
        [Input("Contains", Order = 101)]
        public IDiffSpread<bool> FContains;

        public void OnImportsSatisfied()
        {
            FQueryFlattened.Changed += spread => HandleElementChange(this, EventArgs.Empty);
            FContains.Changed += spread => HandleElementChange(this, EventArgs.Empty);
        }

        protected override void Filter(int i)
        {
            bool CompareType(NotuiElement el)
            {
                if (el.EnvironmentObject is VEnvironmentData venvdat)
                    return FContains[i] ? venvdat.TypeCSharpName.Contains(FQuery[i]) : venvdat.TypeCSharpName == FQuery[i];
                return FContains[i] ? el.GetType().GetCSharpName().Contains(FQuery[i]) : el.GetType().GetCSharpName() == FQuery[i];
            }

            if (string.IsNullOrWhiteSpace(FQuery[i]) || FQueryFlattened.SliceCount == 0 || FContains.SliceCount == 0)
                FOut[i].SliceCount = 0;
            else
            {
                if (FOut.SliceCount == 0) return;
                FOut[i].AssignFrom(FQueryFlattened[i]
                    ? FContext[0].FlatElements.Where(CompareType)
                    : FContext[0].RootElements.Values.Where(CompareType));
            }
        }
    }

    [PluginInfo(
        Name = "Sift",
        Category = "Notui.Context",
        Version = "Opaq",
        Author = "microdee"
    )]
    public class OpaqContextElementFilterNode : ContextElementFilterNode, IPartImportsSatisfiedNotification
    {
        [Input("Separator", Order = 100, DefaultString = "/")]
        public IDiffSpread<string> FSeparator;

        [Input("Use Name", Order = 101, DefaultBoolean = true)]
        public IDiffSpread<bool> FUseName;

        public void OnImportsSatisfied()
        {
            FSeparator.Changed += spread => HandleElementChange(this, EventArgs.Empty);
            FUseName.Changed += spread => HandleElementChange(this, EventArgs.Empty);
        }

        protected override void Filter(int i)
        {
            if (string.IsNullOrWhiteSpace(FQuery[i]) || FUseName.SliceCount == 0 || FSeparator.SliceCount == 0)
                FOut[i].SliceCount = 0;
            else
            {
                if (FOut.SliceCount == 0) return;
                FOut[i].AssignFrom(FContext[i].Opaq(FQuery[i], FSeparator[i], FUseName[i]));
            }
        }
    }
}

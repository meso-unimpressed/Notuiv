using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notui;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Reflection;

namespace Notuiv.Filters
{
    public abstract class ElementFilterNode : IPluginEvaluate
    {
        [Input("Context")]
        public Pin<NotuiElement> FElement;
        
        private Spread<NotuiElement> _prevElements = new Spread<NotuiElement>();

        [Input("Query")]
        public IDiffSpread<ISpread<string>> FQuery;

        [Output("Output")]
        public ISpread<ISpread<NotuiElement>> FOut;

        protected abstract void Filter(int ei, int qi, NotuiElement element, string filter);

        private int _elementsChanged = 0;

        public void Evaluate(int SpreadMax)
        {
            //FOut.Stream.IsChanged = false;
            if (FQuery.IsChanged) HandleElementChange(this, EventArgs.Empty);
            if (FElement.IsConnected && FElement.SliceCount > 0 && FElement[0] != null && FQuery.SliceCount > 0)
            {
                FOut.SliceCount = FElement.SliceCount;
                int ii = 0;
                if(FElement.SliceCount != _prevElements.SliceCount) HandleElementChange(this, EventArgs.Empty);
                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    ii = i;
                    var prevValid = i < _prevElements.SliceCount;
                    if(FElement[i] == null)
                    {
                        if (prevValid && _prevElements[i] != null)
                        {
                            _prevElements[i].OnChildrenUpdated -= HandleElementChange;
                        }
                        continue;
                    }
                    if (!prevValid)
                    {
                        FElement[i].OnChildrenUpdated += HandleElementChange;
                        continue;
                    }

                    if (_prevElements[i] == null || FElement[i].GetHashCode() != _prevElements[i]?.GetHashCode())
                    {
                        if (_prevElements[i] != null)
                        {
                            _prevElements[i].OnChildrenUpdated -= HandleElementChange;
                        }
                        FElement[i].OnChildrenUpdated += HandleElementChange;
                    }
                }

                if(FElement.SliceCount < _prevElements.SliceCount)
                {
                    for (int i = ii; i < _prevElements.SliceCount; i++)
                    {
                        if (_prevElements[i] != null)
                        {
                            _prevElements[i].OnChildrenUpdated -= HandleElementChange;
                        }
                    }
                }
                _prevElements.AssignFrom(FElement);
            }
            else
            {
                FOut.SliceCount = 0;
                for (int i = 0; i < _prevElements.SliceCount; i++)
                {
                    if (_prevElements[i] != null)
                    {
                        _prevElements[i].OnChildrenUpdated -= HandleElementChange;
                    }
                }
            }

            if (_elementsChanged > 0)
            {
                FOut.SliceCount = FElement.SliceCount;
                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    FOut[i].SliceCount = 0;
                    for (int j = 0; j < FQuery[i].SliceCount; j++)
                    {
                        Filter(i, j, FElement[i], FQuery[i][j]);
                    }
                }
                //FOut.Stream.IsChanged = true;
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
        Category = "Notui.Element",
        Version = "String",
        Author = "microdee"
    )]
    public class StringElementFilterNode : ElementFilterNode, IPartImportsSatisfiedNotification
    {
        [Input("Contains", Order = 101)]
        public IDiffSpread<bool> FContains;
        [Input("Use Name", Order = 102, DefaultBoolean = true)]
        public IDiffSpread<bool> FUseName;

        public void OnImportsSatisfied()
        {
            FContains.Changed += spread => HandleElementChange(this, EventArgs.Empty);
            FUseName.Changed += spread => HandleElementChange(this, EventArgs.Empty);
        }

        protected override void Filter(int ei, int qi, NotuiElement element, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || FContains.SliceCount == 0 || FUseName.SliceCount == 0)
                return;
            else
            {
                if (FOut.SliceCount == 0) return;
                if (FUseName[ei])
                {
                    FOut[ei].AddRange(
                        element.Children.Values
                            .Where(
                                el => FContains[ei] ?
                                    el.Name.Contains(filter) :
                                    el.Name.Equals(filter, StringComparison.InvariantCulture)
                            )
                    );
                }
                else
                {
                    if (FContains[ei])
                    {
                        foreach (var child in element.Children.Keys)
                        {
                            if (child.Contains(filter)) FOut[ei].Add(element.Children[child]);
                        }
                    }
                    else
                    {
                        if (element.Children.ContainsKey(filter)) FOut[ei].Add(element.Children[filter]);
                    }
                }
            }
        }
    }

    [PluginInfo(
        Name = "Sift",
        Category = "Notui.Element",
        Version = "Type",
        Author = "microdee"
    )]
    public class TypeElementFilterNode : ElementFilterNode, IPartImportsSatisfiedNotification
    {
        [Input("Contains", Order = 101)]
        public IDiffSpread<bool> FContains;

        public void OnImportsSatisfied()
        {
            FContains.Changed += spread => HandleElementChange(this, EventArgs.Empty);
        }

        protected override void Filter(int ei, int qi, NotuiElement element, string filter)
        {
            bool CompareType(NotuiElement el)
            {
                if (el.EnvironmentObject is VEnvironmentData venvdat)
                    return FContains[ei] ? venvdat.TypeCSharpName.Contains(filter) : venvdat.TypeCSharpName == filter;
                return FContains[ei] ? el.GetType().GetCSharpName().Contains(filter) : el.GetType().GetCSharpName() == filter;
            }

            if (string.IsNullOrWhiteSpace(filter) ||  FContains.SliceCount == 0)
                return;
            else
            {
                if (FOut.SliceCount == 0) return;
                FOut[ei].AddRange(element.Children.Values.Where(CompareType));
            }
        }
    }

    [PluginInfo(
        Name = "Sift",
        Category = "Notui.Element",
        Version = "Opaq",
        Author = "microdee"
    )]
    public class OpaqElementFilterNode : ElementFilterNode, IPartImportsSatisfiedNotification
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

        protected override void Filter(int ei, int qi, NotuiElement element, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || FUseName.SliceCount == 0 || FSeparator.SliceCount == 0) return;
            else
            {
                if (FOut.SliceCount == 0) return;
                FOut[ei].AddRange(element.Opaq(filter, FSeparator[ei], FUseName[ei]));
            }
        }
    }
}

using System.Linq;
using Notui;
using VVVV.PluginInterfaces.V2;

namespace Notuiv
{
    [PluginInfo(
        Name = "GetAttachedValues",
        Category = "Notui.Element",
        Version = "Split",
        Author = "microdee"
    )]
    public class GetAttachedValuesNode : IPluginEvaluate
    {

        [Input("Element")] public Pin<NotuiElement> FElement;
        
        [Output("Values")] public ISpread<ISpread<float>> FVals;
        [Output("Texts")] public ISpread<ISpread<string>> FTexts;
        [Output("Auxiliary Keys")] public ISpread<ISpread<string>> FAuxKeys;
        [Output("Auxiliary Values")] public ISpread<ISpread<AuxiliaryObject>> FAuxVals;

        public void Evaluate(int SpreadMax)
        {
            if(!FElement.IsConnected) return;
            FVals.SliceCount = FTexts.SliceCount = FAuxKeys.SliceCount = FAuxVals.SliceCount = FElement.SliceCount;
            for (int i = 0; i < FElement.SliceCount; i++)
            {
                var element = FElement[i];
                if (element.Value == null)
                {
                    FVals[i].SliceCount = FTexts[i].SliceCount = FAuxKeys[i].SliceCount = FAuxVals.SliceCount = 0;
                }
                else
                {
                    FVals[i].AssignFrom(element.Value.Values);
                    FTexts[i].AssignFrom(element.Value.Texts);
                    FAuxKeys[i].AssignFrom(element.Value.Auxiliary.Keys);
                    FAuxVals[i].AssignFrom(element.Value.Auxiliary.Values);
                }
            }
        }
    }

    [PluginInfo(
        Name = "SetAttachedValues",
        Category = "Notui.Element",
        Version = "Join",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetAttachedValuesNode : IPluginEvaluate
    {

        [Input("Element")] public Pin<NotuiElement> FElement;

        [Input("Values")] public ISpread<ISpread<float>> FVals;
        [Input("Set Values", IsBang = true)] public ISpread<bool> FSetVals;
        [Input("Texts")] public ISpread<ISpread<string>> FTexts;
        [Input("Set Texts", IsBang = true)] public ISpread<bool> FSetTexts;

        [Output("Element Out")] public ISpread<NotuiElement> FElementOut;

        public void Evaluate(int SpreadMax)
        {
            if (!FElement.IsConnected) return;
            FElementOut.SliceCount = FElement.SliceCount;

            for (int i = 0; i < FElement.SliceCount; i++)
            {
                var element = FElement[i];
                if (element.Value == null)
                {
                    element.Value = new AttachedValues();
                }

                if (FSetVals[i])
                    element.Value.Values = FVals[i].ToArray();
                if (FSetTexts[i])
                    element.Value.Texts = FTexts[i].ToArray();
                FElementOut[i] = element;
            }
        }
    }

    [PluginInfo(
        Name = "SetAuxiliary",
        Category = "Notui.Element",
        Version = "Join",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class SetAuxiliaryNode : IPluginEvaluate
    {

        [Input("Element")] public Pin<NotuiElement> FElement;
        
        [Input("Keys")] public ISpread<ISpread<string>> FAuxKeys;
        [Input("Values")] public ISpread<ISpread<AuxiliaryObject>> FAuxVals;
        [Input("Set", IsBang = true)] public ISpread<bool> FSetAux;
        [Input("Remove", IsBang = true)] public ISpread<bool> FRemoveAux;
        [Input("Toggle", IsBang = true)] public ISpread<bool> FTogAux;

        [Output("Element Out")] public ISpread<NotuiElement> FElementOut;

        public void Evaluate(int SpreadMax)
        {
            if (!FElement.IsConnected) return;
            FElementOut.SliceCount = FElement.SliceCount;

            for (int i = 0; i < FElement.SliceCount; i++)
            {
                var element = FElement[i];
                if (element.Value == null)
                {
                    element.Value = new AttachedValues();
                }
                if (FSetAux[i])
                {
                    for (int j = 0; j < FAuxKeys[i].SliceCount; j++)
                    {
                        if (element.Value.Auxiliary.ContainsKey(FAuxKeys[i][j]))
                            element.Value.Auxiliary[FAuxKeys[i][j]] = FAuxVals[i][j];
                        else element.Value.Auxiliary.Add(FAuxKeys[i][j], FAuxVals[i][j]);
                    }
                }
                if (FRemoveAux[i])
                {
                    for (int j = 0; j < FAuxKeys[i].SliceCount; j++)
                    {
                        if (element.Value.Auxiliary.ContainsKey(FAuxKeys[i][j]))
                            element.Value.Auxiliary.Remove(FAuxKeys[i][j]);
                    }
                }
                if (FTogAux[i])
                {
                    for (int j = 0; j < FAuxKeys[i].SliceCount; j++)
                    {
                        if (element.Value.Auxiliary.ContainsKey(FAuxKeys[i][j]))
                            element.Value.Auxiliary.Remove(FAuxKeys[i][j]);
                        else element.Value.Auxiliary.Add(FAuxKeys[i][j], FAuxVals[i][j]);
                    }
                }
                FElementOut[i] = element;
            }
        }
    }

    [PluginInfo(
        Name = "GetAuxiliary",
        Category = "Notui.Element",
        Author = "microdee"
    )]
    public class GetAuxiliaryNode : IPluginEvaluate
    {

        [Input("Element")] public Pin<NotuiElement> FElement;
        [Input("Keys")] public ISpread<ISpread<string>> FAuxKeys;

        [Output("Keys Out")] public ISpread<ISpread<string>> FAuxKeysOut;
        [Output("Values")] public ISpread<ISpread<AuxiliaryObject>> FAuxVals;
        [Output("Found")] public ISpread<ISpread<bool>> FFound;

        private bool _prevconn = false;

        public void Evaluate(int SpreadMax)
        {
            var connframe = _prevconn != FElement.IsConnected;
            _prevconn = FElement.IsConnected;
            if (!FElement.IsConnected) return;
            if (FElement.SliceCount == 0 || FAuxKeys.SliceCount == 0)
            {
                FAuxKeysOut.SliceCount = FAuxVals.SliceCount = FFound.SliceCount = 0;
                return;
            }

            FAuxKeysOut.SliceCount = FAuxVals.SliceCount = FFound.SliceCount = FElement.SliceCount;
            
            for (int i = 0; i < FElement.SliceCount; i++)
            {
                if(FElement[i].Value != null)
                {
                    if(FElement[i].Value.Auxiliary.Count > 0)
                    {
                        var auxdir = FElement[i].Value.Auxiliary;
                        FAuxKeysOut[i].SliceCount = FFound[i].SliceCount = FAuxVals[i].SliceCount = FAuxKeys[i].SliceCount;
                        int jj = 0;
                        for (int j = 0; j < FAuxKeys[i].SliceCount; j++)
                        {
                            var contains = auxdir.ContainsKey(FAuxKeys[i][j]);
                            FFound[i][j] = contains;
                            if (contains)
                            {
                                FAuxKeysOut[i][jj] = FAuxKeys[i][j];
                                FAuxVals[i][jj] = auxdir[FAuxKeys[i][j]];
                            }
                            else
                            {
                                FAuxKeysOut[i].SliceCount = FAuxVals[i].SliceCount = FAuxVals[i].SliceCount - 1;
                                jj--;
                            }
                            jj++;
                        }
                    }
                    else
                    {
                        FAuxKeysOut[i].SliceCount = FFound[i].SliceCount = FAuxVals[i].SliceCount = 0;
                    }
                }
                else
                {
                    FAuxKeysOut[i].SliceCount = FFound[i].SliceCount = FAuxVals[i].SliceCount = 0;
                }
            }
        }
    }
}

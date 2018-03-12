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
        Author = "microdee"
    )]
    public class SetAttachedValuesNode : IPluginEvaluate
    {

        [Input("Element")] public Pin<NotuiElement> FElement;

        [Input("Values")] public ISpread<ISpread<float>> FVals;
        [Input("Set Values", IsBang = true)] public ISpread<bool> FSetVals;
        [Input("Texts")] public ISpread<ISpread<string>> FTexts;
        [Input("Set Texts", IsBang = true)] public ISpread<bool> FSetTexts;
        [Input("Auxiliary Keys")] public ISpread<ISpread<string>> FAuxKeys;
        [Input("Auxiliary Values")] public ISpread<ISpread<AuxiliaryObject>> FAuxVals;
        [Input("Set Auxiliary", IsBang = true)] public ISpread<bool> FSetAux;
        [Input("Remove Auxiliary", IsBang = true)] public ISpread<bool> FRemoveAux;
        [Input("Toggle Auxiliary", IsBang = true)] public ISpread<bool> FTogAux;

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
}

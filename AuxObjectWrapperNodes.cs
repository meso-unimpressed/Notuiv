using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using md.stdl.Coding;
using mp.pddn;
using Notui;
using VVVV.Hosting.Graph;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace Notuiv
{
    public class VAuxObject : AuxiliaryObject
    {
        public object Object { get; set; }

        public override AuxiliaryObject Copy()
        {
            return new VAuxObject { Object = Object };
        }

        public override void UpdateFrom(AuxiliaryObject other)
        {
            if (other is VAuxObject vaux)
                Object = vaux.Object;
        }
    }

    [PluginInfo(
        Name = "Wrap",
        Category = "Notui.Auxiliary",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class AuxObjectWrapNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;
        [Import] protected IHDEHost Hde;

        private ConfigurableTypePinGroup PinGroup;
        private bool _typeChanged;
        private bool _init = true;
        private DiffSpreadPin _input;

        [Output("Output")] public ISpread<AuxiliaryObject> FOut;

        public void OnImportsSatisfied()
        {
            PinGroup = new ConfigurableTypePinGroup(FPluginHost, FIOFactory, Hde.MainLoop, "Input");
            PinGroup.OnTypeChangeEnd += (sender, args) =>
            {
                _typeChanged = true;
                if (!_init) return;
                PinGroup.AddInput(new InputAttribute("Input"));
                _input = PinGroup.Pd.InputPins["Input"];
                _init = false;
            };
        }

        public void Evaluate(int SpreadMax)
        {
            var valid = _input != null;
            if (valid) valid = _input.Spread.SliceCount > 0;
            if (valid) valid = _input[0] != null;
            if (valid)
            {
                if (!_input.Spread.IsChanged && !_typeChanged) return;

                FOut.ResizeAndDismiss(_input.Spread.SliceCount, i => new VAuxObject { Object = _input[i] });
                for (int i = 0; i < _input.Spread.SliceCount; i++)
                {
                    var vaux = (VAuxObject) FOut[i];
                    vaux.Object = _input[i];
                }

                _typeChanged = false;
            }
            else
            {
                FOut.SliceCount = 0;
            }
        }
    }

    [PluginInfo(
        Name = "Unwrap",
        Category = "Notui.Auxiliary",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class AuxObjectUnwrapNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;
        [Import] protected IHDEHost Hde;

        private ConfigurableTypePinGroup PinGroup;
        private bool _typeChanged;
        private bool _init = true;
        private SpreadPin _output;

        [Input("Input")] public Pin<AuxiliaryObject> FIn;

        public void OnImportsSatisfied()
        {
            PinGroup = new ConfigurableTypePinGroup(FPluginHost, FIOFactory, Hde.MainLoop, "Output");
            PinGroup.OnTypeChangeEnd += (sender, args) =>
            {
                _typeChanged = true;
                if (!_init) return;
                PinGroup.AddOutputBinSized(new OutputAttribute("Output"));
                _output = PinGroup.Pd.OutputPins["Output"];
                _init = false;
            };
        }

        public void Evaluate(int SpreadMax)
        {
            var valid = FIn.IsConnected;
            if (valid) valid = _output != null;
            if (valid) valid = FIn.SliceCount > 0;
            if (valid)
            {
                if(!FIn.IsChanged && !_typeChanged) return;
                _output.Spread.SliceCount = FIn.SliceCount;
                for (int i = 0; i < FIn.SliceCount; i++)
                {
                    var cspread = (ISpread)_output[i];
                    if (FIn[i] is VAuxObject vaux)
                    {
                        if (vaux.Object.GetType().Is(PinGroup.GroupType))
                        {
                            cspread.SliceCount = 1;
                            cspread[0] = vaux.Object;
                        }
                        else cspread.SliceCount = 0;
                    }
                    else cspread.SliceCount = 0;
                }
                _typeChanged = false;
            }
            else
            {
                if(_output != null) _output.Spread.SliceCount = 0;
            }
        }
    }
}

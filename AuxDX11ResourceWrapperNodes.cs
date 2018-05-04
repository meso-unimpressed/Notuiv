using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using md.stdl.Coding;
using mp.pddn;
using Notui;
using VVVV.DX11;
using VVVV.Hosting.Graph;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace Notuiv
{
    public static class ConfigTypePinGroupDX11Extensions
    {
        public static void ConfigDX11TypeShortcuts(this ConfigurableTypePinGroup cpg)
        {
            var pgtd = cpg.SimplifiedTypeMapping;
            pgtd.Add("Texture1D", typeof(DX11Resource<DX11Texture1D>));
            pgtd.Add("Texture2D", typeof(DX11Resource<DX11Texture2D>));
            pgtd.Add("Texture3D", typeof(DX11Resource<DX11Texture3D>));
            pgtd.Add("TextureCube", typeof(DX11Resource<DX11TextureCube>));
            pgtd.Add("tex1", typeof(DX11Resource<DX11Texture1D>));
            pgtd.Add("tex2", typeof(DX11Resource<DX11Texture2D>));
            pgtd.Add("tex3", typeof(DX11Resource<DX11Texture3D>));
            pgtd.Add("texcube", typeof(DX11Resource<DX11TextureCube>));
            pgtd.Add("StructuredBuffer", typeof(DX11Resource<IDX11StructuredBuffer>));
            pgtd.Add("structbuf", typeof(DX11Resource<IDX11StructuredBuffer>));
            pgtd.Add("RawBuffer", typeof(DX11Resource<IDX11Buffer>));
            pgtd.Add("ByteAddressBuffer", typeof(DX11Resource<IDX11Buffer>));
            pgtd.Add("rawbuf", typeof(DX11Resource<IDX11Buffer>));
            pgtd.Add("Geometry", typeof(DX11Resource<IDX11Geometry>));
            pgtd.Add("geom", typeof(DX11Resource<IDX11Geometry>));
            pgtd.Add("RenderSemantic", typeof(DX11Resource<IDX11RenderSemantic>));
            pgtd.Add("semantic", typeof(DX11Resource<IDX11RenderSemantic>));
            cpg.OnlyAllowMappedTypes = true;
        }
    }

    [PluginInfo(
        Name = "WrapResource",
        Category = "Notui.Auxiliary.DX11",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class AuxDX11ResourceWrapNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDX11ResourceDataRetriever
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;
        [Import] protected IHDEHost Hde;

        private ConfigurableTypePinGroup PinGroup;
        private bool _typeChanged;
        private bool _init = true;
        private DiffSpreadPin _input;
        private bool _prevvalid = false;

        [Input("Flag Changed", IsBang = true)] public ISpread<bool> FFlagChanged;
        [Output("Output")] public ISpread<AuxiliaryObject> FOut;

        public void OnImportsSatisfied()
        {
            PinGroup = new ConfigurableTypePinGroup(FPluginHost, FIOFactory, Hde.MainLoop, "Input");
            PinGroup.ConfigDX11TypeShortcuts();

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
            FOut.Stream.IsChanged = false;
            var valid = _input != null;
            if (valid) valid = _input.Spread.SliceCount > 0;
            if (valid) valid = _input[0] != null;
            if (valid)
            {
                RenderRequest?.Invoke(this, FPluginHost);
                if (!_input.Spread.IsChanged && !_typeChanged && !FFlagChanged[0]) return;
                
                FOut.ResizeAndDismiss(_input.Spread.SliceCount, i => null);
                for (int i = 0; i < _input.Spread.SliceCount; i++)
                {
                    if (FOut[i] == null) FOut[i] = new VAuxObject {Object = _input[i]};
                    var vaux = (VAuxObject) FOut[i];
                    vaux.Object = _input[i];
                }

                _typeChanged = false;
                FOut.Stream.IsChanged = FFlagChanged[0];
            }
            else
            {
                FOut.Stream.IsChanged = _prevvalid;
                FOut.SliceCount = 0;
            }
            _prevvalid = valid;
        }

        public DX11RenderContext AssignedContext { get; set; }
        public event DX11RenderRequestDelegate RenderRequest;
    }

    [PluginInfo(
        Name = "UnwrapResource",
        Category = "Notui.Auxiliary.DX11",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class AuxDX11ResourceUnwrapNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDX11ResourceHost
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
            PinGroup.ConfigDX11TypeShortcuts();

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

        public Dictionary<DX11RenderContext, IDX11Resource> Data { get; }
        public void Update(DX11RenderContext context)
        { }

        public void Destroy(DX11RenderContext context, bool force)
        {
            //TODO: check if resource really have to be destroyed when they're only fetched from class
        }
    }
}

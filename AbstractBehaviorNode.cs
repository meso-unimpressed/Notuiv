using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Notui;
using mp.pddn;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace Notuiv
{
    public abstract class BehaviorInstanceInfoNode<TBehav, TState> : IPluginEvaluate
        where TBehav : InteractionBehavior where TState : IAuxiliaryObject
    {
        public const string BehaviorStatePrefix = "Internal.Behavior:";

        [Input("Element Instance")]
        public ISpread<NotuiElement> FElement;

        [Input("Behavior")]
        public ISpread<TBehav> FBehavior;

        [Output("BehaviorState")]
        public ISpread<ISpread<TState>> FState;

        public void Evaluate(int SpreadMax)
        {
            FState.SliceCount = FElement.SliceCount;
            for (int i = 0; i < FElement.SliceCount; i++)
            {
                FState[i].SliceCount = 0;
                if (FElement[i].Value == null)
                {
                    FState[i].SliceCount = 0;
                    continue;
                }
                if (!FElement[i].Value.Auxiliary.ContainsKey(BehaviorStatePrefix + FBehavior[0].Id))
                {
                    FState[i].SliceCount = 0;
                    continue;
                }

                FState[i].SliceCount = 1;
                try
                {
                    FState[i][0] = (TState)FElement[i].Value.Auxiliary[BehaviorStatePrefix + FBehavior[0].Id];
                }
                catch
                {
                    FState[i].SliceCount = 0;
                }
            }
        }
    }

    public abstract class AbstractBehaviorNode<T> : IPartImportsSatisfiedNotification, IPluginEvaluate
        where T: new()
    {
        [Output("Output")] public Pin<T> FOutput;

        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;

        protected bool ExposePrivate = false;
        protected T Default = new T();

        public virtual void OnImportsSatisfiedBegin() { }
        public virtual void OnImportsSatisfiedEnd() { }

        public virtual void OnEvaluateBegin() { }
        public virtual void OnEvaluateEnd() { }

        public virtual void OnChangedBegin() { }
        public virtual void OnChangedEnd() { }

        /// <summary>
        /// If an input property is IEnumerable, please specify how to clear it
        /// </summary>
        /// <param name="enumerable">extracted enumerable</param>
        /// <param name="member">Current property info</param>
        /// <param name="i">Current Slice</param>
        public virtual void ClearEnumerable(IEnumerable enumerable, PropertyInfo member, int i) { }

        /// <summary>
        /// If an input property is IEnumerable, please specify how to add objects to it
        /// </summary>
        /// <param name="enumerable">extracted enumerable</param>
        /// <param name="member">Current property info</param>
        /// <param name="obj">The object to be added to the IEnumerable</param>
        /// <param name="i">Current Slice</param>
        public virtual void AddToEnumerable(IEnumerable enumerable, object obj, PropertyInfo member, int i) { }

        /// <summary>
        /// Transform a property to a different value
        /// </summary>
        /// <param name="obj">Original value of the property</param>
        /// <param name="member">Property info</param>
        /// <param name="i">Current slice</param>
        /// <returns>The resulting transformed object</returns>
        public virtual object TransformInput(object obj, PropertyInfo member, int i)
        {
            return MiscExtensions.MapVVVVValueToSystemNumerics(obj);
        }

        /// <summary>
        /// Transform the default value of the property to a different value
        /// </summary>
        /// <param name="obj">Original value of the property</param>
        /// <param name="member">Property info</param>
        /// <param name="i">Current slice</param>
        /// <returns>The resulting transformed object</returns>
        public virtual object TransformDefaultValue(object obj, PropertyInfo member)
        {
            return MiscExtensions.MapSystemNumericsValueToVVVV(obj);
        }
        
        protected virtual double[] TransformDefaultToValues(object obj)
        {
            switch (obj)
            {
                case float v: return new[] { (double)v };
                case double v: return new[] { v };
                case short v: return new[] { (double)v };
                case int v: return new[] { (double)v };
                case long v: return new[] { (double)v };
                case ushort v: return new[] { (double)v };
                case uint v: return new[] { (double)v };
                case ulong v: return new[] { (double)v };
                case decimal v: return new[] { (double)v };
                case Vector2 v:
                {
                    return new double[] {v.X, v.Y};
                }
                case Vector3 v:
                {
                    return new double[] { v.X, v.Y, v.Z };
                }
                case Vector4 v:
                {
                    return new double[] { v.X, v.Y, v.Z, v.W };
                }
                case Quaternion v:
                {
                    return new double[] { v.X, v.Y, v.Z, v.W };
                }
                default: return new [] { 0.0 };
            }
        }

        /// <summary>
        /// Transform the type a property to a different one
        /// </summary>
        /// <param name="original">Original type of the property</param>
        /// <param name="member">Property info</param>
        /// <returns>The resulting transformed type</returns>
        public virtual Type TransformType(Type original, PropertyInfo member)
        {
            return MiscExtensions.MapSystemNumericsTypeToVVVV(original);
        }

        protected Dictionary<PropertyInfo, bool> IsMemberEnumerable = new Dictionary<PropertyInfo, bool>();
        protected Dictionary<PropertyInfo, bool> IsMemberDictionary = new Dictionary<PropertyInfo, bool>();

        protected Type CType;
        protected PinDictionary Pd;

        private void SetDefaultValue(PropertyInfo member, object defaultValue)
        {
            var defVals = TransformDefaultToValues(defaultValue);
            var attr = new InputAttribute(member.Name)
            {
                DefaultValue = defVals[0],
                DefaultValues = defVals,
                DefaultString = defaultValue?.ToString() ?? "",
                DefaultNodeValue = defaultValue
            };
            if (defaultValue is bool b) attr.DefaultBoolean = b;

            Pd.AddInput(TransformType(member.PropertyType, member), attr);
            var spread = Pd.InputPins[member.Name].Spread;
            spread.SliceCount = 1;
            spread[0] = TransformDefaultValue(defaultValue, member);
        }

        private void AddMemberPin(PropertyInfo member)
        {
            Type memberType = typeof(object);

            if (!member.CanRead) return;
            if (member.GetIndexParameters().Length > 0) return;

            memberType = member.PropertyType;

            var enumerable = false;
            var dictionary = false;
            var defaultValue = typeof(T).GetProperty(member.Name)?.GetValue(Default);
            if (memberType.GetInterface("IDictionary") != null)
            {
                try
                {
                    var interfaces = memberType.GetInterfaces().ToList();
                    interfaces.Add(memberType);
                    var stype = interfaces
                        .Where(type =>
                        {
                            try
                            {
                                var res = type.GetGenericTypeDefinition();
                                if (res == null) return false;
                                return res == typeof(IDictionary<,>);
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        })
                        .First().GenericTypeArguments;
                    Pd.AddInput(TransformType(stype[0], member), new InputAttribute(member.Name + " Keys"), binSized: true);
                    Pd.AddInput(TransformType(stype[1], member), new InputAttribute(member.Name + " Values"), binSized: true);
                    dictionary = true;
                }
                catch (Exception)
                {
                    Pd.AddInput(TransformType(memberType, member), new InputAttribute(member.Name));
                    dictionary = false;
                }
            }
            else if (memberType.GetInterface("IEnumerable") != null && memberType != typeof(string))
            {
                try
                {
                    var interfaces = memberType.GetInterfaces().ToList();
                    interfaces.Add(memberType);
                    var stype = interfaces
                        .Where(type =>
                        {
                            try
                            {
                                var res = type.GetGenericTypeDefinition();
                                if (res == null) return false;
                                return res == typeof(IEnumerable<>);
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        })
                        .First().GenericTypeArguments[0];
                    Pd.AddInput(TransformType(stype, member), new InputAttribute(member.Name), binSized: true);
                    enumerable = true;
                }
                catch (Exception)
                {
                    Pd.AddInput(TransformType(memberType, member), new InputAttribute(member.Name));
                    SetDefaultValue(member, defaultValue);
                    enumerable = false;
                }
            }
            else
            {
                SetDefaultValue(member, defaultValue);
                enumerable = false;
            }
            IsMemberEnumerable.Add(member, enumerable);
            IsMemberDictionary.Add(member, dictionary);
        }

        private void FillObject(PropertyInfo member, T behav, int i)
        {
            if (IsMemberDictionary[member])
            {
                if (!(member.GetValue(behav) is IDictionary dict))
                {
                    return;
                }
                var keyspread = (ISpread)Pd.InputPins[member.Name + " Keys"].Spread[i];
                var valuespread = (ISpread)Pd.InputPins[member.Name + " Values"].Spread[i];
                dict.Clear();
                for (int j = 0; j < keyspread.SliceCount; j++)
                {
                    dict.Add(keyspread[j], valuespread[j]);
                }
            }
            else if (IsMemberEnumerable[member])
            {
                if (!(member.GetValue(behav) is IEnumerable enumerable))
                {
                    return;
                }
                var spread = (ISpread)Pd.InputPins[member.Name].Spread[i];
                ClearEnumerable(enumerable, member, i);
                for (int j = 0; j < spread.SliceCount; j++)
                {
                    AddToEnumerable(enumerable, spread[j], member, i);
                }
            }
            else
            {
                member.SetValue(behav, TransformInput(Pd.InputPins[member.Name].Spread[i], member, i));
            }
        }

        public void OnImportsSatisfied()
        {
            Pd = new PinDictionary(FIOFactory);
            CType = typeof(T);

            OnImportsSatisfiedBegin();
            
            foreach (var prop in CType.GetProperties().Where(p => p.CanWrite))
                AddMemberPin(prop);

            OnImportsSatisfiedEnd();
        }

        public void Evaluate(int SpreadMax)
        {
            OnEvaluateBegin();
            if (Pd.InputSpreadMin == 0)
            {
                FOutput.SliceCount = 0;
                OnEvaluateEnd();
                return;
            }

            if (Pd.InputPins.Values.Select(p => p.Spread).Any(o => o == null))
            {
                OnEvaluateEnd();
                return;
            }

            FOutput.Stream.IsChanged = false;
            if (Pd.InputChanged)
            {
                OnChangedBegin();
                var sprmax = Pd.InputSpreadMax;
                FOutput.SliceCount = sprmax;
                for (int i = 0; i < sprmax; i++)
                {
                    if (FOutput[i] == null) FOutput[i] = new T();
                    foreach (var member in IsMemberEnumerable.Keys)
                        FillObject(member, FOutput[i], i);
                }

                FOutput.Stream.IsChanged = true;
                OnChangedEnd();
            }
            OnEvaluateEnd();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using mp.pddn;
using Notui;
using Notui.Behaviors;
using VVVV.PluginInterfaces.V2;

namespace Notuiv
{
    [PluginInfo(
        Name = "ValueSlider2D",
        Category = "Notui.Behavior",
        Version = "Join",
        Author = "microdee"
    )]
    public class ValueSliderBehaviorNode : AbstractBehaviorNode<ValueSlider2D> { }

    [PluginInfo(
        Name = "Sliding",
        Category = "Notui.Behavior",
        Version = "Join",
        Author = "microdee"
    )]
    public class SlidingBehaviorNode : AbstractBehaviorNode<SlidingBehavior>
    {
        /*
        public override Type TransformType(Type original, PropertyInfo member)
        {
            return original == typeof(Func<NotuiElement, IEnumerable<NotuiElement>>) ?
                typeof(string) :
                MiscExtensions.MapVVVVTypeToSystemNumerics(original);
        }

        public override object TransformInput(object obj, PropertyInfo member, int i)
        {
            if (member.Name == "SlideOnlyWithSpecificChildren")
            {
                if (string.IsNullOrWhiteSpace(obj?.ToString())) return null;
                return new Func<NotuiElement, IEnumerable<NotuiElement>>(element => element.Opaq(obj.ToString()));
            }
            return MiscExtensions.MapVVVVValueToSystemNumerics(obj);
        }
        */
    }

    [PluginInfo(
        Name = "SlidingInfo",
        Category = "Notui.Behavior",
        Version = "Split",
        Author = "microdee"
    )]
    public class SlidingBehaviorInfoNode : BehaviorInstanceInfoNode<SlidingBehavior, SlidingBehavior.BehaviorState> { }
}

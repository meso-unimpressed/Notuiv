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
    public class SlidingBehaviorNode : AbstractBehaviorNode<SlidingBehavior> { }

    [PluginInfo(
        Name = "SlidingInfo",
        Category = "Notui.Behavior",
        Version = "Split",
        Author = "microdee"
    )]
    public class SlidingBehaviorInfoNode : BehaviorInstanceInfoNode<SlidingBehavior, SlidingBehavior.BehaviorState> { }

    [PluginInfo(
        Name = "MouseWheelScroll",
        Category = "Notui.Behavior",
        Version = "Join",
        Author = "microdee"
    )]
    public class MouseWheelScrollBehaviorNode : AbstractBehaviorNode<MouseWheelScroll> { }

    [PluginInfo(
        Name = "MouseWheelScrollInfo",
        Category = "Notui.Behavior",
        Version = "Split",
        Author = "microdee"
    )]
    public class MouseWheelScrollBehaviorInfoNode : BehaviorInstanceInfoNode<MouseWheelScroll, MouseWheelScroll.BehaviorState> { }
}

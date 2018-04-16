using System;
using System.Collections.Generic;
using System.Linq;
using md.stdl.Interaction;
using md.stdl.Interfaces;
using Notui;
using md.stdl.Mathematics;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.Reflection;
using VVVV.Utils.VMath;

namespace Notuiv
{
    public class EventBangs : IUpdateable<EventBangs>
    {
        public bool OnInteractionBegin { get; set; }
        public bool OnInteractionEnd { get; set; }
        public bool OnTouchBegin { get; set; }
        public bool OnTouchEnd { get; set; }
        public bool OnHitBegin { get; set; }
        public bool OnHitEnd { get; set; }
        public bool OnInteracting { get; set; }
        public bool OnChildrenUpdated { get; set; }
        public bool OnDeletionStarted { get; set; }
        public bool OnDeleting { get; set; }
        public bool OnFadedIn { get; set; }
        public uint MouseButtonsPressed { get; set; }
        public int HorizontalWheel { get; set; }
        public int VerticalWheel { get; set; }

        public void UpdateFrom(EventBangs other)
        {
            OnInteractionBegin = other.OnInteractionBegin;
            OnInteractionEnd = other.OnInteractionEnd;
            OnTouchBegin = other.OnTouchBegin;
            OnTouchEnd = other.OnTouchEnd;
            OnHitBegin = other.OnHitBegin;
            OnHitEnd = other.OnHitEnd;
            OnInteracting = other.OnInteracting;
            OnChildrenUpdated = other.OnChildrenUpdated;
            OnDeletionStarted = other.OnDeletionStarted;
            OnDeleting = other.OnDeleting;
            OnFadedIn = other.OnFadedIn;

            MouseButtonsPressed = other.MouseButtonsPressed;
            HorizontalWheel = other.HorizontalWheel;
            VerticalWheel = other.VerticalWheel;
        }
    }
    public class ElementEventFlattener
    {
        public EventBangs CurrentFrame { get; } = new EventBangs();
        public EventBangs PreviousFrame { get; } = new EventBangs();

        private void _OnInteractionBeginHandler(object sender, TouchInteractionEventArgs e) => CurrentFrame.OnInteractionBegin = true;
        private void _OnInteractionEndHandler(object sender, TouchInteractionEventArgs e) => CurrentFrame.OnInteractionEnd = true;
        private void _OnTouchBeginHandler(object sender, TouchInteractionEventArgs e) => CurrentFrame.OnTouchBegin = true;
        private void _OnTouchEndHandler(object sender, TouchInteractionEventArgs e) => CurrentFrame.OnTouchEnd = true;
        private void _OnHitBeginHandler(object sender, TouchInteractionEventArgs e) => CurrentFrame.OnHitBegin = true;
        private void _OnHitEndHandler(object sender, TouchInteractionEventArgs e) => CurrentFrame.OnHitEnd = true;
        private void _OnInteractingHandler(object sender, EventArgs e) => CurrentFrame.OnInteracting = true;
        private void _OnChildrenUpdatedHandler(object sender, ChildrenUpdatedEventArgs e) => CurrentFrame.OnChildrenUpdated = true;
        private void _OnDeletionStartedHandler(object sender, EventArgs e) => CurrentFrame.OnDeletionStarted = true;
        private void _OnDeletingHandler(object sender, EventArgs e) => CurrentFrame.OnDeleting = true;
        private void _OnFadedInHandler(object sender, EventArgs e) => CurrentFrame.OnFadedIn = true;

        private void _OnMouseButtonPressed(object sender, MouseInteractionEventArgs e)
        {
            CurrentFrame.MouseButtonsPressed = CurrentFrame.MouseButtonsPressed | (uint)e.Buttons;
        }
        private void _OnMouseButtonReleased(object sender, MouseInteractionEventArgs e)
        {
            CurrentFrame.MouseButtonsPressed = CurrentFrame.MouseButtonsPressed & ~(uint)e.Buttons;
        }
        private void _OnHorizontalMouseWheelChange(object sender, MouseInteractionEventArgs e)
        {
            CurrentFrame.HorizontalWheel = e.Touch.MouseDelta.AccumulatedHorizontalWheelDelta;
        }
        private void _OnVerticalMouseWheelChange(object sender, MouseInteractionEventArgs e)
        {
            CurrentFrame.VerticalWheel = e.Touch.MouseDelta.AccumulatedWheelDelta;
        }

        public void Subscribe(NotuiElement element)
        {
            try
            {
                element.OnInteractionBegin -= _OnInteractionBeginHandler;
                element.OnInteractionEnd -= _OnInteractionEndHandler;
                element.OnTouchBegin -= _OnTouchBeginHandler;
                element.OnTouchEnd -= _OnTouchEndHandler;
                element.OnHitBegin -= _OnHitBeginHandler;
                element.OnHitEnd -= _OnHitEndHandler;
                element.OnInteracting -= _OnInteractingHandler;
                element.OnChildrenUpdated -= _OnChildrenUpdatedHandler;
                element.OnDeletionStarted -= _OnDeletionStartedHandler;
                element.OnDeleting -= _OnDeletingHandler;
                element.OnFadedIn -= _OnFadedInHandler;

                element.OnMouseButtonPressed -= _OnMouseButtonPressed;
                element.OnMouseButtonReleased -= _OnMouseButtonReleased;
                element.OnHorizontalMouseWheelChange -= _OnHorizontalMouseWheelChange;
                element.OnVerticalMouseWheelChange -= _OnVerticalMouseWheelChange;
            }
            catch { }

            element.OnInteractionBegin += _OnInteractionBeginHandler;
            element.OnInteractionEnd += _OnInteractionEndHandler;
            element.OnTouchBegin += _OnTouchBeginHandler;
            element.OnTouchEnd += _OnTouchEndHandler;
            element.OnHitBegin += _OnHitBeginHandler;
            element.OnHitEnd += _OnHitEndHandler;
            element.OnInteracting += _OnInteractingHandler;
            element.OnChildrenUpdated += _OnChildrenUpdatedHandler;
            element.OnDeletionStarted += _OnDeletionStartedHandler;
            element.OnDeleting += _OnDeletingHandler;
            element.OnFadedIn += _OnFadedInHandler;

            element.OnMouseButtonPressed += _OnMouseButtonPressed;
            element.OnMouseButtonReleased += _OnMouseButtonReleased;
            element.OnHorizontalMouseWheelChange += _OnHorizontalMouseWheelChange;
            element.OnVerticalMouseWheelChange += _OnVerticalMouseWheelChange;
        }

        public void Reset()
        {
            PreviousFrame.UpdateFrom(CurrentFrame);

            CurrentFrame.OnInteractionBegin = false;
            CurrentFrame.OnInteractionEnd = false;
            CurrentFrame.OnTouchBegin = false;
            CurrentFrame.OnTouchEnd = false;
            CurrentFrame.OnHitBegin = false;
            CurrentFrame.OnHitEnd = false;
            CurrentFrame.OnInteracting = false;
            CurrentFrame.OnChildrenUpdated = false;
            CurrentFrame.OnDeletionStarted = false;
            CurrentFrame.OnDeleting = false;
            CurrentFrame.OnFadedIn = false;

            CurrentFrame.HorizontalWheel = 0;
            CurrentFrame.VerticalWheel = 0;
        }

        private IHDEHost _hdeHost;

        public ElementEventFlattener(IHDEHost host)
        {
            _hdeHost = host;
            _hdeHost.MainLoop.OnPrepareGraph += (sender, args) => Reset();
        }
    }

    public class VEnvironmentData : AuxiliaryObject
    {
        public Dictionary<string, object> NodeSpecific { get; set; } = new Dictionary<string, object>();
        public ElementEventFlattener FlattenedEvents;

        public string TypeCSharpName { get; }
        public Spread<Touch> Touches { get; } = new Spread<Touch>();
        public Spread<bool> TouchesHitting { get; } = new Spread<bool>();
        public Spread<IntersectionPoint> TouchingIntersections { get; } = new Spread<IntersectionPoint>();
        public Spread<Touch> HittingTouches { get; } = new Spread<Touch>();
        public Spread<IntersectionPoint> HittingIntersections { get; } = new Spread<IntersectionPoint>();
        public Spread<Mouse> Mice { get; } = new Spread<Mouse>();
        public Spread<NotuiElement> Children { get; } = new Spread<NotuiElement>();
        public Spread<InteractionBehavior> Behaviors { get; } = new Spread<InteractionBehavior>();
        public Spread<NotuiElement> Parent { get; } = new Spread<NotuiElement>();
        public Matrix4x4 VInterTr { get; private set; }
        public Matrix4x4 VDispTr { get; private set; }

        private readonly NotuiElement _element;

        public VEnvironmentData(NotuiElement element)
        {
            _element = element;
            TypeCSharpName = element.GetType().GetCSharpName();
            element.OnMainLoopEnd += (sender, args) =>
            {
                Touches.AssignFrom(_element.Touching.Keys);
                TouchesHitting.AssignFrom(_element.Touching.Values.Select(t => t != null));
                TouchingIntersections.AssignFrom(_element.Touching.Values.Where(t => t != null));
                HittingTouches.AssignFrom(_element.Hitting.Keys);
                HittingIntersections.AssignFrom(_element.Hitting.Values);
                Mice.AssignFrom(_element.Mice.Select(t => t.AttachadMouse));
                Children.AssignFrom(_element.Children.Values);
                Behaviors.AssignFrom(_element.Behaviors);

                if (_element.Parent == null)
                    Parent.SliceCount = 0;
                else
                {
                    Parent.SliceCount = 1;
                    Parent[0] = element.Parent;
                }

                VInterTr = _element.InteractionMatrix.AsVMatrix4X4();
                VDispTr = _element.DisplayMatrix.AsVMatrix4X4();
            };
        }

        public override AuxiliaryObject Copy()
        {
            return new VEnvironmentData(_element);
        }

        public override void UpdateFrom(AuxiliaryObject other)
        {
            if (other is VEnvironmentData venvdat)
            {
                NodeSpecific = venvdat.NodeSpecific;
            }
        }
    }

    public static class NotuiUtils
    {
        public static void AttachManagementObject(this NotuiElement element, string nodepath, object obj)
        {
            if (element.EnvironmentObject == null)
                element.EnvironmentObject = new VEnvironmentData(element);
            if (element.EnvironmentObject is VEnvironmentData venvdat)
            {
                if (venvdat.NodeSpecific.ContainsKey(nodepath))
                {
                    venvdat.NodeSpecific[nodepath] = obj;
                }
                else
                {
                    venvdat.NodeSpecific.Add(nodepath, obj);
                }
            }
        }

        public static void AttachSliceId(this NotuiElement element, string nodepath, int id)
        {
            element.AttachManagementObject(nodepath, id);
        }
    }
}

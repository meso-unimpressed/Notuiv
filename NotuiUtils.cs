using System;
using System.Collections.Generic;
using System.Linq;
using md.stdl.Interaction;
using Notui;
using md.stdl.Mathematics;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Reflection;
using VVVV.Utils.VMath;

namespace Notuiv
{
    public class ElementEventFlattener
    {
        public bool OnInteractionBegin { get; private set; }
        public bool OnInteractionEnd { get; private set; }
        public bool OnTouchBegin { get; private set; }
        public bool OnTouchEnd { get; private set; }
        public bool OnHitBegin { get; private set; }
        public bool OnHitEnd { get; private set; }
        public bool OnInteracting { get; private set; }
        public bool OnChildrenUpdated { get; private set; }
        public bool OnDeletionStarted { get; private set; }
        public bool OnDeleting { get; private set; }
        public bool OnFadedIn { get; private set; }

        private void _OnInteractionBeginHandler(object sender, TouchInteractionEventArgs args) => OnInteractionBegin = true;
        private void _OnInteractionEndHandler(object sender, TouchInteractionEventArgs args) => OnInteractionEnd = true;
        private void _OnTouchBeginHandler(object sender, TouchInteractionEventArgs args) => OnTouchBegin = true;
        private void _OnTouchEndHandler(object sender, TouchInteractionEventArgs args) => OnTouchEnd = true;
        private void _OnHitBeginHandler(object sender, TouchInteractionEventArgs args) => OnHitBegin = true;
        private void _OnHitEndHandler(object sender, TouchInteractionEventArgs args) => OnHitEnd = true;
        private void _OnInteractingHandler(object sender, EventArgs args) => OnInteracting = true;
        private void _OnChildrenUpdatedHandler(object sender, ChildrenUpdatedEventArgs args) => OnChildrenUpdated = true;
        private void _OnDeletionStartedHandler(object sender, EventArgs args) => OnDeletionStarted = true;
        private void _OnDeletingHandler(object sender, EventArgs args) => OnDeleting = true;
        private void _OnFadedInHandler(object sender, EventArgs args) => OnFadedIn = true;

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
            } catch { }

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
        }

        public void Reset()
        {
            OnInteractionBegin = false;
            OnInteractionEnd = false;
            OnTouchBegin = false;
            OnTouchEnd = false;
            OnHitBegin = false;
            OnHitEnd = false;
            OnInteracting = false;
            OnChildrenUpdated = false;
            OnDeletionStarted = false;
            OnDeleting = false;
            OnFadedIn = false;
        }

        private IHDEHost _hdeHost;

        public ElementEventFlattener(IHDEHost host)
        {
            _hdeHost = host;
            _hdeHost.MainLoop.OnResetCache += (sender, args) => Reset();
        }
    }

    public class VEnvironmentData : AuxiliaryObject
    {
        public Dictionary<string, object> NodeSpecific { get; set; } = new Dictionary<string, object>();
        public ElementEventFlattener FlattenedEvents;

        public string TypeCSharpName { get; }
        public Spread<TouchContainer> Touches { get; } = new Spread<TouchContainer>();
        public Spread<bool> TouchesHitting { get; } = new Spread<bool>();
        public Spread<IntersectionPoint> TouchingIntersections { get; } = new Spread<IntersectionPoint>();
        public Spread<TouchContainer> HittingTouches { get; } = new Spread<TouchContainer>();
        public Spread<IntersectionPoint> HittingIntersections { get; } = new Spread<IntersectionPoint>();
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

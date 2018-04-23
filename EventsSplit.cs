using System.Windows.Forms;
using Notui;
using VVVV.PluginInterfaces.V2;

namespace Notuiv
{
    [PluginInfo(
        Name = "Events",
        Category = "Notui.Element",
        Author = "microdee"
    )]
    public class GuiElementEventsSplitNodePart : IPluginEvaluate
    {
        [Input("Element")] public Pin<NotuiElement> FElement;
        [Input("Previous Frame")] public ISpread<bool> FPrevFrame;

        [Output("On Interaction Begin", IsBang = true)] public ISpread<bool> FOnInteractionBegin;
        [Output("On Interaction End", IsBang = true)] public ISpread<bool> FOnInteractionEnd;
        [Output("On Touch Begin", IsBang = true)] public ISpread<bool> FOnTouchBegin;
        [Output("Touched")] public ISpread<bool> FTouched;
        [Output("On Touch End", IsBang = true)] public ISpread<bool> FOnTouchEnd;
        [Output("On Hit Begin", IsBang = true)] public ISpread<bool> FOnHitBegin;
        [Output("Hit")] public ISpread<bool> FHit;
        [Output("On Hit End", IsBang = true)] public ISpread<bool> FOnHitEnd;
        [Output("Interacting")] public ISpread<bool> FOnInteracting;
        [Output("On Children Added", IsBang = true)] public ISpread<bool> FOnChildrenUpdated;
        [Output("On Deletion Started", IsBang = true)] public ISpread<bool> FOnDeletionStarted;
        [Output("Deleting", IsBang = true)] public ISpread<bool> FOnDeleting;
        [Output("Faded In", IsBang = true)] public ISpread<bool> FOnFadedIn;

        public void Evaluate(int SpreadMax)
        {
            if (FElement.IsConnected && FElement.SliceCount > 0 && FElement[0] != null)
            {

                FOnInteractionBegin.SliceCount =
                FOnInteractionEnd.SliceCount =
                FOnTouchBegin.SliceCount =
                FOnTouchEnd.SliceCount =
                FOnHitBegin.SliceCount =
                FOnHitEnd.SliceCount =
                FOnInteracting.SliceCount =
                FOnChildrenUpdated.SliceCount =
                FOnDeleting.SliceCount =
                FOnDeletionStarted.SliceCount =
                FTouched.SliceCount =
                FHit.SliceCount =
                FOnFadedIn.SliceCount = FElement.SliceCount;

                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    var element = FElement[i];
                    FHit[i] = element.Hit;
                    FTouched[i] = element.Touched;
                    switch (element.EnvironmentObject)
                    {
                        case null:
                            continue;
                        case VEnvironmentData venvdat:
                            if(venvdat.FlattenedEvents == null) continue;

                            var events = FPrevFrame[0]
                                ? venvdat.FlattenedEvents.PreviousFrame
                                : venvdat.FlattenedEvents.CurrentFrame;

                            FOnInteractionBegin[i] = events.OnInteractionBegin;
                            FOnInteractionEnd[i] = events.OnInteractionEnd;
                            FOnTouchBegin[i] = events.OnTouchBegin;
                            FOnTouchEnd[i] = events.OnTouchEnd;
                            FOnHitBegin[i] = events.OnHitBegin;
                            FOnHitEnd[i] = events.OnHitEnd;
                            FOnInteracting[i] = events.OnInteracting;
                            FOnChildrenUpdated[i] = events.OnChildrenUpdated;
                            FOnDeleting[i] = events.OnDeleting;
                            FOnDeletionStarted[i] = events.OnDeletionStarted;
                            FOnFadedIn[i] = events.OnFadedIn;
                            break;
                    }
                }
            }
            else
            {
                FOnInteractionBegin.SliceCount =
                FOnInteractionEnd.SliceCount =
                FOnTouchBegin.SliceCount =
                FOnTouchEnd.SliceCount =
                FOnHitBegin.SliceCount =
                FOnHitEnd.SliceCount =
                FOnInteracting.SliceCount =
                FOnChildrenUpdated.SliceCount =
                FOnDeleting.SliceCount =
                FOnDeletionStarted.SliceCount =
                FTouched.SliceCount =
                FHit.SliceCount =
                FOnFadedIn.SliceCount = 0;
            }
        }
    }

    [PluginInfo(
        Name = "MouseEvents",
        Category = "Notui.Element",
        Author = "microdee"
    )]
    public class GuiElementMouseEventsSplitNodePart : IPluginEvaluate
    {
        [Input("Element")] public Pin<NotuiElement> FElement;
        [Input("Previous Frame")] public ISpread<bool> FPrevFrame;

        [Output("Left Button")] public ISpread<bool> FLmb;
        [Output("Middle Button")] public ISpread<bool> FMmb;
        [Output("Right Button")] public ISpread<bool> FRmb;
        [Output("X1 Button")] public ISpread<bool> FX1mb;
        [Output("X2 Button")] public ISpread<bool> FX2mb;
        [Output("Buttons")] public ISpread<uint> FButtons;

        [Output("Vertical Wheel")] public ISpread<int> FVScroll;
        [Output("Horizontal Wheel")] public ISpread<int> FHScroll;

        public void Evaluate(int SpreadMax)
        {
            if (FElement.IsConnected)
            {

                FLmb.SliceCount =
                FMmb.SliceCount =
                FRmb.SliceCount =
                FX2mb.SliceCount =
                FButtons.SliceCount =
                FVScroll.SliceCount =
                FHScroll.SliceCount =
                FX1mb.SliceCount = FElement.SliceCount;

                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    var element = FElement[i];
                    switch (element.EnvironmentObject)
                    {
                        case null:
                            continue;
                        case VEnvironmentData venvdat:

                            if (venvdat.FlattenedEvents == null) continue;
                            var events = FPrevFrame[0]
                                ? venvdat.FlattenedEvents.PreviousFrame
                                : venvdat.FlattenedEvents.CurrentFrame;

                            FLmb[i] = (events.MouseButtonsPressed & (uint) MouseButtons.Left) > 0;
                            FMmb[i] = (events.MouseButtonsPressed & (uint)MouseButtons.Middle) > 0;
                            FRmb[i] = (events.MouseButtonsPressed & (uint)MouseButtons.Right) > 0;
                            FX1mb[i] = (events.MouseButtonsPressed & (uint)MouseButtons.XButton1) > 0;
                            FX2mb[i] = (events.MouseButtonsPressed & (uint)MouseButtons.XButton2) > 0;
                            FButtons[i] = events.MouseButtonsPressed;
                            FVScroll[i] = events.VerticalWheel;
                            FHScroll[i] = events.HorizontalWheel;
                            break;
                    }
                }
            }
            else
            {
                FLmb.SliceCount =
                FMmb.SliceCount =
                FRmb.SliceCount =
                FX2mb.SliceCount =
                FButtons.SliceCount =
                FVScroll.SliceCount =
                FHScroll.SliceCount =
                FX1mb.SliceCount = 0;
            }
        }
    }
}

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
            if (FElement.IsConnected)
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
                            if (FPrevFrame[0])
                            {
                                FOnInteractionBegin[i] = venvdat.FlattenedEvents.PreviousFrame.OnInteractionBegin;
                                FOnInteractionEnd[i] = venvdat.FlattenedEvents.PreviousFrame.OnInteractionEnd;
                                FOnTouchBegin[i] = venvdat.FlattenedEvents.PreviousFrame.OnTouchBegin;
                                FOnTouchEnd[i] = venvdat.FlattenedEvents.PreviousFrame.OnTouchEnd;
                                FOnHitBegin[i] = venvdat.FlattenedEvents.PreviousFrame.OnHitBegin;
                                FOnHitEnd[i] = venvdat.FlattenedEvents.PreviousFrame.OnHitEnd;
                                FOnInteracting[i] = venvdat.FlattenedEvents.PreviousFrame.OnInteracting;
                                FOnChildrenUpdated[i] = venvdat.FlattenedEvents.PreviousFrame.OnChildrenUpdated;
                                FOnDeleting[i] = venvdat.FlattenedEvents.PreviousFrame.OnDeleting;
                                FOnDeletionStarted[i] = venvdat.FlattenedEvents.PreviousFrame.OnDeletionStarted;
                                FOnFadedIn[i] = venvdat.FlattenedEvents.PreviousFrame.OnFadedIn;
                            }
                            else
                            {
                                FOnInteractionBegin[i] = venvdat.FlattenedEvents.CurrentFrame.OnInteractionBegin;
                                FOnInteractionEnd[i] = venvdat.FlattenedEvents.CurrentFrame.OnInteractionEnd;
                                FOnTouchBegin[i] = venvdat.FlattenedEvents.CurrentFrame.OnTouchBegin;
                                FOnTouchEnd[i] = venvdat.FlattenedEvents.CurrentFrame.OnTouchEnd;
                                FOnHitBegin[i] = venvdat.FlattenedEvents.CurrentFrame.OnHitBegin;
                                FOnHitEnd[i] = venvdat.FlattenedEvents.CurrentFrame.OnHitEnd;
                                FOnInteracting[i] = venvdat.FlattenedEvents.CurrentFrame.OnInteracting;
                                FOnChildrenUpdated[i] = venvdat.FlattenedEvents.CurrentFrame.OnChildrenUpdated;
                                FOnDeleting[i] = venvdat.FlattenedEvents.CurrentFrame.OnDeleting;
                                FOnDeletionStarted[i] = venvdat.FlattenedEvents.CurrentFrame.OnDeletionStarted;
                                FOnFadedIn[i] = venvdat.FlattenedEvents.CurrentFrame.OnFadedIn;
                            }
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
}

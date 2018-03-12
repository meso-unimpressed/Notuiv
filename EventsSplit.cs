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

        [Output("On Interaction Begin", IsBang = true)] public ISpread<bool> FOnInteractionBegin;
        [Output("On Interaction End", IsBang = true)] public ISpread<bool> FOnInteractionEnd;
        [Output("On Touch Begin", IsBang = true)] public ISpread<bool> FOnTouchBegin;
        [Output("On Touch End", IsBang = true)] public ISpread<bool> FOnTouchEnd;
        [Output("On Hit Begin", IsBang = true)] public ISpread<bool> FOnHitBegin;
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
                FOnFadedIn.SliceCount = FElement.SliceCount;

                for (int i = 0; i < FElement.SliceCount; i++)
                {
                    var element = FElement[i];
                    switch (element.EnvironmentObject)
                    {
                        case null:
                            continue;
                        case VEnvironmentData venvdat:
                            if(venvdat.FlattenedEvents == null) continue;
                            FOnInteractionBegin[i] = venvdat.FlattenedEvents.OnInteractionBegin;
                            FOnInteractionEnd[i] = venvdat.FlattenedEvents.OnInteractionEnd;
                            FOnTouchBegin[i] = venvdat.FlattenedEvents.OnTouchBegin;
                            FOnTouchEnd[i] = venvdat.FlattenedEvents.OnTouchEnd;
                            FOnHitBegin[i] = venvdat.FlattenedEvents.OnHitBegin;
                            FOnHitEnd[i] = venvdat.FlattenedEvents.OnHitEnd;
                            FOnInteracting[i] = venvdat.FlattenedEvents.OnInteracting;
                            FOnChildrenUpdated[i] = venvdat.FlattenedEvents.OnChildrenUpdated;
                            FOnDeleting[i] = venvdat.FlattenedEvents.OnDeleting /* && element.Age.Elapsed.TotalSeconds > element.Context.DeltaTime * 2 */;
                            FOnDeletionStarted[i] = venvdat.FlattenedEvents.OnDeletionStarted;
                            FOnFadedIn[i] = venvdat.FlattenedEvents.OnFadedIn;
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
                FOnFadedIn.SliceCount = 0;
            }
        }
    }
}

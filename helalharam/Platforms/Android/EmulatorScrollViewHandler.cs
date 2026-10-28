using Android.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
namespace helalharam;

// The emulator can turn wheel input into a touch fling. Physical phones keep the standard handler.
internal sealed class EmulatorScrollViewHandler : ScrollViewHandler
{
    protected override MauiScrollView CreatePlatformView() => new BoundedScrollView(Context);
    private sealed class BoundedScrollView(Context context) : MauiScrollView(context)
    {
        public override void Fling(int velocityY)
        {
            // Emulator-only: stop when the wheel/touch drag ends, rather than adding momentum.
        }
    }
}

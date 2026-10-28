using Android.Views;
using Microsoft.Maui.Handlers;
namespace helalharam;

internal static class MouseWheelScrolling
{
    private static int configured;
    public static void Configure()
    {
        if(Interlocked.Exchange(ref configured,1)!=0)return;
        ViewHandler.ViewMapper.AppendToMapping("BoundedMouseWheel",(handler,view)=>
        {
            if(view is not ScrollView && view is not CollectionView)return;
            if(handler.PlatformView is Android.Views.View native)
                native.SetOnGenericMotionListener(new WheelListener());
        });
    }
    private sealed class WheelListener : Java.Lang.Object, Android.Views.View.IOnGenericMotionListener
    {
        public bool OnGenericMotion(Android.Views.View? view, MotionEvent? motion)
        {
            // Touch drags/flings never enter this branch; only an attached mouse wheel is bounded.
            if(view is null || motion?.Action!=MotionEventActions.Scroll || !motion.IsFromSource(Android.Views.InputSourceType.Mouse))return false;
            var delta=motion.GetAxisValue(Axis.Vscroll);
            if(!float.IsFinite(delta)||delta==0)return false;
            float density=view.Resources?.DisplayMetrics?.Density??1f;
            int distance=(int)Math.Round(-Math.Clamp(delta,-1f,1f)*24f*density);
            if(view is AndroidX.Core.Widget.NestedScrollView scroll){scroll.ScrollBy(0,distance);return true;}
            if(view is AndroidX.RecyclerView.Widget.RecyclerView list){list.ScrollBy(0,distance);return true;}
            return false;
        }
    }
}

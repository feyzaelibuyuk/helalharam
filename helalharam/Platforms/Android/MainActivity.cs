using Android.App;
using Android.Content.PM;
using Android.OS;

namespace helalharam
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private readonly EmulatorScrollGesture scrollGesture=new();
        public override bool DispatchTouchEvent(Android.Views.MotionEvent? motion)
        {
            using var adjusted=scrollGesture.Adjust(Window?.DecorView,motion);
            return base.DispatchTouchEvent(adjusted??motion);
        }

#if DEBUG
        public override bool DispatchGenericMotionEvent(Android.Views.MotionEvent? motion)
        {
            if(motion is not null && Android.Util.Log.IsLoggable("HelalScroll",Android.Util.LogPriority.Debug))
                Android.Util.Log.Debug("HelalScroll",$"generic action={motion.Action} source={motion.Source} v={motion.GetAxisValue(Android.Views.Axis.Vscroll)}");
            return base.DispatchGenericMotionEvent(motion);
        }

#endif
    }
}

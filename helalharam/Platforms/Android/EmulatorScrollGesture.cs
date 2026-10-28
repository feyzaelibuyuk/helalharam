using Android.Views;
namespace helalharam;

// Emulator wheel gestures arrive as touch drags. Never transform physical-device input.
internal sealed class EmulatorScrollGesture
{
    private bool active,vertical;
    private float downX,downY,slop;
    public MotionEvent? Adjust(Android.Views.View? root,MotionEvent? input)
    {
        if(input is null)return null;
        if(input.ActionMasked==MotionEventActions.Down)
        {
            active=DeviceInfo.Current.DeviceType==DeviceType.Virtual && input.PointerCount==1 && root is not null && FindTarget(root,(int)input.RawX,(int)input.RawY)==1;
            vertical=false;downX=input.GetX();downY=input.GetY();
            slop=root?.Context is Android.Content.Context context ? ViewConfiguration.Get(context)?.ScaledTouchSlop??8 : 8;
            return null;
        }
        if(!active)return null;
        if(input.PointerCount!=1){active=false;return null;}
        float dx=input.GetX()-downX,dy=input.GetY()-downY;
        if(!vertical && input.ActionMasked==MotionEventActions.Move && Math.Max(Math.Abs(dx),Math.Abs(dy))>slop)
        {
            if(Math.Abs(dx)>Math.Abs(dy)){active=false;return null;}
            vertical=true;
        }
        bool end=input.ActionMasked is MotionEventActions.Up or MotionEventActions.Cancel;
        if(end)active=false;
        if(!vertical)return null;
        // Preserve the native touch-slop boundary so a wheel drag cannot become a button click.
        float distance=Math.Abs(dy)<=slop ? Math.Abs(dy) : slop+(Math.Abs(dy)-slop)*0.2f;
        var adjusted=MotionEvent.Obtain(input);
        adjusted!.SetLocation(input.GetX(),downY+Math.Sign(dy)*distance);
        return adjusted;
    }
    private static int FindTarget(Android.Views.View view,int x,int y)
    {
        if(!view.IsShown)return 0;
        using var bounds=new Android.Graphics.Rect();
        if(!view.GetGlobalVisibleRect(bounds)||!bounds.Contains(x,y))return 0;
        // Text selection and web maps retain their own gestures.
        if(view is Android.Widget.EditText || view is Android.Webkit.WebView)return -1;
        if(view is ViewGroup group)
            for(int i=group.ChildCount-1;i>=0;i--){var child=group.GetChildAt(i);if(child is null)continue;int hit=FindTarget(child,x,y);if(hit!=0)return hit;}
        return view is AndroidX.Core.Widget.NestedScrollView || view is AndroidX.RecyclerView.Widget.RecyclerView ? 1 : 0;
    }
}

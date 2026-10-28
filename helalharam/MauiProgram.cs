using Plugin.Maui.OCR;
using ZXing.Net.Maui.Controls;
using Microsoft.Extensions.Logging;

namespace helalharam
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
#if ANDROID
            MouseWheelScrolling.Configure();
#endif
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .UseOcr()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

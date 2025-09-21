using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Logging;

namespace Biosero.TeachPendant.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton(SpeechToText.Default);

        return builder.Build();
	}
}

using ProduceNowApp.Services;
using Splat;

namespace ProduceNowApp.Desktop;

public class DesktopBootstrapper
{
    public DesktopBootstrapper(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // Call services.Register<T> and pass it lambda that creates instance of your service
        services.Register<SIPSorceryMedia.Abstractions.IVideoEncoder>(() => new SIPSorceryMedia.Encoders.VpxVideoEncoder());
    }
}
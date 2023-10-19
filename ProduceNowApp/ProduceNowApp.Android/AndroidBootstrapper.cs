using ProduceNowApp.Services;
using Splat;

namespace ProduceNowApp.Android;

public class AndroidBootstrapper
{
    public AndroidBootstrapper(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // Call services.Register<T> and pass it lambda that creates instance of your service
        
        services.Register<IVideoEncoderFactory>(() => new VideoEncoderFactory());
    }
}
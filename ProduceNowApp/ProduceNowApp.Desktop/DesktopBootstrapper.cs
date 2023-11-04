using System;
using Splat;

namespace ProduceNowApp.Desktop;

public class DesktopBootstrapper
{
    public DesktopBootstrapper(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        /*
         * First check, if we have ffmpeg installed 
         */
        ProduceNow.FFmpeg.Owner? ffmpegOwner = ProduceNow.FFmpeg.Owner.Instance;
        
        try
        {
            // Call services.Register<T> and pass it lambda that creates instance of your service
            services.Register<SIPSorceryMedia.Abstractions.IVideoEncoder>(
                () => new SIPSorceryMedia.Encoders.VpxVideoEncoder());
        }
        catch (Exception e)
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using Splat;

namespace ProduceNowApp.Desktop;

public class DesktopBootstrapper
{
    private Microsoft.Extensions.Logging.ILogger _logger = 
        ProduceNow.Common.ApplicationLogging.LoggerFactory.CreateLogger<DesktopBootstrapper>();
    public DesktopBootstrapper(
        IMutableDependencyResolver services,
        IReadonlyDependencyResolver resolver)
    {
        /*
         * Perform one-time ffmpeg initialization. 
         */
        ProduceNow.FFmpeg.Owner? ffmpegOwner = ProduceNow.FFmpeg.Owner.Instance;
        bool haveIt = false;

        if (!haveIt)
        {
            try
            {
                // Call services.Register<T> and pass it lambda that creates instance of your service
                services.Register<SIPSorceryMedia.Abstractions.IVideoEncoder>(
                    () => new SIPSorceryMedia.Encoders.VpxVideoEncoder());
                haveIt = true;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Unable to use libvpx encoder. Trying next.");
            }
        }

        if (!haveIt && null != ffmpegOwner)
        {
            try
            {
                services.Register<SIPSorceryMedia.Abstractions.IVideoEncoder>(
                    () => new FFmpegVideoEncoder(new Dictionary<string, string>()));
                haveIt = true;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Unable to instantiate ffmpeg video encoder: {e}.");
            }
        }

        if (!haveIt)
        {
            _logger.LogError("Unable to find an encoder implementation.");
            throw new ApplicationException("Unable to find an encoder implementation.");
        }

    }
}

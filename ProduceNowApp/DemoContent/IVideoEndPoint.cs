using SIPSorceryMedia.Abstractions;

namespace DemoContent;

public interface IVideoEndPoint
{
    public void ExternalVideoSourceRawSample(
        uint durationMilliseconds, 
        int width, int height, byte[] sample,
        VideoPixelFormatsEnum pixelFormat);
     

}
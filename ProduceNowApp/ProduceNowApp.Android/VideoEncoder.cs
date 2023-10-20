using System;
using System.Collections.Generic;
using Android.Media;
using Java.Nio;
using SIPSorceryMedia.Abstractions;

namespace ProduceNowApp.Android;

public class VideoEncoder : IVideoEncoder
{
    /**
     * Covers all the _mediaXxx objects
     */
    private object _lockCodec = new();
    private MediaCodec _mediaCodec = null;
    private MediaFormat _mediaFormat = null;
    private int _mediaFrameIndex = 0;
    private ByteBuffer[] _mediaInputBuffers = null;
    private ByteBuffer[] _mediaOutputBuffers = null;
    private MediaCodec.BufferInfo _mediaBufferInfo = null;
    
    /*
     * end of the media scope 
     */

    public List<VideoFormat> SupportedFormats { get; }

    
    public byte[] EncodeVideo(int width, int height, byte[] sample, VideoPixelFormatsEnum pixelFormat, VideoCodecsEnum codec)
    {
        throw new System.NotImplementedException();
    }

    
    public byte[] EncodeVideoFaster(RawImage rawImage, VideoCodecsEnum codec)
    {
        throw new System.NotImplementedException();
    }

    
    public void ForceKeyFrame()
    {
        return;
    }


    private void _closeFlush()
    {
        _mediaBufferInfo.Dispose();
        _mediaBufferInfo = null;
        
        _mediaCodec.Stop();
        _mediaCodec.Release();
        _mediaCodec.Dispose();
        _mediaCodec = null;
    }

    /**
     * Ensure we have a video decoder with the current parameters.
     */
    private void _needVideoDecoder()
    {
        lock (_lockCodec)
        {
            if (null != _mediaCodec)
            {
                /*
                 * Look, if the parameters match.
                 * If they do, just return.
                 * If they don't, flush the previous data, then create a new.
                 */
                // TXWTODO: Actually do that.
                return;
            }
            if (null == _mediaCodec)
            {
                _mediaCodec = MediaCodec.CreateDecoderByType(MimeVp8);
                _mediaFormat = MediaFormat.CreateVideoFormat(MimeVp8, 640, 480);
                _mediaCodec.Configure(_mediaFormat, null, null, 0);
                _mediaCodec.SetParameter(MediaCodec.ParameterKeyLowLatency);
                _mediaCodec.Start();
                _mediaBufferInfo = new();
                _mediaFrameIndex = 0;
                
                _mediaInputBuffers = _mediaCodec.GetInputBuffers();
                _mediaOutputBuffers = _mediaCodec.GetOutputBuffers();
            }
        }
    }
    
    private static long DefaultTimeoutUs = 5000;
    
    public IEnumerable<VideoSample> DecodeVideo(byte[] encodedSample, VideoPixelFormatsEnum pixelFormat, VideoCodecsEnum codec)
    {
        List<VideoSample> listFrames = new();
        _needVideoDecoder();
        bool sawOutputEOS = false;
        bool sawInputEOS = false;
        bool haveMoreInput = true;
        Console.WriteLine($"Decode media is called.");
        while (!sawOutputEOS)
        {
            if (!sawInputEOS && haveMoreInput)
            {
                int inputBufIndex = _mediaCodec.DequeueInputBuffer(DefaultTimeoutUs);
                if (inputBufIndex >= 0)
                {
                    _mediaInputBuffers[inputBufIndex].Clear();
                    _mediaInputBuffers[inputBufIndex].Put(encodedSample);
                    _mediaInputBuffers[inputBufIndex].Rewind();
                    haveMoreInput = false;
                    Console.WriteLine($"Decoding frameIndex {_mediaFrameIndex}");
                    try
                    {
                        _mediaCodec.QueueInputBuffer(
                            inputBufIndex,
                            0,
                            encodedSample.Length,
                            _mediaFrameIndex,
                            sawInputEOS ? MediaCodecBufferFlags.EndOfStream : 0
                        );
                        Console.WriteLine($"successfully enqueued.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception decoding frame {_mediaFrameIndex}: {e}");
                    }

                    _mediaFrameIndex++;
                }
            }

            int result = _mediaCodec.DequeueOutputBuffer(_mediaBufferInfo, DefaultTimeoutUs);
            Console.WriteLine($"result is {result}.");
            if (result >= 0)
            {
                int outputBufIndex = result;
                var outputBuffer = _mediaOutputBuffers[outputBufIndex];
                if (outputBuffer != null)
                {
                    Console.WriteLine($"Have output buffer.");
                    byte[] outputBytes = new byte[outputBuffer.Capacity()];
                    outputBuffer.Get(outputBytes);
                    listFrames.Add(new()
                    {
                        Width = 640, Height = 480, Sample = outputBytes
                    });
                    if ((_mediaBufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                    {
                        sawOutputEOS = true;
                    }

                    _mediaCodec.ReleaseOutputBuffer(outputBufIndex, false);
                }
            }
            else if (result == (int)MediaCodecInfoState.OutputBuffersChanged)
            {
                Console.WriteLine("result is OutputBufferChanged");
                _mediaOutputBuffers = _mediaCodec.GetOutputBuffers();
            }
            else if (result == (int)MediaCodecInfoState.TryAgainLater)
            {
                Console.WriteLine("result is TryAgainLater");
                break;
            } else
            {
                if (sawInputEOS || !haveMoreInput)
                {
                    break;
                }
            }

        }

        return listFrames;
    }

    
    public IEnumerable<RawImage> DecodeVideoFaster(byte[] encodedSample, VideoPixelFormatsEnum pixelFormat, VideoCodecsEnum codec)
    {
        throw new System.NotImplementedException();
    }

    
    public void Dispose()
    {
        _mediaCodec.Dispose();
    }
    
    private static string MimeVp8 = "video/x-vnd.on2.vp8";


    public VideoEncoder()
    {
        SupportedFormats = new();
        SupportedFormats.Add(new VideoFormat(VideoCodecsEnum.VP8, 96, 90000));
    }
}
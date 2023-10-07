
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DemoSignalServer.Models;

public class RTCSignalContext : DbContext
{
    public RTCSignalContext(DbContextOptions<RTCSignalContext> options)
        : base(options)
    {
    }
    public virtual DbSet<WebRTCSignal> WebRTCSignals { get; set; }
    
}
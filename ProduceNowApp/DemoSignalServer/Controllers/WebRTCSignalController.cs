using DemoSignalServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPSorcery.Net;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace DemoSignalServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebRTCSignalController : ControllerBase
{
    private readonly Models.RTCSignalContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<WebRTCSignalController> _logger;

    public int SdpSignallingTimeoutMs
    {
        get;
        set;
    } = 5000;

    public WebRTCSignalController(
        Models.RTCSignalContext context,
        IConfiguration config,
        ILogger<WebRTCSignalController> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of the "callers" that have one or more current WebRTC signals pending 
    /// for the "to" identity.
    /// </summary>
    /// <param name="to">The "callee" to get the list of callers for.</param>
    /// <returns>A list of callers that have one or more pending signals for the callee.</returns>
    //[HttpGet]
    //public async Task<ActionResult<IEnumerable<string>>> GetCallers(string to)
    //{
    //    if (string.IsNullOrEmpty(to))
    //    {
    //        return BadRequest();
    //    }

    //    return await _context.WebRTCSignals.Where(x => x.To.ToLower() == to.ToLower())
    //            .OrderBy(x => x.From)
    //            .Select(x => x.From)
    //            .Distinct()
    //            .ToListAsync();
    //}

    /// <summary>
    /// Gets a list of the WebRTC signals for a single call. The signals are for the "to" 
    /// identity and supplied by the "from" identity.
    /// </summary>
    /// <param name="to">The identity to get the WebRTC signals for.</param>
    /// <param name="from">The identity to get the WebRTC signals from.</param>
    /// <param name="type">Optional. A string to filter the types of signals to return.</param>
    /// <returns>A list of </returns>
    /// <example>
    /// $.ajax({
    ///     url: 'https://localhost:5001/us/them`,
    ///     type: 'GET',
    ///     success: onSuccess,
    ///     error: onError
    /// });
    /// </example>
    [HttpGet("{to}/{from}/{type=any}")]
    public async Task<ActionResult<string>> GetSignalsForCaller(string to, string from, WebRTCSignalTypesEnum type = WebRTCSignalTypesEnum.any)
    {
        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(from))
        {
            return BadRequest();
        }
        _logger?.LogInformation($"GetSignalsForCaller to={to}, from={from}");

        /*
         * First update all entries that we have registered (mark the LastQueriedAt)
         */
        await MarkMyPuts(to);

        /*
         * Expire the timed out clients
         */
        await ExpireTimedout();

        var query = _context.WebRTCSignals.Where(x =>
            x.To.ToLower() == to.ToLower() &&
            x.From.ToLower() == from.ToLower() &&
            x.DeliveredAt == null);

        if (type != WebRTCSignalTypesEnum.any)
        {
            query = query.Where(x => x.SignalType == type.ToString());
        }

        var nextSignal = await query
            .OrderBy(x => x.Inserted)
            .FirstOrDefaultAsync();

        if (nextSignal != null)
        {
            nextSignal.DeliveredAt = DateTime.UtcNow.ToString("o");
            await _context.SaveChangesAsync();

            return nextSignal.Signal;
        }
        else
        {
            return NoContent();
        }
    }

    /// <summary>
    /// Inserts a new WebRTC signal message.
    /// </summary>
    /// <param name="from">The source identity for the signal message.</param>
    /// <param name="to">The destination identity for the signal message.</param>
    /// <param name="sdp">The JSON formatted SDP offer or answer message.</param>
    /// <example>
    /// pc = new RTCPeerConnection();
    /// sdpOfferInit = await pc.createOffer();
    /// $.ajax({
    ///        url: 'https://localhost:5001/sdp/us/them',
    ///        type: 'PUT',
    ///        contentType: 'application/json',
    ///        data: JSON.stringify(pc.localDescription),
    ///        success: onSuccess,
    ///        error: onError
    ///   });
    /// </example>
    [HttpPut("sdp/{from}/{to}")]
    public async Task<IActionResult> Put(string from, string to, [FromBody] RTCSessionDescriptionInit sdp)
    {
        _logger?.LogInformation($"Put to={to}, from={from}");

        if (sdp != null)
        {
            Console.WriteLine($"sdp is {sdp}");
        }
        else
        {
            Console.WriteLine($"sdp is null.");
        }
        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(from) || sdp == null || sdp.sdp == null)
        {
            _logger.LogWarning($"WebRTC signal controller PUT sdp request had invalid parameters.");
            return BadRequest();
        }

        if (sdp.type == RTCSdpType.offer)
        {
            await ExpireExisting(from, to);
        }

        string nowString = DateTime.UtcNow.ToString("o");
        Models.WebRTCSignal sdpSignal = new Models.WebRTCSignal
        {
            ID = Guid.NewGuid().ToString(),
            To = to,
            From = from,
            SignalType = WebRTCSignalTypesEnum.sdp.ToString(),
            Signal = sdp.toJSON(),
            Inserted = nowString,
            LastQueriedAt = nowString
        };

        _context.WebRTCSignals.Add(sdpSignal);

        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Inserts a new WebRTC signal message.
    /// </summary>
    /// <param name="from">The source identity for the signal message.</param>
    /// <param name="to">The destination identity for the signal message.</param>
    /// <param name="ice">The JSON formatted ICE candidate.</param>
    /// <example>
    /// pc = new RTCPeerConnection();
    /// pc.onicecandidate = evt => {
    ///   evt.candidate && 
    ///   $.ajax({
    ///        url: 'https://localhost:5001/ice/us/them',
    ///        type: 'PUT',
    ///        contentType: 'application/json',
    ///        data: JSON.stringify(evt.candidate),
    ///        success: onSuccess,
    ///        error: onError
    ///   });
    /// };
    /// </example>
    [HttpPut("ice/{from}/{to}")]
    public async Task<IActionResult> PutIce(string from, string to, [FromBody] RTCIceCandidateInit ice)
    {
        _logger?.LogInformation($"PutIce to={to}, from={from}");

        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(from) || ice == null || ice.candidate == null)
        {
            _logger.LogWarning($"WebRTC signal controller PUT ice candidate request had invalid parameters.");
            return BadRequest();
        }

        string nowString = DateTime.UtcNow.ToString("o");
        WebRTCSignal iceSignal = new WebRTCSignal
        {
            ID = Guid.NewGuid().ToString(),
            To = to,
            From = from,
            SignalType = WebRTCSignalTypesEnum.ice.ToString(),
            Signal = ice.toJSON(),
            Inserted = nowString,
            LastQueriedAt = nowString
        };

        _context.WebRTCSignals.Add(iceSignal);

        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Removes any pending WebRTC signal messages for a WebRTC source and destination. The idea is that
    /// a new SDP offer invalidates any previous SDP and ICE messages.
    /// </summary>
    /// <param name="from">The identity of the peer that set the SDP offer or answer.</param>
    /// <param name="to">>The identity of the destination peer for the SDP offer or answer.</param>
    private async Task ExpireExisting(string from, string to)
    {
        var existing = await _context.WebRTCSignals.Where(x =>
            (from.ToLower() == x.From.ToLower() && to.ToLower() == x.To.ToLower()) ||
             (to.ToLower() == x.From.ToLower() && from.ToLower() == x.To.ToLower()))
           .ToArrayAsync();

        await _dumpAll();

        if (existing?.Length > 0)
        {
            string nowString = DateTime.UtcNow.ToString("o");
            foreach (var dup in existing)
            {
                _logger.LogInformation($"@{nowString}: Removing dup from {dup.From} to {dup.To}");
            }
            _context.RemoveRange(existing);
            await _context.SaveChangesAsync();
        }
    }


    private async Task MarkMyPuts(string fromWhom)
    {
        // DateTime oldest = DateTime.Now - TimeSpan.FromMilliseconds(SdpSignallingTimeoutMs);

        var mine = await _context.WebRTCSignals.Where(x =>
            (fromWhom.ToLower() == x.From.ToLower())
            ).ToArrayAsync();

        await _dumpAll();

        if (mine?.Length == 0)
        {
            return;
        }

        string nowString = DateTime.UtcNow.ToString("o");
        foreach (var my in mine)
        {
            _logger.LogInformation($"@{nowString}: Marking mine from {my.From} to {my.To} last @{my.LastQueriedAt}");
            my.LastQueriedAt = nowString;
        }
        await _context.SaveChangesAsync();
    }


    private async Task _dumpAll()
    {
        string nowString = DateTime.UtcNow.ToString("o");
        var all = await _context.WebRTCSignals.ToArrayAsync();
        foreach (var entry in all)
        {
            _logger.LogInformation($"@{nowString}: Have from {entry.From} to {entry.To} last @{entry.LastQueriedAt}");
        }
    }


    /// <summary>
    ///  Remove everything with an age older than
    /// </summary>
    private async Task ExpireTimedout()
    {
        string nowString = DateTime.UtcNow.ToString("o");
        DateTime oldest = DateTime.UtcNow - TimeSpan.FromMilliseconds(SdpSignallingTimeoutMs);

        _logger.LogInformation($"@{nowString}: oldest: {oldest.ToString("o")}");
        var existing = await _context.WebRTCSignals.Where(x =>
                (DateTime.Parse(x.LastQueriedAt) < oldest))
            .ToArrayAsync();

        await _dumpAll();
        if (existing?.Length > 0)
        {
            foreach(var old in existing)
            {
                _logger.LogInformation($"@{nowString}: About to remove old from {old.From} to {old.To}");
            }
            _context.RemoveRange(existing);
            await _context.SaveChangesAsync();
        }
    }
}
using DemoSignalServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPSorcery.Net;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;

namespace DemoSignalServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebRTCSignalController : ControllerBase
{
    private readonly Models.RTCSignalContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<WebRTCSignalController> _logger;
    private readonly IServiceProvider _serviceProvider;

    public int SdpSignallingTimeoutMs
    {
        get;
        set;
    } = 5000;

    public WebRTCSignalController(
        Models.RTCSignalContext context,
        IConfiguration config,
        ILogger<WebRTCSignalController> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }


    private async Task _repeatUntilDone(Func<Task> call)
    {
        while (true)
        {
            try
            {
                await call();
                break;
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogInformation("Repeating database call due to concurrency exception.");
            }
        }
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
    public async Task<ActionResult<string>> GetSignalsForCaller(string to, string from,
        WebRTCSignalTypesEnum type = WebRTCSignalTypesEnum.any)
    {
        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(from))
        {
            return BadRequest();
        }

        _logger?.LogInformation($"GetSignalsForCaller to={to}, from={from}");

        //var transientContext = _serviceProvider.GetRequiredService<RTCSignalContext>();

        /*
         * First update all entries that we have registered (mark the LastQueriedAt)
         */
        await _repeatUntilDone(async () => await MarkMyPuts(to));

        /*
         * Expire the timed out clients
         */
        await _repeatUntilDone(async () => await ExpireTimedout());

        while (true)
        {
            try
            {
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
                    _context.Update(nextSignal);
                    await _context.SaveChangesAsync();
                    return nextSignal.Signal;
                }
                else
                {
                    return NoContent();
                }
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogInformation("Repeating query due to concurrency.");
            }
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
        _logger?.LogInformation($"Put to={to}, from={from}, sdp: {sdp.sdp}");

        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(from) || sdp == null || sdp.sdp == null)
        {
            _logger.LogWarning($"WebRTC signal controller PUT sdp request had invalid parameters.");
            return BadRequest();
        }

        if (sdp.type == RTCSdpType.offer)
        {
            await _repeatUntilDone(async () => await ExpireExisting(from, to));
        }

        while (true)
        {
            try
            {
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

                await _context.WebRTCSignals.AddAsync(sdpSignal);
                await _context.SaveChangesAsync();
                break;
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogInformation("Repeating query due to concurrency.");
            }
        }

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

        while (true)
        {
            try
            {

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

                await _context.WebRTCSignals.AddAsync(iceSignal);
                await _context.SaveChangesAsync();
                break;

            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogInformation($"Updated due to concurrency.");
            }
        }

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

        if (existing?.Length > 0)
        {
            string nowString = DateTime.UtcNow.ToString("o");
            _context.RemoveRange(existing);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError($"Rerunning due to concurrency problems.");
            }
        }
    }


    private async Task MarkMyPuts(string fromWhom)
    {
        var mine = await _context.WebRTCSignals.Where(x =>
            (fromWhom.ToLower() == x.From.ToLower())
        ).ToArrayAsync();

        if (mine?.Length == 0)
        {
            return;
        }

        string nowString = DateTime.UtcNow.ToString("o");
        bool didModify = false;
        foreach (var my in mine)
        {
            my.LastQueriedAt = nowString;
            _context.Update(my);
            didModify = true;
        }

        if (didModify)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError($"Error while persisting changes: {e}");
            }
        }
    }


    /// <summary>
    ///  Remove everything with an age older than
    /// </summary>
    private async Task ExpireTimedout()
    {
        
        string nowString = DateTime.UtcNow.ToString("o");
        DateTime oldest = DateTime.UtcNow - TimeSpan.FromMilliseconds(SdpSignallingTimeoutMs);

        var existing = await _context.WebRTCSignals.Where(x =>
                (DateTime.Parse(x.LastQueriedAt, null, DateTimeStyles.RoundtripKind) < oldest))
            .ToArrayAsync();

        // await _dumpAll();
        if (existing?.Length > 0)
        {
            foreach (var old in existing)
            {
                _logger.LogInformation($"@{nowString}: About to remove old from {old.From} to {old.To}");
            }

            _context.RemoveRange(existing);
            
            try {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while persisting changes: {e}");
            }
        }
    }
}

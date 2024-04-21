using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using System.Diagnostics;

namespace ParagliderFlightLog.XcScoreWapper;

public class XcScoreManager(ILogger<XcScoreManager> logger, IConfiguration config) : IDisposable
{
    private readonly Queue<ProcessRequest> _processRequests = new();
    private bool _running;
    private Task? _computeTask;
    private bool _disposedValue;

    /// <summary>
    /// Number of flight in the queue still to process
    /// </summary>
    public int FlightToProcess => _processRequests.Count;
    /// <summary>
    /// Indicates if a score engine is installed
    /// </summary>
    public bool ScoreEngineInstalled { get; private set; } = true;


    private async Task Compute(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _running)
        {
            if (_processRequests.TryDequeue(out var request))
            {
                string flightPath = Path.Combine(GetTmpFileDirectory(), Path.GetRandomFileName());
                string scorePath = Path.Combine(GetTmpFileDirectory(), Path.GetRandomFileName());

                try
                {
                    // writing file data to a file to give it to the external calculator
                    await File.WriteAllTextAsync(flightPath, request.Flight.IgcFileContent, token);
                    // execute the external calculator on the file and output the result in another file
                    //var result = await Cli
                    //.Wrap($"{GetCalculatorCmd()} {flightPath} out={scorePath} scoring=XContest")
                    //.WithValidation(CommandResultValidation.None)
                    //.ExecuteBufferedAsync();
                    var result = await Cli
                    .Wrap($"{GetCalculatorCmd()}")
                    .WithArguments($"{GetCjsPath()} {flightPath} out={scorePath} scoring=XContest")
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync();
                    logger.LogInformation("Flight score result : {standard}", result.StandardOutput);
                    if (!result.IsSuccess)
                    {
                        logger.LogError("Flight score exited with error code {errorCode} : {error}", result.ExitCode, result.StandardError);
                        continue;
                    }
                    // Reading the result to put it in the requested flight and if ok write in the database associated with the request
                    string scoreText = File.ReadAllText(scorePath);
                    var xcScore = XcScore.FromJson(scoreText);
                    if (xcScore != null)
                    {
                        request.Flight.XcScore = xcScore;
                        request.Db.UpdateFlight(request.Flight.ToFlight());
                    }
                }
                catch (Exception ex)
                {
                    _running = false;
                    ScoreEngineInstalled = false;
                    logger.LogCritical(ex, "Score engine cannot be executed");
                    throw;
                }
            }
            else
            {
                try
                {
                    var result = await Cli
    .Wrap($"{GetCalculatorCmd()}")
    .WithArguments($"{GetCjsPath()}")
    .WithValidation(CommandResultValidation.None)
    .ExecuteBufferedAsync();
                    bool isReady = result.StandardOutput.StartsWith("igc-xc-score");
                    ScoreEngineInstalled = isReady;
                    await Task.Delay(1000, token);
                    _running = isReady;
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "Cannot launch xcengine using cli wrapper");
                    _running = false;
                    ScoreEngineInstalled = false;
                }
            }
        }
    }
    public void QueueFlightForScoring(FlightWithData flight, FlightLogDB db)
    {
        if (string.IsNullOrEmpty(flight.IgcFileContent)) return;
        var request = new ProcessRequest()
        {
            Flight = flight,
            Db = db
        };
        _processRequests.Enqueue(request);
    }

    public void Start(CancellationToken token)
    {
        _running = true;
        logger.LogInformation("Starting XcScore manager. WorkingDirectory: {workingDirectory}, Calculator Cmd: {CalculatorCmd}, OutputDirectory: {OutputDirectory}",
        GetCalculatorCmd(), GetTmpFileDirectory());
        _computeTask = Compute(token);
    }
    public void Stop()
    {
        _running = false;
        _computeTask?.Wait();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _computeTask?.Dispose();
            }
            _processRequests.Clear();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private string GetCalculatorCmd()
    {
        string output = config.GetSection("XcScore")["CalculatorCmd"] ?? "";
        return output;
    }
    private string GetCjsPath(){
        string output = config.GetSection("XcScore")["CjsPath"] ?? "";
        return output;
    }
    private string GetTmpFileDirectory()
    {
        string output = config.GetSection("XcScore")["TmpFileDirectory"] ?? "";
        Directory.CreateDirectory(output);
        return output;
    }
    private sealed class ProcessRequest
    {
        public FlightWithData Flight { get; set; } = null!;
        public FlightLogDB Db { get; set; } = null!;
    }
}



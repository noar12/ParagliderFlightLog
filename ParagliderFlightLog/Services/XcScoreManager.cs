using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.Services;
/// <summary>
/// Service to manage the use of the external XcScore calculator
/// </summary>
/// <param name="logger"></param>
/// <param name="config"></param>
public class XcScoreManager(ILogger<XcScoreManager> logger, IConfiguration config, XcScoreManagerData xcScoreManagerData) : BackgroundService
{
    private bool _running;





    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _running = true;
        logger.LogInformation("Starting XcScore manager. OutputDirectory: {OutputDirectory}",
             GetTmpFileDirectory());
        while (!stoppingToken.IsCancellationRequested && _running)
        {
            var request = xcScoreManagerData.GetNextProcessRequest();
            if (request is not null)
            {
                string flightPath = Path.Combine(GetTmpFileDirectory(), Path.GetRandomFileName());
                string scorePath = Path.Combine(GetTmpFileDirectory(), Path.GetRandomFileName());

                try
                {
                    // writing file data to a file to give it to the external calculator
                    await File.WriteAllTextAsync(flightPath, request.Flight.IgcFileContent, stoppingToken);
                    // execute the external calculator on the file and output the result in another file
                    var result = await Cli
                        .Wrap("igc-xc-score")
                        .WithArguments($"{flightPath} out={scorePath} scoring=XContest")
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteBufferedAsync(stoppingToken);
                    logger.LogInformation("Flight score result : {Standard}", result.StandardOutput);
                    if (!result.IsSuccess)
                    {
                        logger.LogError("Flight score exited with error code {ErrorCode} : {Error}", result.ExitCode,
                            result.StandardError);
                        continue;
                    }

                    // Reading the result to put it in the requested flight and if ok write in the database associated with the request
                    string scoreText = await File.ReadAllTextAsync(scorePath, stoppingToken);
                    var xcScore = XcScore.FromJson(scoreText);
                    if (xcScore != null)
                    {
                        request.Flight.XcScore = xcScore;
                        request.Db.UpdateFlight(request.Flight.ToFlight());
                    }
                }
                catch (TaskCanceledException cancelEx)
                {
                    logger.LogWarning(cancelEx, "Task canceled during score calculation");
                }
                catch (Exception ex)
                {
                    _running = false;
                    xcScoreManagerData.ScoreEngineInstalled = false;
                    logger.LogCritical(ex, "Score engine cannot be executed");
                }
            }
            else
            {
                try
                {
                    await Task.Delay(1000, stoppingToken);
                    if (!_running || !xcScoreManagerData.ScoreEngineInstalled)
                    {
                        var result = await Cli
                            .Wrap("igc-xc-score")
                            .WithValidation(CommandResultValidation.None)
                            .ExecuteBufferedAsync(stoppingToken);
                        bool isReady = result.StandardOutput.StartsWith("igc-xc-score");
                        xcScoreManagerData.ScoreEngineInstalled = isReady;
                        await Task.Delay(1000, stoppingToken);
                        _running = isReady;
                    }
                }
                catch (TaskCanceledException canceledException)
                {
                    logger.LogInformation(canceledException, "Task cancelled");
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "Cannot launch xcengine using cli wrapper");
                    _running = false;
                    xcScoreManagerData.ScoreEngineInstalled = false;
                }
            }
        }
    }
    private string GetTmpFileDirectory()
    {
        string output = config.GetSection("XcScore")["TmpFileDirectory"] ?? "";
        Directory.CreateDirectory(output);
        return output;
    }

}



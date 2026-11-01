using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace TwBlazor.Templates.Cli;

/// <summary>
/// Runs child processes and forwards their output to the console.
/// </summary>
internal static class ProcessRunner
{
    /// <summary>
    /// Runs a process to completion.
    /// </summary>
    /// <param name="fileName">The executable to start.</param>
    /// <param name="arguments">The arguments passed to the executable.</param>
    /// <param name="workingDirectory">The working directory for the process.</param>
    /// <param name="verbose">Whether the child process output is written to the console.</param>
    /// <param name="cancellationToken">A token that cancels the running process.</param>
    /// <returns>The process exit code.</returns>
    internal static async Task<int> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        string workingDirectory,
        bool verbose,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                throw new CliException($"Could not start '{fileName}'.");
            }
        }
        catch (Win32Exception exception)
        {
            Console.Error.WriteLine($"error: could not start '{fileName}' ({exception.Message}). Is it installed and on the PATH?");
            return 1;
        }

        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await standardOutput;
        var error = await standardError;

        if (verbose)
        {
            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.Out.Write(output);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.Error.Write(error);
            }
        }

        return process.ExitCode;
    }
}

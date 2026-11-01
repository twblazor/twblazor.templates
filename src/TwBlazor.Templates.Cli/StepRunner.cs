namespace TwBlazor.Templates.Cli;

/// <summary>
/// Runs a command step with a numbered purple label and animated dots.
/// </summary>
internal sealed class StepRunner
{
    private static readonly string[] Frames = ["   ", ".  ", ".. ", "..."];

    private readonly TextWriter _output;
    private readonly bool _animate;
    private int _count;

    internal StepRunner(TextWriter output, bool animate)
    {
        _output = output;
        _animate = animate;
    }

    internal async Task<int> RunAsync(string label, Func<CancellationToken, Task<int>> action, CancellationToken cancellationToken)
    {
        var number = ++_count;
        var line = $"{number}. {label}";

        if (!_animate)
        {
            _output.Write(Banner.Paint($"{line} ... "));
            var exitCode = await action(cancellationToken);
            _output.WriteLine(exitCode == 0 ? Banner.PaintSuccess("done.") : Banner.PaintError("failed."));
            return exitCode;
        }

        using var dots = new Dots(_output, Banner.Paint(line));
        var code = await action(cancellationToken);
        dots.Stop();
        _output.WriteLine($"\r{Banner.Paint(line)} {(code == 0 ? Banner.PaintSuccess("✓") : Banner.PaintError("✗"))}   ");
        return code;
    }

    private sealed class Dots : IDisposable
    {
        private readonly TextWriter _output;
        private readonly string _line;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _task;

        internal Dots(TextWriter output, string line)
        {
            _output = output;
            _line = line;
            _task = Task.Run(Spin);
        }

        private async Task Spin()
        {
            var index = 0;
            while (!_cts.IsCancellationRequested)
            {
                _output.Write($"\r{_line} {Frames[index++ % Frames.Length]}");
                try
                {
                    await Task.Delay(150, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            try
            {
                _task.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Dispose() => Stop();
    }
}

using TwBlazor.Templates.Cli;
using Xunit;

namespace TwBlazor.Templates.Tests;

public sealed class StepRunnerTests
{
    [Fact]
    public async Task StepRunner_NumbersStepsAndReportsResult()
    {
        var writer = new StringWriter();
        var steps = new StepRunner(writer, animate: false);

        var first = await steps.RunAsync("first", _ => Task.FromResult(0), CancellationToken.None);
        var second = await steps.RunAsync("second", _ => Task.FromResult(1), CancellationToken.None);

        var text = writer.ToString();
        Assert.Equal(0, first);
        Assert.Equal(1, second);
        Assert.Contains("1. first", text);
        Assert.Contains("2. second", text);
        Assert.Contains("done.", text);
        Assert.Contains("failed.", text);
    }

    [Fact]
    public async Task StepRunner_WritesLabelEvenWhenNotAnimated()
    {
        var writer = new StringWriter();
        var steps = new StepRunner(writer, animate: false);

        await steps.RunAsync("step", _ => Task.FromResult(0), CancellationToken.None);

        var text = writer.ToString();
        Assert.Contains("1. ", text);
        Assert.Contains("done.", text);
    }
}

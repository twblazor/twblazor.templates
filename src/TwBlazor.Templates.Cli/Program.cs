using System.Text;
using TwBlazor.Templates.Cli;

// Tailwind's CLI prints a Unicode banner. Windows consoles default to an OEM code page,
// which turns the "≈" glyph into mojibake such as "Ôëê" when child output is decoded.
try
{
    Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}
catch (IOException)
{
    // The console may be redirected or unavailable; the child output is still decoded as UTF-8.
}

using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

return await CliApplication.RunAsync(args, cancellation.Token);

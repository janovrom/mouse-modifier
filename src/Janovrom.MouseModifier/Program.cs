using Janovrom.MouseModifier.WindowsService;

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Application.Exit();
};

Hook.Start();
Console.WriteLine("Press Ctrl+C to exit.");
Application.Run();
Hook.Stop();
using Janovrom.MouseModifier.WindowsService;

class Program
{
    static void Main()
    {
        Hook.Start();
        Application.Run();
        Hook.Stop();
    }
}

public static class VisualUpgradeSetup
{
    public static void Run() => PixelLabSetup.Run();
    public static void Verify()
    {
        Run();
        LabGameChecks.Run();
    }
}

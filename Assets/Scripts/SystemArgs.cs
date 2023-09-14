public static class SystemArgs
{
    private static bool initialized = false;

    private static string trainerStatusPath = null;
    private static string modelsPath = null;
    private static string gameParamsPath = null;
    private static string arenaParamsPath = null;

    public static string TrainerStatusPath { get { Initialize(); return trainerStatusPath; } }
    public static string ModelsPath { get { Initialize(); return modelsPath; } }
    public static string GameParamsPath { get { Initialize(); return gameParamsPath; } }
    public static string ArenaParamsPath { get { Initialize(); return arenaParamsPath; } }

    private static void Initialize()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;

        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("trainer_status="))
            {
                trainerStatusPath = arg[15..];
            }
            if (arg.StartsWith("models_path="))
            {
                modelsPath = arg[12..];
            }
            if (arg.StartsWith("game_params="))
            {
                gameParamsPath = arg[12..];
            }
            if (arg.StartsWith("arena_params="))
            {
                arenaParamsPath = arg[13..];
            }
        }
    }
}

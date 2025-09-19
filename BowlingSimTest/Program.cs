using System.Text.Json;

namespace BowlingSimTest;

/// <summary>
/// Entry point for the bowling simulation used in the design interview exercise.
/// </summary>
internal static class Program
{
    private const string ConfigDirectory = "GameModes";

    private static void Main()
    {
        Console.WriteLine("=== Bowling Simulation Test Harness ===\n");
        Console.WriteLine("This lightweight console app lets a candidate try out alternative bowling rules.\n");

        // Load the standard mode bundled with the repository and any additional configs.
        var availableModes = LoadGameModes();
        if (availableModes.Count == 0)
        {
            Console.WriteLine("No game modes were found. Ensure the configuration files are present in the GameModes folder.");
            return;
        }

        var selectedMode = PromptForGameMode(availableModes);
        Console.WriteLine($"\nSelected mode: {selectedMode.Name} (Frames: {selectedMode.FrameCount})\n");

        var players = PromptForPlayers();
        Console.WriteLine();

        RunGame(players, selectedMode);

        Console.WriteLine("\nThanks for playing! Discuss with the candidate how they might evolve these rules.");
    }

    /// <summary>
    /// Loads game mode definitions from the <see cref="ConfigDirectory"/> folder.
    /// </summary>
    private static List<GameModeConfig> LoadGameModes()
    {
        var modes = new List<GameModeConfig>();

        if (!Directory.Exists(ConfigDirectory))
        {
            return modes;
        }

        foreach (var file in Directory.EnumerateFiles(ConfigDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var json = File.ReadAllText(file);
                var config = JsonSerializer.Deserialize<GameModeConfig>(json);
                if (config != null)
                {
                    config.SourceFile = Path.GetFileName(file);
                    modes.Add(config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load '{file}': {ex.Message}");
            }
        }

        return modes;
    }

    /// <summary>
    /// Lets the facilitator pick a game mode to use for the session.
    /// </summary>
    private static GameModeConfig PromptForGameMode(IReadOnlyList<GameModeConfig> modes)
    {
        Console.WriteLine("Available game modes:");
        for (var i = 0; i < modes.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {modes[i].Name} - {modes[i].Description}");
        }

        while (true)
        {
            Console.Write("Select a mode by number: ");
            var response = Console.ReadLine();
            if (int.TryParse(response, out var index) && index >= 1 && index <= modes.Count)
            {
                return modes[index - 1];
            }

            Console.WriteLine("Please enter a valid mode number.");
        }
    }

    /// <summary>
    /// Prompt for player names and ensure at least two are registered.
    /// </summary>
    private static List<PlayerState> PromptForPlayers()
    {
        Console.Write("How many players are participating (minimum 2)? ");
        int playerCount;
        while (!int.TryParse(Console.ReadLine(), out playerCount) || playerCount < 2)
        {
            Console.Write("Please enter a number >= 2: ");
        }

        var players = new List<PlayerState>();
        for (var i = 0; i < playerCount; i++)
        {
            Console.Write($"Enter name for player {i + 1}: ");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Player {i + 1}";
            }

            players.Add(new PlayerState(name.Trim()));
        }

        return players;
    }

    /// <summary>
    /// Runs the game loop across all frames and players.
    /// </summary>
    private static void RunGame(IReadOnlyList<PlayerState> players, GameModeConfig mode)
    {
        for (var frame = 1; frame <= mode.FrameCount; frame++)
        {
            Console.WriteLine($"\n=== Frame {frame} ===");
            foreach (var player in players)
            {
                Console.WriteLine($"\n{player.Name}'s turn:");
                PlayFrame(player, frame, mode);
                DisplayScoreboard(players, mode);
            }
        }

        Console.WriteLine("\n=== Final Scores ===");
        DisplayScoreboard(players, mode, showFrameBreakdown: true);
    }

    /// <summary>
    /// Handles a single player's frame including strike/spare logic.
    /// </summary>
    private static void PlayFrame(PlayerState player, int frameNumber, GameModeConfig mode)
    {
        var isFinalFrame = frameNumber == mode.FrameCount;
        var pinsRemaining = mode.PinsPerFrame;

        // First roll.
        var firstRoll = PromptForRoll(pinsRemaining);
        player.Rolls.Add(firstRoll);
        pinsRemaining -= firstRoll;
        Console.WriteLine($"Roll 1 knocked down {firstRoll} pin(s). Remaining: {pinsRemaining}.");

        if (!isFinalFrame)
        {
            // A strike in non-final frames ends the turn immediately.
            if (firstRoll == mode.PinsPerFrame)
            {
                Console.WriteLine("Strike! Turn complete.");
                return;
            }

            var secondRoll = PromptForRoll(pinsRemaining);
            player.Rolls.Add(secondRoll);
            pinsRemaining -= secondRoll;
            Console.WriteLine($"Roll 2 knocked down {secondRoll} pin(s). Remaining: {pinsRemaining}.");

            if (firstRoll + secondRoll == mode.PinsPerFrame)
            {
                Console.WriteLine("Spare!");
            }

            return;
        }

        // Final frame logic: allow for extra roll(s) on strike or spare.
        int secondRollFinal = 0;
        if (firstRoll == mode.PinsPerFrame)
        {
            Console.WriteLine("Strike! Pins reset for bonus rolls.");
            pinsRemaining = mode.PinsPerFrame;
            secondRollFinal = PromptForRoll(pinsRemaining);
            player.Rolls.Add(secondRollFinal);
            pinsRemaining = secondRollFinal == mode.PinsPerFrame ? mode.PinsPerFrame : mode.PinsPerFrame - secondRollFinal;
            Console.WriteLine($"Bonus Roll 1 knocked down {secondRollFinal} pin(s). Remaining: {pinsRemaining}.");

            var thirdRoll = PromptForRoll(pinsRemaining);
            player.Rolls.Add(thirdRoll);
            Console.WriteLine($"Bonus Roll 2 knocked down {thirdRoll} pin(s).");
            return;
        }

        // Non-strike first roll in final frame uses the remaining pins as usual.
        var secondRoll = PromptForRoll(pinsRemaining);
        player.Rolls.Add(secondRoll);
        pinsRemaining -= secondRoll;
        Console.WriteLine($"Roll 2 knocked down {secondRoll} pin(s). Remaining: {pinsRemaining}.");

        if (firstRoll + secondRoll == mode.PinsPerFrame)
        {
            Console.WriteLine("Spare! Awarded one bonus roll.");
            pinsRemaining = mode.PinsPerFrame;
            var thirdRoll = PromptForRoll(pinsRemaining);
            player.Rolls.Add(thirdRoll);
            Console.WriteLine($"Bonus Roll knocked down {thirdRoll} pin(s).");
        }
    }

    /// <summary>
    /// Prompts the user for an integer roll between 0 and the remaining pins.
    /// </summary>
    private static int PromptForRoll(int pinsRemaining)
    {
        while (true)
        {
            Console.Write($"Enter pins knocked down (0-{pinsRemaining}): ");
            var response = Console.ReadLine();
            if (int.TryParse(response, out var pins) && pins >= 0 && pins <= pinsRemaining)
            {
                return pins;
            }

            Console.WriteLine("Invalid entry. Please try again.");
        }
    }

    /// <summary>
    /// Displays the rolling scoreboard so the candidate can track scoring logic live.
    /// </summary>
    private static void DisplayScoreboard(IReadOnlyList<PlayerState> players, GameModeConfig mode, bool showFrameBreakdown = false)
    {
        Console.WriteLine("\nCurrent Scores:");
        foreach (var player in players)
        {
            var (score, frameTotals) = ScoreCalculator.CalculateScore(player.Rolls, mode);
            Console.WriteLine($"  {player.Name}: {score} points");

            if (showFrameBreakdown)
            {
                Console.WriteLine("    Frame totals: " + string.Join(", ", frameTotals));
            }
        }
    }
}

/// <summary>
/// Represents the mutable state for each player.
/// </summary>
internal sealed class PlayerState
{
    public PlayerState(string name)
    {
        Name = name;
    }

    public string Name { get; }

    /// <summary>
    /// Stores every roll the player has completed. This makes the standard scoring
    /// algorithm straightforward because it works with a flat list of rolls.
    /// </summary>
    public List<int> Rolls { get; } = new();
}

/// <summary>
/// Additional metadata that can be stored with each game mode configuration.
/// </summary>
internal sealed class GameModeConfig
{
    public string Name { get; set; } = "Standard";

    public string Description { get; set; } = "Traditional 10-frame bowling.";

    public int FrameCount { get; set; } = 10;

    public int PinsPerFrame { get; set; } = 10;

    /// <summary>
    /// Extra points to add on top of the standard spare bonus. Used by the alternative sample mode.
    /// </summary>
    public int ExtraSpareBonus { get; set; }

    /// <summary>
    /// Stores which file the configuration was loaded from to help during interviews.
    /// </summary>
    public string? SourceFile { get; set; }
}

/// <summary>
/// Contains the scoring logic for standard bowling with a hook for variant rules.
/// </summary>
internal static class ScoreCalculator
{
    /// <summary>
    /// Calculates the total score and a breakdown per frame.
    /// </summary>
    public static (int totalScore, List<int> frameTotals) CalculateScore(List<int> rolls, GameModeConfig mode)
    {
        var total = 0;
        var frameTotals = new List<int>();
        var rollIndex = 0;

        for (var frame = 0; frame < mode.FrameCount && rollIndex < rolls.Count; frame++)
        {
            if (IsStrike(rolls, rollIndex, mode))
            {
                var frameScore = mode.PinsPerFrame + GetRollSafe(rolls, rollIndex + 1) + GetRollSafe(rolls, rollIndex + 2);
                total += frameScore;
                frameTotals.Add(frameScore);
                rollIndex += 1;
                continue;
            }

            var framePins = GetRollSafe(rolls, rollIndex) + GetRollSafe(rolls, rollIndex + 1);
            if (framePins == mode.PinsPerFrame)
            {
                var frameScore = mode.PinsPerFrame + GetRollSafe(rolls, rollIndex + 2) + mode.ExtraSpareBonus;
                total += frameScore;
                frameTotals.Add(frameScore);
            }
            else
            {
                total += framePins;
                frameTotals.Add(framePins);
            }

            rollIndex += 2;
        }

        return (total, frameTotals);
    }

    private static bool IsStrike(IReadOnlyList<int> rolls, int index, GameModeConfig mode) => GetRollSafe(rolls, index) == mode.PinsPerFrame;

    private static int GetRollSafe(IReadOnlyList<int> rolls, int index) => index < rolls.Count ? rolls[index] : 0;
}

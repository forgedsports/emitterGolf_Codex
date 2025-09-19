# Bowling Simulation Test Harness

This folder contains a minimal .NET 6 console project that acts as a playground for exploring bowling rule variations during a game design interview.

## Project Structure

- `Program.cs` – Entry point and game logic. The code is heavily commented so it can serve as a conversation starter during the interview.
- `BowlingSimTest.csproj` – Project file targeting .NET 6.
- `GameModes/` – Folder containing JSON files that describe available modes. `standard.json` implements the classic rule set while `spare_frenzy.json` shortens the game to five frames and awards an extra five points for every spare.

## Running the Simulation

1. Ensure you have the .NET 6 SDK installed.
2. From this directory, run:

   ```bash
   dotnet run
   ```

3. Choose a game mode, enter player names (at least two), and follow the prompts to record each roll.

Feel free to add more JSON files to prototype additional modes or tweak the scoring logic inside `Program.cs` to match the candidate's proposal.

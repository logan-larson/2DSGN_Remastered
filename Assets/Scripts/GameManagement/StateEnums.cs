

public enum GameState
{
    PreGame,
    InGame,
    PostGame
}


public enum LobbyState
{
    // After the game ends or when the server is initialized, the lobby state is set to PreVoting. This is when the new map options are selected
    // and the players are waiting for the voting period to start.
    PreVoting,
    // After a single vote is cast, the lobby state is set to Voting. A timer is started to prevent
    // players from waiting too long for all the players to vote.
    Voting,
    // After the voting period ends, either by all players voting or by a timer, the lobby state is set to PostVoting. 
    // This is when the map is selected and spawned. After the map is spawned, the players are spawned. Then the lobby state is set to PreGame.
    PostVoting,
    // After the map and players are spawned, the lobby state is set to PreGame. This is when the countdown starts.
    // After the countdown ends, the lobby state is set to Game.
    PreGame,
    // After the countdown ends, the lobby state is set to Game. This is when the game is running. The lobby waits for an event
    // from the Game Manager to end the game (e.g. a team wins, a player wins, a timer ends, etc.).
    Game,
    // After the game ends, the lobby state is set to PostGame. This is when the game results are shown and a timer is started to go back to PreVoting.
    PostGame,
}

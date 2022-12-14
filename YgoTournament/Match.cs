namespace YgoTournament;

public class Match
{
    /// <summary>
    /// Represents a single match in a round. Provides methods to handle match reports
    /// </summary>
    #region Data
    #region Member

    private bool _isDrawn;
    private Player? _winner;
    private Player? _loser;

    #endregion Member

    #region Properties

    public bool IsFinished => _isDrawn || _winner is not null;

    public Player Player1 { get; }

    public Player Player2 { get; }

    #endregion Properties

    #endregion Data
    
    #region Constructor

    /// <summary>
    /// Creates a match with the 2 given players.
    /// </summary>
    /// <param name="p1">Player 1</param>
    /// <param name="p2">Player 2</param>
    public Match(Player p1, Player p2)
    {
        Player1 = p1;
        Player2 = p2;
        //add the players to each others opponents list for tiebreaker tracking
        Player1.AddOpponent(Player2);
        Player2.AddOpponent(Player1);
    }
    #endregion Constructor
    
    #region Matchreports

    /// <summary>
    /// Reports a win for the given player
    /// </summary>
    /// <param name="p">Player who won</param>
    /// <param name="numberOfRound">number of the round the win is reported for</param>
    /// <exception cref="ArgumentException">Throws exception if given player did not participate in this match</exception>
    public void ReportWin(Player p, int numberOfRound)
    {
        if (Player1 != p && Player2 != p)
        {
            throw new ArgumentException("Given player did not participate in this match.");
        }

        if (Player1 == p)
        {
            _winner = Player1;
            _loser = Player2;
        }
        else
        {
            _winner = Player2;
            _loser = Player1;
        }

        _winner.AddWin(numberOfRound);
        _loser.AddLose(numberOfRound);
    }
    
    /// <summary>
    /// Reports a loss for the given player
    /// </summary>
    /// <param name="p">Player who lost</param>
    /// <param name="numberOfRound">number of the round the loss is reported for</param>
    /// <exception cref="ArgumentException">Throws exception if given player did not participate in this match</exception>
    public void ReportLose(Player p, int numberOfRound)
    {
        if (Player1 != p && Player2 != p)
        {
            throw new ArgumentException("Given player did not participate in this match.");
        }

        if (Player1 == p)
        {
            _winner = Player2;
            _loser = Player1;
        }
        else
        {
            _winner = Player1;
            _loser = Player2;
        }

        _winner.AddWin(numberOfRound);
        _loser.AddLose(numberOfRound);
    }

    /// <summary>
    /// Reports a draw
    /// </summary>
    /// <param name="numberOfRound">number of the round the draw is reported for</param>
    public void ReportDraw(int numberOfRound)
    {
        _isDrawn = true;
        Player1.AddDraw(numberOfRound);
        Player2.AddDraw(numberOfRound);
    }
    #endregion Matchreports
}
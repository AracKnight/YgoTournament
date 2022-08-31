namespace YgoTournament;

public class Round
{
    /// <summary>
    /// Represents a round in a tournament. Provides methods to create pairings and handle match reports.
    /// </summary>
    #region Data
    
    #region Member
    
    private readonly Dictionary<int, Match> _tables;

    #endregion Member
    
    #region Properties
    public int NumberOfRound { get; }

    public bool IsFinished
    {
        get
        {
            return _tables.All(tuple => tuple.Value.IsFinished);
        }
    }
    
    #endregion Properties
    #endregion Data
    
    #region Constructor
    /// <summary>
    /// Creates a new round and generates pairings for it.
    /// </summary>
    /// <param name="players">List of players participating in the tournament. Dropped players are ignored.</param>
    /// <param name="nr">Number of the round</param>
    public Round(List<Player> players, int nr)
    {
        _tables = new Dictionary<int, Match>();
        NumberOfRound = nr;
        var playersDropped = players.Where(p => p.IsDropped).ToList(); //filter out dropped players
        foreach (var p in playersDropped)
        {
            players.Remove(p);
        }

        var comparer = new PlayerComparer(); //sort players by their current tiebreakers then match first with second and so on
        players.Sort(comparer);
        var j = 1;
        for (var i = 0; i < players.Count; i+=2)
        {
            Match match;
            if (i == players.Count - 1)
            {
                match = new Match(players[i], new Player("BYE"));
                match.ReportWin(players[i], NumberOfRound);
            }
            else
            {
                match = new Match(players[i], players[i + 1]);
            }
            _tables.Add(j, match);
            j++;
        }
    }
    #endregion Constructor
    
    #region Matchreports

    /// <summary>
    /// Reports win for the given player
    /// </summary>
    /// <param name="p">Player who won</param>
    /// <exception cref="ArgumentException">Throws exception if given player was not in the pairings.</exception>
    public void ReportWin(Player p)
    {
        Match? match = null;
        foreach (var tuple in _tables.Where(tuple => tuple.Value.Player1 == p || tuple.Value.Player2 == p)) //search match with the given player
        {
            match = tuple.Value;
        }

        if (match is null)
        {
            throw new ArgumentException("Given Player does not participate in the tournament.");
        }

        match.ReportWin(p, NumberOfRound);
    }
    
    /// <summary>
    /// Reports loss for the given player
    /// </summary>
    /// <param name="p">Player who lost</param>
    /// <exception cref="ArgumentException">Throws exception if given player was not in the pairings.</exception>
    public void ReportLose(Player p)
    {
        Match? match = null;
        foreach (var tuple in _tables.Where(tuple => tuple.Value.Player1 == p || tuple.Value.Player2 == p)) //search match with the given player
        {
            match = tuple.Value;
        }

        if (match is null)
        {
            throw new ArgumentException("Given Player does not participate in the tournament.");
        }

        match.ReportLose(p, NumberOfRound);
    }
    /// <summary>
    /// Reports draw for the given player
    /// </summary>
    /// <param name="p">Player who drew</param>
    /// <exception cref="ArgumentException">Throws exception if given player was not in the pairings.</exception>
    public void ReportDraw(Player p)
    {
        Match? match = null;
        foreach (var tuple in _tables.Where(tuple => tuple.Value.Player1 == p || tuple.Value.Player2 == p)) //search match with the given player
        {
            match = tuple.Value;
        }

        if (match is null)
        {
            throw new ArgumentException("Given Player does not participate in the tournament.");
        }
        match.ReportDraw(NumberOfRound);
    }
    #endregion Matchreports
    
    #region Overrides

    /// <summary>
    /// Creates a string in the form of 'Table x: a vs b'
    /// </summary>
    /// <returns>The constructed string</returns>
    public override string ToString()
    {
        return _tables.Aggregate($"", (current, tuple) => current + $"Table {tuple.Key}:\t{tuple.Value.Player1.Name}\tvs.\t{tuple.Value.Player2.Name}\n");
    }
    #endregion Overrides
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace YgoTournament
{

    public class Tournament
    {
        /// <summary>
        /// Represents a YGO Locals Tournament. Provides methods to add or drop players, pair rounds, report match results
        /// and get standings accordingly
        /// </summary>

        #region Data

        #region Member

        private readonly List<Player> _players;

        private Round _currentRound;
        private readonly int _maxRounds;

        #endregion Member

        #region Properties

        public string Standings
        {
            get
            {
                var standings = "";
                if (_currentRound != null)
                {
                    if (!_currentRound.IsFinished)
                    {
                        return standings;
                    }
                }
                else
                {
                    return standings;
                }

                SortPlayers();
                var j = 1;
                double lastTie = -1;
                foreach (var p in _players)
                {
                    if (Math.Abs(lastTie - p.Tiebreaker) < 0.1)
                    {
                        standings += $"\t{p.Name}\t{p.Points}\t{p.Tiebreaker}\n";
                    }
                    else
                    {
                        lastTie = p.Tiebreaker;
                        standings += $"{j}\t{p.Name}\t{p.Points}\t{p.Tiebreaker}\n";
                    }

                    j++;
                }

                return standings;
            }
        }

        public string Pairings
        {
            get
            {
                if (_currentRound is null)
                {
                    return "";
                }

                return _currentRound.ToString();
            }
        }

        public string Name { get; }

        public int RoundNr
        {
            get
            {
                if (_currentRound is null)
                {
                    return 0;
                }

                return _currentRound.NumberOfRound;
            }
        }

        #endregion Properties

        #endregion Data

        #region Constructor

        /// <summary>
        /// Create a new tournament
        /// </summary>
        /// <param name="name">The name of the tournament</param>
        /// <param name="players">List of player names participating</param>
        /// <param name="maxRounds">Maximum number of Rounds to be played</param>
        public Tournament(string name, List<string> players, int maxRounds = 9999)
        {
            var tempPlayers = ConvertStringsToPlayers(players);
            Name = name;
            _maxRounds = maxRounds;
            _players = tempPlayers;
        }

        #endregion Constructor

        #region Players

        #region Adding

        /// <summary>
        /// Adds a new player to the tournament. Checks for already past rounds and handles accordingly if so
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="winBye">Whether the player is considered to have won already past rounds. Does not have
        /// any effect, if no round has started yet</param>
        public void AddPlayer(string name, bool winBye = false)
        {
            if (_currentRound != null)
            {
                if (_currentRound.NumberOfRound > 0)
                {
                    AddPlayerLate(name, winBye);
                }
            }

            var p = ConvertStringToPlayer(name);
            _players.Add(p);
        }

        /// <summary>
        /// Adds a new player to the tournament and records his results for already past rounds
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="winBye">Whether the player is considered to have won already past rounds</param>
        public void AddPlayerLate(string name, bool winBye = false)
        {
            if (_currentRound is null)
            {
                throw new ArgumentException("Tournament has not started yet.");
            }

            var nr = _currentRound.NumberOfRound;
            var p = ConvertStringToPlayer(name);
            for (int i = 1; i <= nr; i++)
            {
                p.AddOpponent(new Player("BYE"));
                if (winBye)
                {
                    p.AddWin(i);
                }
                else
                {
                    p.AddLose(i);
                }
            }

            _players.Add(p);
        }

        /// <summary>
        /// Adds a List of Players with given names to the tournament. Checks for already past rounds and handles it
        /// accordingly
        /// </summary>
        /// <param name="players">List of player names to add</param>
        /// <param name="winBye">Whether the player is considered to have won already past rounds. Does not have
        /// any effect, if no round has started yet</param>
        public void AddPlayerRange(List<string> players, bool winBye = false)
        {
            foreach (var p in players)
            {
                AddPlayer(p, winBye);
            }
        }

        #endregion Adding

        #region Drop

        /// <summary>
        /// Drops the player with given name from the tournament
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <exception cref="ArgumentException">Throws Exception if player was not found</exception>
        public void DropPlayer(string name)
        {
            var p = GetPlayerByName(name);
            if (p is null)
            {
                throw new ArgumentException("Provided player does not participate in the tournament.");
            }

            var nr = 0;
            if (_currentRound != null)
            {
                nr = _currentRound.NumberOfRound;
            }

            p.Drop(nr);
        }

        /// <summary>
        /// Adding a dropped player back to the tournament. Checks if he missed rounds and handles accordingly.
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="winBye">Whether the player is considered to have won already past rounds. Does not have
        /// any effect, if no round has past</param>
        /// <exception cref="ArgumentException">Throws Exception if player was not found</exception>
        public void UndoDrop(string name, bool winBye = false)
        {
            var p = GetPlayerByName(name);
            if (p is null)
            {
                throw new ArgumentException("Provided player does not participate in the tournament.");
            }

            var nr = 0;
            if (_currentRound != null)
            {
                nr = _currentRound.NumberOfRound;
            }

            p.UndoDrop(nr, winBye);
        }

        #endregion Drop

        #endregion Players

        #region Pairings

        /// <summary>
        /// Pairs the next round
        /// </summary>
        /// <exception cref="ArgumentException">Throws exception if current round is not finished yet</exception>
        public void PairNextRound()
        {
            var nr = 1;
            if (_currentRound != null)
            {
                if (!_currentRound.IsFinished)
                {
                    throw new ArgumentException("Current round is not finished yet.");
                }

                nr = _currentRound.NumberOfRound + 1;
            }

            foreach (var p in _players)
            {
                p.UpdatePointrate(nr);
            }

            foreach (var p in _players)
            {
                p.UpdateTiebreaker1();
            }

            foreach (var p in _players)
            {
                p.UpdateTiebreaker2();
            }

            if (nr > _maxRounds)
            {
                throw new ArgumentException("Maximum number of Rounds is reached.");
            }

            var nextRound = new Round(new List<Player>(_players), nr);
            _currentRound = nextRound;
        }

        #endregion Pairings

        #region Matchreports

        /// <summary>
        /// Reports a win for given player
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <exception cref="ArgumentException">Throws exception if player is not in pairings</exception>
        public void ReportWin(string name)
        {
            var player = GetPlayerByName(name);
            if (player is null)
            {
                throw new ArgumentException("Requested player does not participate in the tournament.");
            }

            if (_currentRound is null)
            {
                throw new ArgumentException("There is currently no round in progress.");
            }

            _currentRound.ReportWin(player);
        }

        /// <summary>
        /// Reports a lose for given player
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <exception cref="ArgumentException">Throws exception if player is not in pairings</exception>
        public void ReportLose(string name)
        {
            var player = GetPlayerByName(name);
            if (player is null)
            {
                throw new ArgumentException("Requested player does not participate in the tournament.");
            }

            if (_currentRound is null)
            {
                throw new ArgumentException("There is currently no round in progress.");
            }

            _currentRound.ReportLose(player);
        }

        /// <summary>
        /// Reports a draw for given player
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <exception cref="ArgumentException">Throws exception if player is not in pairings</exception>
        public void ReportDraw(string name)
        {
            var player = GetPlayerByName(name);
            if (player is null)
            {
                throw new ArgumentException("Requested player does not participate in the tournament.");
            }

            if (_currentRound is null)
            {
                throw new ArgumentException("There is currently no round in progress.");
            }

            _currentRound.ReportDraw(player);
        }

        /// <summary>
        /// Reports a draw for given player
        /// </summary>
        /// <param name="tableNr">Name of the player</param>
        /// <exception cref="ArgumentException">Throws exception if player is not in pairings</exception>
        public void ReportDraw(int tableNr)
        {
            if (_currentRound is null)
            {
                throw new ArgumentException("There is currently no round in progress.");
            }

            _currentRound.ReportDraw(tableNr);
        }

        /// <summary>
        /// Reports a double loss
        /// </summary>
        /// <param name="tableNr">Number of the table to report the double loss on</param>
        /// <exception cref="ArgumentException">Throws exception if there is no active round or the given table
        /// number could not be found</exception>
        public void ReportDoubleLoss(int tableNr)
        {
            if (_currentRound is null)
            {
                throw new ArgumentException("Now round has started yet.");
            }

            _currentRound.ReportDoubleLoss(tableNr);
        }

        #endregion Matchreports

        #region Standings

        public List<string> GetWinner()
        {
            SortPlayers();
            var winner = new List<string>();
            if (_players.Count < 1)
            {
                return winner;
            }

            var lastTie = _players[0].Tiebreaker;
            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i].Tiebreaker < lastTie)
                {
                    break;
                }

                winner.Add(_players[i].ToString());
            }

            return winner;
        }

        #endregion Standings

        #region Private helpers

        /// <summary>
        /// Converts a list of player names into a list of Player instances
        /// </summary>
        /// <param name="playerList">List of the player names</param>
        /// <returns>List of the Player instances</returns>
        private static List<Player> ConvertStringsToPlayers(List<string> playerList)
        {
            return playerList.Select(ConvertStringToPlayer).ToList();
        }

        /// <summary>
        /// Creates a Player instance with the given name
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <returns>Player instance</returns>
        private static Player ConvertStringToPlayer(string name)
        {
            var player = new Player(name);
            return player;
        }

        /// <summary>
        /// Searches for the player with the given name
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <returns>The player instance or null if none was found</returns>
        private Player GetPlayerByName(string name)
        {
            return _players.FirstOrDefault(p => p.Name == name);
        }

        private void SortPlayers()
        {
            if (_currentRound is null)
            {
                return;
            }

            foreach (var p in _players)
            {
                p.UpdatePointrate(_currentRound.NumberOfRound);
            }

            foreach (var p in _players)
            {
                p.UpdateTiebreaker1();
            }

            foreach (var p in _players)
            {
                p.UpdateTiebreaker2();
            }

            var comparer = new PlayerComparer();
            _players.Sort(comparer);
        }

        #endregion
    }
}
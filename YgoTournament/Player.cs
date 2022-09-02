using System;
using System.Collections.Generic;
using System.Linq;

namespace YgoTournament
{

    public class Player
    {
        /// <summary>
        /// Represents a player in a tournament. Holds his points and tiebreaker values and updates them according to his match results
        /// </summary>

        #region Data

        #region Member

        private int _roundDropped;

        private readonly List<Player> _opponents;
        private double _tiebreaker2;

        #endregion Member

        #region Properties

        public string Name { get; }

        public int Points { get; private set; }

        public double Tiebreaker { get; private set; }

        public bool IsDropped { get; private set; }

        private double Pointrate { get; set; }

        private double Tiebreaker1 { get; set; }

        #endregion Properties

        #endregion Data

        #region Constructor

        /// <summary>
        /// Creating a new player with the given name.
        /// </summary>
        /// <param name="name">Name of the player</param>
        public Player(string name)
        {
            Name = name;
            _opponents = new List<Player>();
        }

        #endregion Constructor

        #region Opponents

        /// <summary>
        /// Adds given player to the list of this players opponents
        /// </summary>
        /// <param name="p">Player to add</param>
        public void AddOpponent(Player p)
        {
            _opponents.Add(p);
        }

        #endregion Opponents

        #region Drop

        /// <summary>
        /// Drops the player
        /// </summary>
        /// <param name="numberOfRound">Number of round player dropped in</param>
        public void Drop(int numberOfRound)
        {
            if (IsDropped)
            {
                return;
            }

            IsDropped = true;
            _roundDropped = numberOfRound; //is stored in case the player later returns
        }

        /// <summary>
        /// Adding the player to the tournament again
        /// </summary>
        /// <param name="numberOfRound">Number of round player returned in</param>
        /// <param name="winBye">Whether the player is considered to have won already past rounds. Does not have
        /// any effect, if no round has started yet</param>
        public void UndoDrop(int numberOfRound, bool winBye = false)
        {
            if (!IsDropped)
            {
                return;
            }

            IsDropped = false;
            var diff = numberOfRound - _roundDropped;
            for (var i = 0; i < diff; i++)
            {
                _opponents.Add(new Player("BYE"));
                if (winBye)
                {
                    AddWin(i + 1);
                }
                else
                {
                    AddLose(i + 1);
                }
            }
        }

        #endregion Drop

        #region Match reports

        /// <summary>
        /// Reports win for player and updates tiebreaker
        /// </summary>
        /// <param name="numberOfRound">Number of round win is reported for</param>
        public void AddWin(int numberOfRound)
        {
            Points += 3;
            UpdatePointrate(numberOfRound);
            UpdateTiebreaker();
        }

        /// <summary>
        /// Reports loss for player and updates tiebreaker
        /// </summary>
        /// <param name="numberOfRound">Number of round loss is reported for</param>
        public void AddLose(int numberOfRound)
        {
            UpdatePointrate(numberOfRound);
            UpdateTiebreaker();
        }

        /// <summary>
        /// Reports draw for player and updates tiebreaker
        /// </summary>
        /// <param name="numberOfRound">Number of round draw is reported for</param>
        public void AddDraw(int numberOfRound)
        {
            Points += 1;
            UpdatePointrate(numberOfRound);
            UpdateTiebreaker();
        }

        /// <summary>
        /// Removes an already reported win
        /// </summary>
        /// <param name="numberOfRound">Number of round win was reported for</param>
        public void RemoveWin(int numberOfRound)
        {
            Points -= 3;
            UpdatePointrate(numberOfRound - 1);
            UpdateTiebreaker();
        }

        /// <summary>
        /// Removes an already reported loss
        /// </summary>
        /// <param name="numberOfRound">Number of round win was reported for</param>
        public void RemoveLoss(int numberOfRound)
        {
            UpdatePointrate(numberOfRound - 1);
            UpdateTiebreaker();
        }

        /// <summary>
        /// Removes an already reported draw
        /// </summary>
        /// <param name="numberOfRound">Number of round win was reported for</param>
        public void RemoveDraw(int numberOfRound)
        {
            Points -= 1;
            UpdatePointrate(numberOfRound - 1);
            UpdateTiebreaker();
        }

        #endregion Match reports

        #region Tiebreaker

        /// <summary>
        /// Updates the Pointrate according to the maximum amount of possible points
        /// </summary>
        /// <param name="numberOfRound">Number of current round</param>
        public void UpdatePointrate(int numberOfRound)
        {
            Pointrate = Math.Round(Points / (float)(numberOfRound * 3) * 1000);
            if (Pointrate >= 1000)
            {
                Pointrate = 999;
            }
        }

        /// <summary>
        /// Updates the second part of the players tiebreaker
        /// </summary>
        public void UpdateTiebreaker1()
        {
            if (_opponents.Count == 0)
            {
                return;
            }

            double percentage = 0;
            var counter = 0;
            foreach (var opp in _opponents.Where(opp => opp.Name != "BYE")) //ignore byes
            {
                percentage += opp.Pointrate;
                counter++;
            }

            if (counter > 0)
            {
                Tiebreaker1 = Math.Round(percentage / (float)counter); //calculate average of pointrate of all opponents
            }
            else
            {
                Tiebreaker1 = 0;
            }

            UpdateTiebreaker();
        }

        /// <summary>
        /// Updates the third part of the players tiebreaker
        /// </summary>
        public void UpdateTiebreaker2()
        {
            if (_opponents.Count == 0)
            {
                return;
            }

            double percentage = 0;
            var counter = 0;
            foreach (var opp in _opponents.Where(opp => opp.Name != "BYE"))
            {
                percentage += opp.Tiebreaker1;
                counter++;
            }

            if (counter > 0)
            {
                _tiebreaker2 =
                    Math.Round(percentage /
                               (float)counter); //calculate average of pointrate of all players opponents' opponents
            }
            else
            {
                _tiebreaker2 = 0;
            }

            UpdateTiebreaker();
        }

        /// <summary>
        /// Update the whole tiebreaker score
        /// </summary>
        private void UpdateTiebreaker()
        {
            //tiebreaker is given as a 6+ digit number. The last three digits are the average winrate (as number of points
            //divided by maximum possible points) of the players opponents' opponents.
            //the second last three digits are the average winrate of all players opponents
            //All remaining first digits are the total sum of points earned in tournament yet
            Tiebreaker = Points * 1000000 + Tiebreaker1 * 1000 + _tiebreaker2;
        }

        #endregion Tiebreaker

        #region Overrides

        /// <summary>
        /// Creates string in the form 'name points tiebreaker'
        /// </summary>
        /// <returns>Returns the constructed string</returns>
        public override string ToString()
        {
            return $"{Name}\t{Points}\t{Tiebreaker}";
        }

        #endregion Overrides
    }

    public class PlayerComparer : IComparer<Player>
    {
        /// <summary>
        /// Custom comparer to sort list of players according to their tiebreaker. Players with the same name are ordered
        /// randomly
        /// </summary>
        private static readonly Random Random = new Random();

        public int Compare(Player p1, Player p2)
        {
            if (p1 is null)
            {
                return p2 is null ? 0 : 1;
            }

            if (p2 is null)
            {
                return -1;
            }

            if (p1.Tiebreaker > p2.Tiebreaker)
            {
                return -1;
            }

            if (p1.Tiebreaker < p2.Tiebreaker)
            {
                return 1;
            }

            var choices = new List<int>() { -1, 1 };
            return choices[Random.Next(choices.Count)];
        }
    }
}
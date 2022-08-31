Class to handle a YGO Tournament Local.

Use the namespace 'YgoTournament', create an instance of Tournament with a chosen
name and the list of names of your players.

You can then start your tournament by calling the .PairNextRound method.
You can report wins, draws, losses by the according .ReportWin, .ReportLose,
.ReportDraw method while providing the name of the player.

You can drop/undrop players by the .DropPlayer, .UndoDrop methods providing the
player name.

You can start a new round once the last is finished by calling .PairNextRound again.

You can get the final standings via the Standings Property.

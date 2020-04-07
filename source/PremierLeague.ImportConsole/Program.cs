using ConsoleTables;
using PremierLeague.Core;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.Entities;
using PremierLeague.Persistence;
using Serilog;
using System;
using System.Linq;

namespace PremierLeague.ImportConsole
{
    class Program
    {
        static void Main()
        {
            PrintHeader();
            InitData();
            AnalyzeData();

            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 60));

            Console.WriteLine(
                  @"
            _,...,_
          .'@/~~~\@'.          
         //~~\___/~~\\        P R E M I E R  L E A G U E 
        |@\__/@@@\__/@|             
        |@/  \@@@/  \@|            (inkl. Statistik)
         \\__/~~~\__//
          '.@\___/@.'
            `""""""
                ");

            Console.WriteLine(new String('-', 60));
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Importiert die Ergebnisse (csv-Datei >> Datenbank).
        /// </summary>
        private static void InitData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                Log.Information("Import der Spiele und Teams in die Datenbank");

                Log.Information("Datenbank löschen");
                unitOfWork.DeleteDatabase();
                Log.Information("Datenbank migrieren");
                unitOfWork.MigrateDatabase();
                Log.Information("Spiele werden von premierleague.csv eingelesen");
                var games = ImportController.ReadFromCsv().ToArray();
                if (games.Length == 0)
                {
                    Log.Warning("!!! Es wurden keine Spiele eingelesen");
                }
                else
                {
                    Log.Debug($"  Es wurden {games.Count()} Spiele eingelesen!");
                    var teams = games.Select(t => new
                    {
                        Team = t.HomeTeam
                    })
                    .Concat(games.Select(t => new
                    {
                        Team = t.GuestTeam
                    }))
                    .Distinct()
                    .ToArray();

                    Log.Debug($"  Es wurden {teams.Count()} Teams eingelesen!");

                    unitOfWork.Games.AddRange(games);
                    Log.Information("Daten werden in Datenbank gespeichert (in Context übertragen)");

                    unitOfWork.SaveChanges();
                    Log.Information("Daten wurden in DB gespeichert!");
                }
            }
        }

        private static void AnalyzeData()
        {
            using(IUnitOfWork uow = new UnitOfWork()) 
            {
                var teamWithMostGoals = uow.Teams.GetTeamWithMostGoals();
                PrintResult("Team mit den meisten geschossenen Toren:", $"{teamWithMostGoals.Team.Name}: {teamWithMostGoals.Goals} Tore");

                var teamWithMostAwayGoal = uow.Teams.GetTeamWithMostAwayGoals();
                PrintResult("Team mit den meisten geschossenen Auswärtstoren:", $"{teamWithMostAwayGoal.Team.Name}: {teamWithMostAwayGoal.Goals} Auswärtstore");

                var teamWithMostHomeGoals = uow.Teams.GetTeamWihMostHomeGoals();
                PrintResult("Team mit den meisten geschossenen Heimtoren", $"{teamWithMostHomeGoals.Team.Name}: {teamWithMostHomeGoals.Goals} Heimtore");

                var teamWithHighestGoalRatio = uow.Teams.GetTeamsWithHighestGoalRatio();
                PrintResult("Team mit dem besten Torverhältnis:", $"{teamWithHighestGoalRatio.Team.Name}: {teamWithHighestGoalRatio.Ratio} Torverhältnis");

                var teamStatic = uow.Teams.GetTeamStats();

                PrintResult("Team Leistung im Durchschnitt (sotiert nach durchsn. geschossene Tore pro Spiel [absteig.]):", ConsoleTable
                    .From(teamStatic)
                    .Configure(o => o.NumberAlignment = Alignment.Right)
                    .ToStringAlternative());

                //var teamTable = uow.Teams
                
            }
        }

        /// <summary>
        /// Erstellt eine Konsolenausgabe
        /// </summary>
        /// <param name="caption">Enthält die Überschrift</param>
        /// <param name="result">Enthält das ermittelte Ergebnise</param>
        private static void PrintResult(string caption, string result)
        {
            Console.WriteLine();

            if (!string.IsNullOrEmpty(caption))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new String('=', caption.Length));
                Console.WriteLine(caption);
                Console.WriteLine(new String('=', caption.Length));
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(result);
            Console.ResetColor();
            Console.WriteLine();
        }


    }
}

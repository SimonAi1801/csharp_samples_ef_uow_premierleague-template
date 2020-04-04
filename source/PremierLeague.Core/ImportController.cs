using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PremierLeague.Core.Entities;
using Utils;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        const string _fileName = "PremierLeague.csv";

        public static IEnumerable<Game> ReadFromCsv()
        {
            string filePath = MyFile.GetFullNameInApplicationTree(_fileName);
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
            IDictionary<string, Team> teams = new Dictionary<string, Team>();
            IList<Game> games = new List<Game>();

            foreach (var item in lines)
            {
                string[] parts = item.Split(';');
                int round = Convert.ToInt32(parts[0]);
                string homeName = parts[1];
                string guestName = parts[2];
                int homeGoals = Convert.ToInt32(parts[3]);
                int guestGoals = Convert.ToInt32(parts[4]);
                Team newTeam;

                Game game = new Game
                {
                    Round = round,
                    HomeGoals = homeGoals,
                    GuestGoals = guestGoals
                };

                Team homeTeam;
                if (teams.TryGetValue(homeName, out homeTeam))
                {
                    homeTeam.HomeGames.Add(game);
                    game.HomeTeam = homeTeam;
                }
                else
                {
                    newTeam = new Team
                    {
                        Name = homeName
                    };
                    teams.Add(homeName, newTeam);
                    newTeam.HomeGames.Add(game);
                    game.HomeTeam = newTeam;
                }

                Team guestTeam;
                if (teams.TryGetValue(guestName, out guestTeam))
                {
                    guestTeam.AwayGames.Add(game);
                    game.GuestTeam = guestTeam;
                }
                else
                {
                    newTeam = new Team
                    {
                        Name = guestName
                    };
                    teams.Add(guestName, newTeam);
                    newTeam.AwayGames.Add(game);
                    game.GuestTeam = newTeam;
                }
                games.Add(game);
            }
            return games.ToArray();
        }

    }
}

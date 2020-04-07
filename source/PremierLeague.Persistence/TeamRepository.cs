using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }

        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }

        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }

        public (Team Team, int Goals) GetTeamWithMostGoals()
        {
            return _dbContext.Teams
                   .Select(t => new
                   {
                       Team = t,
                       Goals = t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals)
                   })
                   .AsEnumerable()
                   .OrderByDescending(t => t.Goals)
                   .Select(t => (t.Team, t.Goals))
                   .First();
        }

        public (Team Team, int Goals) GetTeamWithMostAwayGoals()
        {
            return _dbContext.Teams
                   .Select(t => new
                   {
                       Team = t,
                       Goals = t.AwayGames.Sum(g => g.GuestGoals)
                   })
                   .AsEnumerable()
                   .Select(t => (t.Team, t.Goals))
                   .OrderByDescending(t => t.Goals)
                   .First();
        }

        public (Team Team, int Goals) GetTeamWihMostHomeGoals()
        {
            return _dbContext.Teams
                   .Select(t => new
                   {
                       Team = t,
                       Goals = t.HomeGames.Sum(g => g.HomeGoals)
                   })
                   .AsEnumerable()
                   .Select(t => (t.Team, t.Goals))
                   .OrderByDescending(t => t.Goals)
                   .First();
        }

        public (Team Team, int Ratio) GetTeamsWithHighestGoalRatio()
        {
            return _dbContext.Teams
                   .Select(t => new
                   {
                       Team = t,
                       Ratio = t.HomeGames.Sum(g => g.HomeGoals) - t.HomeGames.Sum(g => g.GuestGoals) +
                               t.AwayGames.Sum(g => g.HomeGoals) - t.AwayGames.Sum(g => g.GuestGoals)
                   })
                   .AsEnumerable()
                   .Select(t => (t.Team, t.Ratio))
                   .OrderByDescending(t => t.Ratio)
                   .First();
        }

        public IEnumerable<TeamStatisticDto> GetTeamStats()
        {
            return _dbContext.Teams
                   .Select(t => new TeamStatisticDto
                   {
                       Name = t.Name,
                       AvgGoalsShotAtHome = t.HomeGames.Average(g => g.HomeGoals),
                       AvgGoalsShotOutwards = t.AwayGames.Average(g => g.GuestGoals),
                       AvgGoalsShotInTotal = t.HomeGames.Select(g => new 
                                                                {
                                                                  GoalsShot = g.HomeGoals
                                                                })
                                                                .Concat(t.AwayGames.
                                                                        Select(g => new 
                                                                        {
                                                                            GoalsShot = g.GuestGoals
                                                                        }))
                                                                .Average(_ => _.GoalsShot),
                       AvgGoalsGotAtHome = t.HomeGames.Average(g => g.GuestGoals),
                       AvgGoalsGotOutwards = t.AwayGames.Average(g => g.HomeGoals),
                       AvgGoalsGotInTotal = t.HomeGames.Select(g => new 
                                                               {
                                                                    GoalsGot = g.GuestGoals
                                                               })
                                                               .Concat(t.AwayGames
                                                               .Select(g => new
                                                               {
                                                                    GoalsGot = g.HomeGoals
                                                               }))
                                                               .Average(_ => _.GoalsGot)
                   })
                   .OrderByDescending(t => t.AvgGoalsGotInTotal)
                   .ToArray();


        }
    }
}
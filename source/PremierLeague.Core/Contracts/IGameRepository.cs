using System.Collections.Generic;
using PremierLeague.Core.Entities;
using PremierLeague.Core.DataTransferObjects;

namespace PremierLeague.Core.Contracts
{
    public interface IGameRepository
    {
        void AddRange(IEnumerable<Game> games);
        IEnumerable<Game> GetAllWithTeams();
        void Add(Game game);
    }
}
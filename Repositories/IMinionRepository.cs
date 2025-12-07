using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public interface IMinionRepository : IRepository<Minion>
    {
        IEnumerable<Minion> GetMinionsByBase(int baseId);
        IEnumerable<Minion> GetMinionsByScheme(int schemeId);
        IEnumerable<Minion> GetMinionsBySpecialty(string specialty);
    }
}

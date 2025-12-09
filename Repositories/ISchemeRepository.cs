using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public interface ISchemeRepository : IRepository<EvilScheme>
    {
        IEnumerable<EvilScheme> GetSchemesByStatus(string status);
        IEnumerable<EvilScheme> GetOverdueSchemes();
    }
}

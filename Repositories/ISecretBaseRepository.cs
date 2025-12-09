using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public interface ISecretBaseRepository : IRepository<SecretBase>
    {
        IEnumerable<SecretBase> GetBasesByLocation(string location);
    }
}

using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Repositories
{
    public interface IEquipmentRepository : IRepository<Equipment>
    {
        IEnumerable<Equipment> GetEquipmentByScheme(int schemeId);
        IEnumerable<Equipment> GetEquipmentByBase(int baseId);
        IEnumerable<Equipment> GetEquipmentByCategory(string category);
    }
}

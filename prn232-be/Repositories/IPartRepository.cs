using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IPartRepository
    {
        IEnumerable<Part> GetAllParts();
        Part? GetPartById(int partId);
        void AddPart(Part part);
        void UpdatePart(Part part);
        void DeletePart(int partId);
        IEnumerable<Part> GetPartsFiltered(int categoryId, int supplierId);
    }
}

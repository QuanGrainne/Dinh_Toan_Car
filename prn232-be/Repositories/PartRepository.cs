using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class PartRepository : IPartRepository
    {
        public IEnumerable<Part> GetAllParts() => PartDAO.Instance.GetAllParts();

        public Part? GetPartById(int partId) => PartDAO.Instance.GetPartById(partId);

        public void AddPart(Part part) => PartDAO.Instance.AddPart(part);

        public void UpdatePart(Part part) => PartDAO.Instance.UpdatePart(part);

        public void DeletePart(int partId) => PartDAO.Instance.DeletePart(partId);

        public IEnumerable<Part> GetPartsFiltered(int categoryId, int supplierId) => PartDAO.Instance.GetPartsFiltered(categoryId, supplierId);
    }
}

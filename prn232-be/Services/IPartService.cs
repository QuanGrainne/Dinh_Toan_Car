using System.Collections.Generic;
using BusinessObjects.Models;
using BusinessObjects.ViewModels;

namespace Services
{
    public interface IPartService
    {
        IEnumerable<Part> GetAllParts();
        Part? GetPartById(int partId);
        void AddPart(Part part);
        void UpdatePart(Part part);
        void UpdatePartMetadata(UpdatePartViewModel model, int? adminId);
        bool HasTransactions(int partId);
        void DeletePart(int partId);
        IEnumerable<Part> GetPartsFiltered(int categoryId);
    }
}


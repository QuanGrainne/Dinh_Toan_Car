using BusinessObjects.DTOs;

namespace Services
{
    public interface IAppointmentConsumedPartService
    {
        void ReportIncurredPart(IncurredPartReportDto dto);
        void AddPart(IncurredPartReportDto dto);
        void ApproveIncurredPart(IncurredPartApprovalDto dto);
        void RemoveIncurredPart(int consumedPartId);
    }
}

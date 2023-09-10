using System.ComponentModel.DataAnnotations;

namespace DataToolKit.Models
{
    public class BatchControl
    {

        public int BatchId { get; set; }
        public string? StatusDescription { get; set; }
        public string? SubmitDate { get; set; }
        public string? SubmitName { get; set; }
              
        public string? VendorName { get; set; }

        public string? CustomerName { get; set; }
        public string? RequestTypeCode { get; set; }

        public string? DescriptionTitle { get; set; }
        public string? ReportTitle { get; set; }
        public string? ProjectCode { get; set; }
        public string? InputFileName { get; set; }
        public string? InputRecordCount { get; set; }
        public string? ResultEmail1 { get; set; }
        public string? ResultEmail2 { get; set; }
        public string? ResultEmail3 { get; set; }
        public string? ResultEmail4 { get; set; }

        public string? ResultEmail5 { get; set; }

        public int? OrginalBatchId { get; set; }

    }
}

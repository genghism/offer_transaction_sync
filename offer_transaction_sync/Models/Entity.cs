using offer_transaction_sync.Attributes;

namespace offer_transaction_sync.Models
{
    public class Entity(
        string _DocumentType,
        string _DocumentNumber,
        DateTime _ValidFrom,
        DateTime _ValidUntil,
        string _BusinessArea,
        string _Customer,
        decimal _TotalAmount,
        string _PriceList,
        string _Note,
        string _RejectReason,
        string _CompletionStatus,
        string _OrderStatus,
        string _PaymentCondition,
        string _ManagerCustomer,
        string _ManagerDocument,
        string _CreatedBy,
        DateTime _CreatedAt,
        string _ChangedBy,
        DateTime _ChangedAt)
    {
        [DataSource("document_type", true)]
        public string DocumentType { get; set; } = _DocumentType;

        [DataSource("document_number", true)]
        public string DocumentNumber { get; set; } = _DocumentNumber;

        [DataSource("valid_from", false)]
        public DateTime ValidFrom { get; set; } = _ValidFrom;

        [DataSource("valid_until", false)]
        public DateTime ValidUntil { get; set; } = _ValidUntil;

        [DataSource("business_area", false)]
        public string BusinessArea { get; set; } = _BusinessArea;

        [DataSource("customer", false)]
        public string Customer { get; set; } = _Customer;

        [DataSource("total_amount", false)]
        public decimal TotalAmount { get; set; } = _TotalAmount;

        [DataSource("price_list", false)]
        public string PriceList { get; set; } = _PriceList;

        [DataSource("note", false)]
        public string Note { get; set; } = _Note;

        [DataSource("reject_reason", false)]
        public string RejectReason { get; set; } = _RejectReason;

        [DataSource("completion_status", false)]
        public string CompletionStatus { get; set; } = _CompletionStatus;

        [DataSource("order_status", false)]
        public string OrderStatus { get; set; } = _OrderStatus;

        [DataSource("payment_condition", false)]
        public string PaymentCondition { get; set; } = _PaymentCondition;

        [DataSource("manager_customer", false)]
        public string ManagerCustomer { get; set; } = _ManagerCustomer;

        [DataSource("manager_document", false)]
        public string ManagerDocument { get; set; } = _ManagerDocument;

        [DataSource("created_by", false)]
        public string CreatedBy { get; set; } = _CreatedBy;

        [DataSource("created_at", false)]
        public DateTime CreatedAt { get; set; } = _CreatedAt;

        [DataSource("changed_by", false)]
        public string ChangedBy { get; set; } = _ChangedBy;

        [DataSource("changed_at", false)]
        public DateTime ChangedAt { get; set; } = _ChangedAt;
    }
}
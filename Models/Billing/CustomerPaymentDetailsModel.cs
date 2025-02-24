using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Billing
{
    public class CustomerPaymentDetailsModel
    {
        public List<PaymentHistory>? PaymentHistory { get; set; }
        public List<CardDetails>? PaymentMethods { get; set; }
        public List<ProductViewModel>? Products { get; set; }
        public string? StripePrimaryKey { get; set; }
    }

    public class PaymentHistory
    {
        public string? InvoiceId { get; set; }
        public string? PaymentId { get; set; }  // Unique Payment Intent ID from Stripe
        public double Amount { get; set; }    // Payment Amount (converted from cents to dollars)
        public string? Currency { get; set; }  // Currency Code (e.g., USD, EUR)
        public string? Status { get; set; }    // Payment Status (succeeded, incomplete, requires_action, etc.)
        public string? Description { get; set; } // Payment description (if available)
        public DateTime CreatedAt { get; set; } // Payment creation timestamp
        public string? PaymentMethod { get; set; } // Payment method ID (optional)
        public string? ReceiptUrl { get; set; }  // Stripe Receipt URL (optional)
    }

    public class CardDetails
    {
        public string? Id { get; set; }

        public string? Brand { get; set; }

        public string? Last4 { get; set; }

        public long ExpMonth { get; set; }

        public long ExpYear { get; set; }

        public bool IsDefault { get; set; }
    }

    public class ProductViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string?>? Images { get; set; }
        public Dictionary<string?, string?>? Metadata { get; set; }
        public List<PriceViewModel>? Prices { get; set; }
        public string? NextBillingDate { get; set; }
        public string? SubscriptionId { get; set; }
    }

    public class PriceViewModel
    {
        public string? Id { get; set; }
        public double UnitAmount { get; set; }
        public string? Currency { get; set; }
        public bool IsRecurring { get; set; }
    }
}

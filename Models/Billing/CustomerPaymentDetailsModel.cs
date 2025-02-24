using System;
using System.Collections.Generic;
using System.Globalization;
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
        public string CreatedAtView { get{ return Preferences.Default.Get("Lan", "en") == "ar" ? CreatedAt.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo("ar-AR")) : CreatedAt.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo("en-EN")); } } // Payment creation timestamp
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
        public string Amount { get { return (Prices != null && Prices.Count > 0) ? Prices.FirstOrDefault()!.UnitAmount.ToString() + " " + Prices.FirstOrDefault()!.Currency!.ToUpper() : ""; } set { } }

        //static string GetSubstringBetween(string input, char start, char end)
        //{
        //    int startIndex = input.IndexOf(start);
        //    int endIndex = input.IndexOf(end);

        //    if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        //    {
        //        return input.Substring(startIndex, endIndex - startIndex - 1);
        //    }
        //    else
        //    {
        //        return string.Empty; // Return empty if not found
        //    }
        //}
    }

    public class PriceViewModel
    {
        public string? Id { get; set; }
        public double UnitAmount { get; set; }
        public string? Currency { get; set; }
        public bool IsRecurring { get; set; }
    }

}

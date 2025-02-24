using Cardrly.ViewModels;

namespace Cardrly.Pages;
public partial class BillingPage : Controls.CustomControl
{
    public BillingPage(BillingViewModel model)
    {
        InitializeComponent();
        this.BindingContext = model;
    }
}
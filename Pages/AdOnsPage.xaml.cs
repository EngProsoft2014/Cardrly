using Cardrly.Controls;
using Cardrly.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace Cardrly.Pages;

public partial class AdOnsPage : Controls.CustomControl
{
    ADOnsViewModel viewModel;

    public AdOnsPage(ADOnsViewModel vm)
	{
		InitializeComponent();

        this.BindingContext = viewModel = vm;
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
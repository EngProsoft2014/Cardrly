using Cardrly.ViewModels.MeetingsAi;
using Mopups.Pages;
using Mopups.Services;


namespace Cardrly.Pages.MeetingsScript;

public partial class ConfirmRecordPopup : PopupPage
{

    RecordViewModel viewModel;

    public ConfirmRecordPopup(RecordViewModel vm)
    {
        InitializeComponent();
        this.BindingContext = viewModel = vm;
    }

    private async void OkButton_Clicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
        viewModel._audioService.Stop();
        await viewModel.StopRecording();
        await Navigation.PopAsync();
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
        await viewModel.ResetUi();
    }


    // 🚫 Disable background tap dismiss
    protected override bool OnBackgroundClicked() => true;

    // 🚫 Disable Android Back button dismiss
    protected override bool OnBackButtonPressed() => true;
}
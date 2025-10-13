using Cardrly.Models.MeetingAiActionRecord;
using Cardrly.ViewModels;
using Mopups.Services;
using Syncfusion.Maui.Core.Carousel;
using System.Globalization;

namespace Cardrly.Pages.MeetingsScript;

public partial class MeetingSettingPopup : Mopups.Pages.PopupPage
{
    NotesScriptDetailsViewModel viewModel;

    public MeetingSettingPopup(NotesScriptDetailsViewModel model)
	{
		InitializeComponent();
        this.BindingContext = viewModel = model;

        string Lan = Preferences.Default.Get("Lan", "en");

        if (Lan == "ar")
        {
            this.FlowDirection = FlowDirection.RightToLeft;
            CultureInfo.CurrentCulture = new CultureInfo("ar");
        }
        else
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            CultureInfo.CurrentCulture = new CultureInfo("en");
        }
    }


    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.IsEnabled = false;
        await MopupService.Instance.PopAsync();
        this.IsEnabled = true;
    }

    private void picLang_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedLanguage = picker.SelectedItem as string;

        viewModel.SelectedLanguage = selectedLanguage;

    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedScriptType = picker.SelectedItem as ScriptTypeModel;

        viewModel.SelectedScriptType = selectedScriptType;
    }
}
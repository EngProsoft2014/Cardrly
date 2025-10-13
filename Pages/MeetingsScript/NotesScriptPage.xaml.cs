
using Azure.AI.TextAnalytics;
using Cardrly.Helpers;
using Cardrly.Models.MeetingAiAction;
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using Microsoft.CognitiveServices.Speech;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace Cardrly.Pages.MeetingsScript;

public partial class NotesScriptPage : Controls.CustomControl
{
	NotesScriptViewModel viewModel;
    CancellationTokenSource? _cts;

    public NotesScriptPage(NotesScriptViewModel model)
	{
		InitializeComponent();
		this.BindingContext = viewModel = model;     
    }

    private void MeetingsSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        colListMeetings.ItemsSource = new ObservableCollection<MeetingAiActionResponse>(viewModel.LstMeetingModel.Where(x=> x.title.ToLower().Contains(e.NewTextValue.ToLower())).ToList());
    }


}
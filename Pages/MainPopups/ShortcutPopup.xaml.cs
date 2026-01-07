using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Models.Home;
using Cardrly.ViewModels;
using CommunityToolkit.Maui;
using Mopups.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Cardrly.Pages.MainPopups;

public partial class ShortcutPopup : Mopups.Pages.PopupPage
{

    public event Action<ObservableCollection<ShortcutItem>> ShortcutClose;

    public ShortcutPopup(HomeViewModel vm)
	{
		InitializeComponent();
        this.BindingContext = vm;
        Init();
    }

    void Init()
    {
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

    private async void DoneButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is HomeViewModel vm)
        {
            var selected = vm.Shortcuts.Where(x => x.IsChecked).ToList();
            ShortcutClose?.Invoke(new ObservableCollection<ShortcutItem>(selected));
        }


        await MopupService.Instance.PopAsync();
    }

}
using Cardrly.ViewModels;
using Plugin.NFC;
using System.Text;

namespace Cardrly.Pages;

public partial class ProfilePage : Controls.CustomControl
{
    public ProfilePage(ProfileViewModel model)
    {
        InitializeComponent();
        this.BindingContext = model;
    }
}
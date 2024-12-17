using Cardrly.ViewModels;
using Plugin.NFC;
using System.Text;

namespace Cardrly.Pages;

public partial class ProfilePage : Controls.CustomControl
{
    ProfileViewModel Model;
    public ProfilePage(ProfileViewModel model)
    {
        InitializeComponent();
        Model = model;
    }
}
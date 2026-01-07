using Cardrly.Constants;
using Cardrly.Helpers;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Cardrly.ViewModels
{
    public partial class ResetPasswordViewModel : BaseViewModel
    {
        new class ResetEmail
        {
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string? Email { get; set; }
        }

        #region Prop
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        [ObservableProperty]
        string reEmail;
        #endregion

        #region Service
        readonly IGenericRepository Rep;
        readonly Services.Data.ServicesService _service;
        #endregion

        #region Cons
        public ResetPasswordViewModel(IGenericRepository GenericRep, Services.Data.ServicesService service)
        {
            Rep = GenericRep;
            _service = service;
        }
        #endregion

        #region RelayCommand

        [RelayCommand]
        async Task SendEmail()
        {
            IsEnable = false;

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                ResetEmail model = new ResetEmail { Email = ReEmail };
                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (Regex.IsMatch(model.Email, pattern))
                    {
                        string UserToken = await _service.UserToken();
                        UserDialogs.Instance.ShowLoading();
                        var Postjson = await Rep.PostTRAsync<ResetEmail, ErrorResult>($"{ApiConstants.ForgetPasswordApi}", model!, UserToken);
                        UserDialogs.Instance.HideHud();
                        if (Postjson.Item2 == null)
                        {
                            await App.Current!.MainPage!.Navigation.PopAsync();
                        }
                        else
                        {
                            if (Postjson.Item2 != null)
                            {
                                await Toast.Make($"{Postjson.Item2!.errors!.Values.FirstOrDefault()}", CommunityToolkit.Maui.Core.ToastDuration.Long, 15).Show();
                            }
                        }
                    }
                    else
                    {
                        var toast = Toast.Make(Cardrly.Resources.Lan.AppResources.enter_vaild_Email, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                        await toast.Show();
                    }
                }
                else
                {
                    var toast = Toast.Make(Cardrly.Resources.Lan.AppResources.msgFREmail, CommunityToolkit.Maui.Core.ToastDuration.Long, 15);
                    await toast.Show();
                }
            }

            IsEnable = true;

        }
        #endregion
    }
}

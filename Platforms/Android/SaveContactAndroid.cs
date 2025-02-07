using Android.Content;
using Android.Provider;
using Cardrly.Models.Lead;
using Cardrly.Resources.Lan;
using Cardrly.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp = Microsoft.Maui.ApplicationModel;

namespace Cardrly
{
    public class SaveContactAndroid : ISaveContact
    {

        public async Task SaveContactMethod(LeadResponse contact)
        {
            try
            {
                var intent = new Intent(Intent.ActionInsert);
                intent.SetType(ContactsContract.Contacts.ContentType);

                intent.PutExtra(ContactsContract.Intents.Insert.Name, contact.FullName);
                intent.PutExtra(ContactsContract.Intents.Insert.JobTitle, contact.JobTitle);
                intent.PutExtra(ContactsContract.Intents.Insert.Phone, contact.Phone);
                intent.PutExtra(ContactsContract.Intents.Insert.Email, contact.Email);
                intent.PutExtra(ContactsContract.Intents.Insert.Postal, contact.Address);
                intent.PutExtra(ContactsContract.Intents.Insert.Company, contact.Company);

                intent.SetFlags(ActivityFlags.NewTask);
                MauiApp.Platform.CurrentActivity.StartActivity(intent);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgcontactwasnotsaved}", $"{AppResources.msgOk}");
            }
        }

    }

}

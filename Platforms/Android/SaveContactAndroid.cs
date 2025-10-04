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
            if (!string.IsNullOrEmpty(contact.FullName) && !string.IsNullOrEmpty(contact.Phone))
            {
                try
                {
                    var intent = new Intent(Intent.ActionInsert);
                    intent.SetType(ContactsContract.Contacts.ContentType);

                    intent.PutExtra(ContactsContract.Intents.Insert.Name, contact.FullName);//(Required)
                    intent.PutExtra(ContactsContract.Intents.Insert.JobTitle, contact.JobTitle ?? string.Empty);
                    intent.PutExtra(ContactsContract.Intents.Insert.Phone, contact.Phone);//(Required)
                    intent.PutExtra(ContactsContract.Intents.Insert.Email, contact.Email ?? string.Empty);
                    intent.PutExtra(ContactsContract.Intents.Insert.Postal, contact.Address ?? string.Empty);
                    intent.PutExtra(ContactsContract.Intents.Insert.Company, contact.Company ?? string.Empty);

                    intent.SetFlags(ActivityFlags.NewTask);
                    MauiApp.Platform.CurrentActivity.StartActivity(intent);
                }
                catch (Exception ex)
                {
                    await Application.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgcontactwasnotsaved}", $"{AppResources.msgOk}");
                }
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert($"{AppResources.msgWarning}", $"{AppResources.msgFullname_and_phone_numbe_fields_required}", $"{AppResources.msgOk}");
            }
        }

    }

}

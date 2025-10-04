using Cardrly.Models.Lead;
using Cardrly.Resources.Lan;
using Cardrly.Services;
using Contacts;
using Foundation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cardrly
{
    public class SaveContactiOS : ISaveContact
    {
        public async Task SaveContactMethod(LeadResponse contact)
        {
            if(!string.IsNullOrEmpty(contact.FullName) && !string.IsNullOrEmpty(contact.Phone))
            {
                try
                {
                    var newContact = new CNMutableContact
                    {
                        GivenName = contact.FullName, //(Required)
                        JobTitle = contact.JobTitle ?? string.Empty,
                        OrganizationName = contact.Website ?? string.Empty,
                        PostalAddresses = new CNLabeledValue<CNPostalAddress>[]
                        {
                        new CNLabeledValue<CNPostalAddress>(CNLabelKey.Home, new CNMutablePostalAddress()
                        {
                            City = contact.Address ?? string.Empty
                        })
                        },
                        UrlAddresses = new CNLabeledValue<NSString>[]
                        {
                        new CNLabeledValue<NSString>(CNLabelKey.Home, new NSString(contact.Website ?? string.Empty))
                        },
                        PhoneticOrganizationName = contact.Company ?? string.Empty,
                    };

                    // Add phone number (Required)
                    newContact.PhoneNumbers = new[]
                    {
                        new CNLabeledValue<CNPhoneNumber>(CNLabelKey.Home, new CNPhoneNumber(contact.Phone!))
                    };

                    // Add email
                    if (!string.IsNullOrWhiteSpace(contact.Email))
                    {
                        newContact.EmailAddresses = new[]
                        {
                        new CNLabeledValue<NSString>(CNLabelKey.Home, new NSString(contact.Email))
                    };
                    }

                    var store = new CNContactStore();
                    var saveRequest = new CNSaveRequest();
                    saveRequest.AddContact(newContact, store.DefaultContainerIdentifier);

                    NSError error = null;
                    await Task.Run(() => store.ExecuteSaveRequest(saveRequest, out error));

                    if (error != null)
                    {
                        throw new Exception(error.LocalizedDescription);
                    }

                    await Application.Current!.MainPage!.DisplayAlert($"{AppResources.msgSuccess}", $"{AppResources.msgContactSaved}", $"{AppResources.msgOk}");
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

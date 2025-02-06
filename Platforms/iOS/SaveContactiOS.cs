using Cardrly.Models.Lead;
using Cardrly.Services;
using Contacts;
using Foundation;
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
            
            try
            {
                var newContact = new CNMutableContact
                {
                    GivenName = contact.FullName,
                    JobTitle = contact.JobTitle,
                    OrganizationName = contact.Website
                };

                // Add phone number
                if (!string.IsNullOrWhiteSpace(contact.Phone))
                {
                    newContact.PhoneNumbers = new[]
                    {
                    new CNLabeledValue<CNPhoneNumber>(CNLabelKey.Home, new CNPhoneNumber(contact.Phone))
                };
                }

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

                await Application.Current!.MainPage!.DisplayAlert("Success", "Contact saved!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}

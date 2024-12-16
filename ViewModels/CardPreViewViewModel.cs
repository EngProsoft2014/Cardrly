using CommunityToolkit.Mvvm.ComponentModel;
using Cardrly.Mode_s.Card;

namespace Cardrly.ViewModels
{
    public partial class CardPreViewViewModel : BaseViewModel
    {
        [ObservableProperty]
        CardResponse card = new CardResponse();
        public CardPreViewViewModel(CardResponse _Card)
        {
            Card = _Card;
        }
    }
}

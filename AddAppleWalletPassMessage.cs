using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Cardrly
{
    public class AddAppleWalletPassMessage : ValueChangedMessage<byte[]>
    {
        public AddAppleWalletPassMessage(byte[] value) : base(value) { }
    }
}

using MessagePack;
using MessagePack.Resolvers;
using Shared.MessagePackFormatters;

namespace Shared.Helpers;

public static class MessagePackHelper
{
    public static void SetupMessagePack()
    {
        var formatterResolver = StaticCompositeResolver.Instance;
        formatterResolver.Register(new Vector2Formatter());
        
        var resolver = CompositeResolver.Create(
            formatterResolver,
            ContractlessStandardResolver.Instance,
            StandardResolver.Instance
        );
        
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(resolver)
            .WithSecurity(MessagePackSecurity.UntrustedData); 

        MessagePackSerializer.DefaultOptions = options;
    }
}
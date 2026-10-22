using GraphQL.Types;
using VirtoCommerce.Passkeys.Xapi.Commands;

namespace VirtoCommerce.Passkeys.Xapi.Schemas;

public class InputCreatePasskeyType : InputObjectGraphType<CreatePasskeyCommand>
{
    public InputCreatePasskeyType()
    {
        Field(x => x.Credential);
        Field(x => x.Name, nullable: true);
    }
}

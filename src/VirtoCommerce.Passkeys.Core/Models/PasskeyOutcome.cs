namespace VirtoCommerce.Passkeys.Core.Models;

public enum PasskeyOutcome
{
    Undefined,
    Success,
    PasskeyDisabled,
    InvalidPasskey,
    AccountLocked,
}

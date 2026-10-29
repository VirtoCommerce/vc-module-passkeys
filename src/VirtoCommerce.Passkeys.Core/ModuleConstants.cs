namespace VirtoCommerce.Passkeys.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public const string SignInType = "Passkey";
        public const string GrantType = "passkey";

        public static class Parameters
        {
            public const string Credential = "credential";
        }

        public static class FailureReason
        {
            public const string InvalidPasskey = "InvalidPasskey";
        }

        public static class KeyState
        {
            public const string DeviceOnly = "DeviceOnly";
            public const string BackedUp = "BackedUp";
            public const string NotBackedUp = "NotBackedUp";
        }
    }
}

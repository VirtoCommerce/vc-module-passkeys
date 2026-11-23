export function isPasskeySupported(): boolean {
  return (
    typeof PublicKeyCredential !== "undefined" &&
    "parseCreationOptionsFromJSON" in PublicKeyCredential &&
    "parseRequestOptionsFromJSON" in PublicKeyCredential
  );
}

export async function isPasskeyAutofillSupported(): Promise<boolean> {
  return (
    isPasskeySupported() &&
    "isConditionalMediationAvailable" in PublicKeyCredential &&
    PublicKeyCredential.isConditionalMediationAvailable()
  );
}

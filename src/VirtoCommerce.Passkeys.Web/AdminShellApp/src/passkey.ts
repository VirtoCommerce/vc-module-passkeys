// The browser side of the "Passkey" sign-in method in the Admin Shell, as Scripts/module.js is in the AngularJS admin:
// WebAuthn options from api/passkeys, the browser's passkey prompt, then the platform's cookie sign-in (grant type
// "passkey") or the module's POST api/passkeys to save a new passkey.
import { i18n } from "@vc-shell/framework";

/** A key of the user's (the shell's SignInKey); a passkey's state says where it lives */
interface SignInKey {
  id: string;
  name?: string;
  state?: string;
}

/** The part of the shell's sign-in methods ($signInMethods) this plugin uses */
export interface SignInMethodsHost {
  extend: (
    name: string,
    extension: {
      title?: () => string;
      signInTitle?: () => string;
      icon?: string;
      signIn?: () => Promise<boolean | void>;
      createKey?: () => Promise<boolean | void>;
      createKeyTitle?: () => string;
      describeKey?: (key: SignInKey) => string | undefined;
      keyIcon?: (key: SignInKey) => string | undefined;
    },
  ) => void;
  signIn: (parameters: Record<string, string>) => Promise<void>;
}

const t = (key: string) => i18n.global.t(key);

// a passkey's backup state (the platform's SignInKey.state): kept on this device, or synced by its password manager
const STATES: Record<string, { icon: string; title: string }> = {
  DeviceOnly: { icon: "lucide-laptop", title: "PASSKEYS.STATES.DEVICE_ONLY" },
  BackedUp: { icon: "lucide-cloud", title: "PASSKEYS.STATES.BACKED_UP" },
  NotBackedUp: { icon: "lucide-cloud-off", title: "PASSKEYS.STATES.NOT_BACKED_UP" },
};

/** The JSON forms of WebAuthn options are parsed by the browser itself; older browsers have no passkey button */
export function isPasskeySupported() {
  const credential = window.PublicKeyCredential as
    | (typeof PublicKeyCredential & { parseCreationOptionsFromJSON?: unknown; parseRequestOptionsFromJSON?: unknown })
    | undefined;
  return (
    !!credential &&
    !!navigator.credentials &&
    typeof credential.parseCreationOptionsFromJSON === "function" &&
    typeof credential.parseRequestOptionsFromJSON === "function"
  );
}

// the user closed or timed out the browser's prompt (or no passkey was picked): not an error to show
const isCancelled = (error: unknown) => error instanceof DOMException && (error.name === "NotAllowedError" || error.name === "AbortError");

async function post(url: string, body?: unknown) {
  return fetch(url, {
    method: "POST",
    headers: { Accept: "application/json", ...(body === undefined ? {} : { "Content-Type": "application/json" }) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
}

async function options(url: string) {
  const response = await post(url);
  if (!response.ok) throw new Error(`${response.status} ${response.statusText}`);
  return response.json();
}

async function signIn(host: SignInMethodsHost) {
  const json = await options("/api/passkeys/request-options");
  let credential: Credential | null;
  try {
    credential = await navigator.credentials.get({ publicKey: PublicKeyCredential.parseRequestOptionsFromJSON(json) });
  } catch (error) {
    if (isCancelled(error)) return false;
    throw error;
  }
  if (!credential) return false;
  // the platform checks the assertion and signs in with a cookie; lockout and the platform's sign-in rules apply
  await host.signIn({ grant_type: "passkey", credential: JSON.stringify(credential) });
  return true;
}

async function createKey() {
  const json = await options("/api/passkeys/creation-options");
  let credential: Credential | null;
  try {
    credential = await navigator.credentials.create({ publicKey: PublicKeyCredential.parseCreationOptionsFromJSON(json) });
  } catch (error) {
    if (isCancelled(error)) return false;
    // the authenticator already holds a passkey of this account (excludeCredentials)
    if (error instanceof DOMException && error.name === "InvalidStateError") throw new Error(t("PASSKEYS.ERRORS.ALREADY_REGISTERED"));
    throw error;
  }
  if (!credential) return false;
  // the name comes from the authenticator (its AAGUID) on the server, as in the AngularJS admin; it can be renamed later
  const response = await post("/api/passkeys", { credential: JSON.stringify(credential) });
  if (!response.ok) {
    const result = (await response.json().catch(() => undefined)) as { errors?: (string | { description?: string })[] } | undefined;
    const message = (result?.errors ?? []).map((e) => (typeof e === "string" ? e : e.description)).filter(Boolean).join("\n");
    throw new Error(message || t("PASSKEYS.ERRORS.SAVE_FAILED"));
  }
  return true;
}

/** Adds the passkey to the shell's sign-in methods: its title and how its keys read always, signing in and adding where the browser can */
export function extendPasskey(host: SignInMethodsHost) {
  const supported = isPasskeySupported();
  host.extend("Passkey", {
    title: () => t("PASSKEYS.TITLE"),
    signInTitle: () => t("PASSKEYS.SIGN_IN"),
    icon: "lucide-fingerprint",
    signIn: supported ? () => signIn(host) : undefined,
    createKey: supported ? createKey : undefined,
    createKeyTitle: () => t("PASSKEYS.ADD"),
    describeKey: (key) => (key.state && STATES[key.state] ? t(STATES[key.state].title) : undefined),
    keyIcon: (key) => (key.state ? STATES[key.state]?.icon : undefined) ?? "lucide-fingerprint",
  });
}

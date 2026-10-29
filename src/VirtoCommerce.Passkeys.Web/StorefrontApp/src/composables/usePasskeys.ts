import { Logger, useErrorsTranslator, useNotifications } from "@vc-frontend/core";
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useCreatePasskeyMutation, getPasskeyCreationOptions } from "../api/graphql";
import { PASSKEY_STATE_ICONS } from "../constants";
import { isPasskeyPromptCancelled, isPasskeySupported } from "../utils";
import type { PasskeyStateType } from "../constants";
import type { IdentityErrorType, SignInKeyType, SignInMethodContributionType } from "@vc-frontend/core";

export function usePasskeys(): SignInMethodContributionType {
  const { t, te } = useI18n();
  const notifications = useNotifications();
  const { translate: translatePasskeyError } = useErrorsTranslator<IdentityErrorType>("passkeys.identity_error");
  const { translate: translateIdentityError } = useErrorsTranslator<IdentityErrorType>("identity_error");

  const { mutate: createPasskeyMutation } = useCreatePasskeyMutation();

  const creating = ref(false);

  async function createPasskey(): Promise<void> {
    creating.value = true;

    try {
      const options = await getPasskeyCreationOptions();
      if (!options) {
        return;
      }

      const publicKey = PublicKeyCredential.parseCreationOptionsFromJSON(JSON.parse(options));
      const credential = await navigator.credentials.create({ publicKey });
      if (!credential) {
        return;
      }

      const identityResult = (await createPasskeyMutation({ command: { credential: JSON.stringify(credential) } }))
        ?.data?.createPasskey;

      if (!identityResult?.succeeded) {
        showError(identityResult);
      }
    } catch (e) {
      if (isPasskeyPromptCancelled(e)) {
        return;
      }

      if (e instanceof DOMException && e.name === "InvalidStateError") {
        notifications.error({ text: t("passkeys.security_page.passkeys.already_registered") });

        return;
      }

      Logger.error(`${usePasskeys.name}.${createPasskey.name}`, e);
      showError();
    } finally {
      creating.value = false;
    }
  }

  function getKeyIcon(key: SignInKeyType): string {
    return (key.state && PASSKEY_STATE_ICONS[key.state as PasskeyStateType]) || "key";
  }

  function getKeyDetails(key: SignInKeyType): string[] {
    const stateKey = `passkeys.security_page.passkeys.states.${key.state}`;

    return key.state && te(stateKey) ? [t(stateKey)] : [];
  }

  function showError(identityResult?: { errors?: IdentityErrorType[] }): void {
    const error = identityResult?.errors?.[0];
    const translate =
      error && te(`passkeys.identity_error.${error.code}`) ? translatePasskeyError : translateIdentityError;

    notifications.error({
      text: (error && translate(error)) || t("common.messages.something_went_wrong"),
    });
  }

  return {
    title: () => t("passkeys.security_page.passkeys.title"),
    description: () => t("passkeys.security_page.passkeys.description"),
    noKeysText: () => t("passkeys.security_page.passkeys.no_passkeys"),
    createKeyText: () => t("passkeys.security_page.passkeys.add_button"),
    creating,
    createKey: isPasskeySupported() ? createPasskey : undefined,
    getKeyIcon,
    getKeyDetails,
  };
}

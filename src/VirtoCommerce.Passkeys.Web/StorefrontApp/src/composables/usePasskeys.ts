import { globals, Logger, SUPPRESS_ERROR_NOTIFICATIONS_CONTEXT } from "@vc-frontend/core";
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useCreatePasskeyMutation, getPasskeyCreationOptions } from "../api/graphql";
import { PASSKEY_STATE_ICONS } from "../constants";
import { isPasskeySupported } from "../utils";
import { usePasskeyErrors } from "./usePasskeyErrors";
import type { PasskeyStateType } from "../constants";
import type { SignInKeyType, SignInMethodContributionType } from "@vc-frontend/core";

export function usePasskeys(): SignInMethodContributionType {
  const { t, te } = useI18n();
  const { handleExpectedPasskeyError, showError } = usePasskeyErrors();

  const { mutate: createPasskeyMutation } = useCreatePasskeyMutation();

  const creating = ref(false);

  async function createPasskey(): Promise<void> {
    creating.value = true;

    try {
      const options = await getPasskeyCreationOptions(globals.storeId, SUPPRESS_ERROR_NOTIFICATIONS_CONTEXT);
      if (!options) {
        return;
      }

      const publicKey = PublicKeyCredential.parseCreationOptionsFromJSON(JSON.parse(options));
      const credential = await navigator.credentials.create({ publicKey });
      if (!credential) {
        return;
      }

      const identityResult = (
        await createPasskeyMutation({ command: { storeId: globals.storeId, credential: JSON.stringify(credential) } })
      )?.data?.createPasskey;

      if (!identityResult?.succeeded) {
        showError(identityResult?.errors?.[0]);
      }
    } catch (e) {
      if (handleExpectedPasskeyError(e)) {
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

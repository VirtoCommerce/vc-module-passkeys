import {
  globals,
  Logger,
  useAnalytics,
  useAuth,
  useErrorsTranslator,
  useNotifications,
  useSignMeIn,
} from "@vc-frontend/core";
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { getPasskeyRequestOptions } from "../api/graphql";
import { isPasskeyPromptCancelled } from "../utils";
import type { IdentityErrorType } from "@vc-frontend/core";

type PasskeySignInOptionsType = Omit<CredentialRequestOptions, "publicKey"> & { returnUrl?: string };

const ANALYTICS_LOGIN_METHOD = "passkey";
const GRANT_TYPE = "passkey";

export function usePasskeySignIn() {
  const { t } = useI18n();
  const { authorizeWithGrant, errors: authErrors } = useAuth();
  const { signIn: signMeIn } = useSignMeIn();
  const { translate } = useErrorsTranslator<IdentityErrorType>("shared.account.sign_in_form.errors");
  const { analytics } = useAnalytics();
  const notifications = useNotifications();

  const loading = ref(false);

  async function signIn(options: PasskeySignInOptionsType = {}): Promise<void> {
    if (loading.value) {
      return;
    }

    const { returnUrl, ...requestOptions } = options;
    // An autofill request waits until the user picks a passkey, so it must not block the button meanwhile
    let isLoadingOwner = requestOptions.mediation !== "conditional";
    loading.value = isLoadingOwner;

    try {
      const credential = await getCredential(requestOptions);
      if (!credential) {
        return;
      }

      isLoadingOwner = true;
      loading.value = true;

      await authorizeWithGrant(
        new URLSearchParams({
          grant_type: GRANT_TYPE,
          scope: "offline_access",
          storeId: globals.storeId,
          credential: JSON.stringify(credential),
        }),
      );

      await signMeIn({ returnUrl });

      analytics("login", ANALYTICS_LOGIN_METHOD, { success: true });
    } catch (e) {
      if (isPasskeyPromptCancelled(e) || requestOptions.signal?.aborted) {
        return;
      }

      const error = authErrors.value?.[0];

      Logger.error(`${usePasskeySignIn.name}.${signIn.name}`, e);
      analytics("login", ANALYTICS_LOGIN_METHOD, { success: false, errors: error?.code ?? String(e) });

      notifications.error({
        duration: 5000,
        single: true,
        text: (error && translate(error)) || t("common.messages.something_went_wrong"),
      });
    } finally {
      if (isLoadingOwner) {
        loading.value = false;
      }
    }
  }

  async function getCredential(
    requestOptions: Omit<CredentialRequestOptions, "publicKey">,
  ): Promise<Credential | null> {
    const options = await getPasskeyRequestOptions();
    if (!options) {
      return null;
    }

    const publicKey = PublicKeyCredential.parseRequestOptionsFromJSON(JSON.parse(options));

    return navigator.credentials.get({ ...requestOptions, publicKey });
  }

  return {
    loading,
    signIn,
  };
}

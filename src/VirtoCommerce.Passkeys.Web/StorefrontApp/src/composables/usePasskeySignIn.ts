import {
  globals,
  Logger,
  SUPPRESS_ERROR_NOTIFICATIONS_CONTEXT,
  useAnalytics,
  useAuth,
  useSignMeIn,
} from "@vc-frontend/core";
import { ref } from "vue";
import { getPasskeyRequestOptions } from "../api/graphql";
import { usePasskeyErrors } from "./usePasskeyErrors";

type PasskeySignInOptionsType = Omit<CredentialRequestOptions, "publicKey"> & { returnUrl?: string };

const ANALYTICS_LOGIN_METHOD = "passkey";
const GRANT_TYPE = "passkey";

export function usePasskeySignIn() {
  const { authorizeWithGrant, errors: authErrors } = useAuth();
  const { signIn: signMeIn } = useSignMeIn();
  const { analytics } = useAnalytics();
  const { handleExpectedPasskeyError, showError } = usePasskeyErrors();

  const loading = ref(false);

  async function signIn(options: PasskeySignInOptionsType = {}): Promise<void> {
    if (loading.value) {
      return;
    }

    const { returnUrl, ...requestOptions } = options;
    const isUserInitiated = requestOptions.mediation !== "conditional";
    // An autofill request waits until the user picks a passkey, so it must not block the button meanwhile
    let isLoadingOwner = isUserInitiated;
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
      if (requestOptions.signal?.aborted || handleExpectedPasskeyError(e, { notify: isUserInitiated })) {
        return;
      }

      const error = authErrors.value?.[0];

      Logger.error(`${usePasskeySignIn.name}.${signIn.name}`, e);
      analytics("login", ANALYTICS_LOGIN_METHOD, { success: false, errors: error?.code ?? String(e) });

      showError(error);
    } finally {
      if (isLoadingOwner) {
        loading.value = false;
      }
    }
  }

  async function getCredential(
    requestOptions: Omit<CredentialRequestOptions, "publicKey">,
  ): Promise<Credential | null> {
    const options = await getPasskeyRequestOptions(globals.storeId, SUPPRESS_ERROR_NOTIFICATIONS_CONTEXT);
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

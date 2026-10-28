import { defineAsyncComponent } from "vue";
import { EXTENSION_CATEGORIES, useExtensionRegistry } from "@vc-frontend/core";
import { usePasskeys } from "./composables/usePasskeys";
import { PASSKEY_SIGN_IN_METHOD } from "./constants";
import { loadLocales } from "./i18n";
import { isPasskeySupported } from "./utils";

export function init(): void {
  const { register, registerContribution } = useExtensionRegistry();

  if (isPasskeySupported()) {
    register(EXTENSION_CATEGORIES.identityProviders, PASSKEY_SIGN_IN_METHOD, {
      component: defineAsyncComponent(() => import("./components/passkey-provider.vue")),
    });
  }

  registerContribution(EXTENSION_CATEGORIES.signInMethods, PASSKEY_SIGN_IN_METHOD, { use: usePasskeys });
  loadLocales();
}

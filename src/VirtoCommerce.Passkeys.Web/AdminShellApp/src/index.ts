import { defineAppModule } from "@vc-shell/framework";
import type { App } from "vue";
import * as locales from "./locales";
import { extendPasskey, type SignInMethodsHost } from "./passkey";

// The module has no admin screens; its plugin adds the passkey to the Admin Shell's sign-in methods: a "Sign in with a
// passkey" button on the sign-in page and "Add a passkey" in the profile's sign-in methods, next to the AngularJS ones
const appModule = defineAppModule({ locales });

export default {
  install(app: App) {
    appModule.install(app);
    // an Admin Shell without sign-in methods has nothing to extend
    const host = app.config.globalProperties.$signInMethods as SignInMethodsHost | undefined;
    if (host) extendPasskey(host);
  },
};

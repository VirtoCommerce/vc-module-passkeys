import { definePluginManifest } from "@vc-frontend/core/manifest";

// What the storefront knows before running this plugin. It does not replace the registrations in
// src/index.ts. Reference: https://github.com/VirtoCommerce/vc-frontend/blob/dev/client-app/core/federation/declarations.md
export default definePluginManifest({
  // An off switch per store, without a release: add a public store setting to the module, import settingEnabled
  // above, and uncomment. While the setting is missing or false, nothing of the plugin loads.
  // when: settingEnabled("VirtoCommerce.Passkeys", "Passkeys.StorefrontPlugin.Enabled"),
  slots: [{ at: "identityProviders/Passkey", policy: "reserve" }],
});

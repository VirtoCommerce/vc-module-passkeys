import path from "node:path";
import { getDynamicModuleConfiguration } from "@vc-shell/mf-module";

// Builds the passkeys UI of the Admin Shell as a Module Federation remote of its host.
// Output: <module>/plugins/admin-shell/ (remoteEntry.js + public/plugin.json),
// served by the platform at /modules/$(VirtoCommerce.Passkeys)/plugins/admin-shell/.
const base = "/modules/$(VirtoCommerce.Passkeys)/plugins/admin-shell/";
process.env.APP_BASE_PATH = base;

export default getDynamicModuleConfiguration({
  appId: "admin-shell",
  moduleRoot: path.resolve(__dirname, ".."),
  entry: "./src/index.ts",
  remoteName: "passkeys_admin",
  base,
});

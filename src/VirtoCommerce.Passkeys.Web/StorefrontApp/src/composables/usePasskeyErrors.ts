import { ApolloError } from "@apollo/client";
import { useErrorsTranslator, useNotifications } from "@vc-frontend/core";
import { useI18n } from "vue-i18n";
import type { IdentityErrorType } from "@vc-frontend/core";

const ERROR_LOCALE_GROUPS = ["passkeys.identity_error", "shared.account.sign_in_form.errors", "identity_error"];

export function usePasskeyErrors() {
  const { t, te } = useI18n();
  const notifications = useNotifications();

  const translators = ERROR_LOCALE_GROUPS.map((group) => ({
    group,
    translate: useErrorsTranslator<IdentityErrorType>(group).translate,
  }));

  function handleExpectedPasskeyError(error: unknown, { notify = true }: { notify?: boolean } = {}): boolean {
    if (isPasskeyPromptCancelled(error)) {
      return true;
    }

    if (isPasskeyAlreadyRegistered(error)) {
      if (notify) {
        showNotification(t("passkeys.security_page.passkeys.already_registered"));
      }
      return true;
    }

    if (isPasskeysDisabledError(error)) {
      if (notify) {
        showNotification(t("passkeys.identity_error.passkeys_disabled"));
      }
      return true;
    }

    return false;
  }

  function showError(error?: IdentityErrorType): void {
    showNotification(error && translateError(error));
  }

  function translateError(error: IdentityErrorType): string | undefined {
    const translator = translators.find(({ group }) => te(`${group}.${error.code}`));

    return translator ? translator.translate(error) : error.description;
  }

  function showNotification(text?: string): void {
    notifications.error({
      duration: 5000,
      group: "Passkeys",
      singleInGroup: true,
      text: text || t("common.messages.something_went_wrong"),
    });
  }

  return { handleExpectedPasskeyError, showError };
}

function isPasskeyPromptCancelled(error: unknown): boolean {
  return error instanceof DOMException && error.name === "NotAllowedError";
}

function isPasskeyAlreadyRegistered(error: unknown): boolean {
  return error instanceof DOMException && error.name === "InvalidStateError";
}

function isPasskeysDisabledError(error: unknown): boolean {
  return (
    error instanceof ApolloError &&
    error.graphQLErrors.some(({ extensions }) => extensions?.code === "passkeys_disabled")
  );
}

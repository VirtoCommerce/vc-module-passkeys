<template>
  <IdentityProvider
    v-if="te('passkeys.sign_in.button_text')"
    :text="$t('passkeys.sign_in.button_text')"
    :icon="passkeyIcon"
    :loading="loading"
    @sign-in="signInWithButton"
  />
</template>

<script setup lang="ts">
import { IdentityProvider } from "@vc-frontend/core";
import { onMounted, onUnmounted } from "vue";
import { useI18n } from "vue-i18n";
import passkeyIcon from "../assets/passkey.svg";
import { usePasskeySignIn } from "../composables/usePasskeySignIn";
import { isPasskeyAutofillSupported } from "../utils";

interface IProps {
  returnUrl: string;
}

const props = defineProps<IProps>();

const { te } = useI18n();
const { loading, signIn } = usePasskeySignIn();

let isUnmounted = false;
let autofillAbortController: AbortController | undefined;

onMounted(startAutofillSignIn);

onUnmounted(() => {
  isUnmounted = true;
  stopAutofillSignIn();
});

async function startAutofillSignIn(): Promise<void> {
  if (!(await isPasskeyAutofillSupported()) || isUnmounted) {
    return;
  }

  autofillAbortController = new AbortController();
  await signIn({ returnUrl: props.returnUrl, mediation: "conditional", signal: autofillAbortController.signal });
}

function stopAutofillSignIn(): void {
  autofillAbortController?.abort();
}

async function signInWithButton(): Promise<void> {
  // The browser keeps only one WebAuthn request pending at a time
  stopAutofillSignIn();

  await signIn({ returnUrl: props.returnUrl });
  await startAutofillSignIn();
}
</script>

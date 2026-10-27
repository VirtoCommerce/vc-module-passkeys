export const PASSKEY_SIGN_IN_METHOD = "Passkey";

export type PasskeyStateType = "DeviceOnly" | "BackedUp" | "NotBackedUp";

export const PASSKEY_STATE_ICONS: Record<PasskeyStateType, string> = {
  DeviceOnly: "laptop",
  BackedUp: "cloud",
  NotBackedUp: "cloud",
};

import { useMutation } from "@vue/apollo-composable";
import { CreatePasskeyDocument } from "../../types";

export function useCreatePasskeyMutation() {
  return useMutation(CreatePasskeyDocument);
}

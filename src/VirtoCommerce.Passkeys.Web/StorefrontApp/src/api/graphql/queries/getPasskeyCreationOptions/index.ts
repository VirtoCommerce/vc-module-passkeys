import { graphqlClient } from "@vc-frontend/core";
import { GetPasskeyCreationOptionsDocument } from "../../types";

export async function getPasskeyCreationOptions(): Promise<string | undefined> {
  const { data } = await graphqlClient.query({
    query: GetPasskeyCreationOptionsDocument,
    fetchPolicy: "no-cache",
  });

  return data.passkeyCreationOptions ?? undefined;
}

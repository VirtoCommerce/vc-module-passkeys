import { graphqlClient } from "@vc-frontend/core";
import { GetPasskeyRequestOptionsDocument } from "../../types";

export async function getPasskeyRequestOptions(): Promise<string | undefined> {
  const { data } = await graphqlClient.query({
    query: GetPasskeyRequestOptionsDocument,
    fetchPolicy: "no-cache",
  });

  return data.passkeyRequestOptions ?? undefined;
}

import { graphqlClient } from "@vc-frontend/core";
import { GetPasskeyRequestOptionsDocument } from "../../types";
import type { DefaultContext } from "@apollo/client/core";

export async function getPasskeyRequestOptions(storeId: string, context?: DefaultContext): Promise<string | undefined> {
  const { data } = await graphqlClient.query({
    query: GetPasskeyRequestOptionsDocument,
    variables: { storeId },
    fetchPolicy: "no-cache",
    context,
  });

  return data.passkeyRequestOptions ?? undefined;
}

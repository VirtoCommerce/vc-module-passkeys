import { graphqlClient } from "@vc-frontend/core";
import { GetPasskeyCreationOptionsDocument } from "../../types";
import type { DefaultContext } from "@apollo/client/core";

export async function getPasskeyCreationOptions(
  storeId: string,
  context?: DefaultContext,
): Promise<string | undefined> {
  const { data } = await graphqlClient.query({
    query: GetPasskeyCreationOptionsDocument,
    variables: { storeId },
    fetchPolicy: "no-cache",
    context,
  });

  return data.passkeyCreationOptions ?? undefined;
}

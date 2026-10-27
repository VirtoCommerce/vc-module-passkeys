// This file is auto-generated. Do not edit manually.

import type { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T;
export type InputMaybe<T> = T;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
};

export type IdentityErrorType = {
  code?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
};

export type IdentityResultType = {
  /** The errors that occurred during the identity operation. */
  errors?: Maybe<Array<Maybe<IdentityErrorType>>>;
  succeeded: Scalars['Boolean']['output'];
};

export type InputCreatePasskeyType = {
  credential: Scalars['String']['input'];
  name?: InputMaybe<Scalars['String']['input']>;
};

export type Mutations = {
  createPasskey?: Maybe<IdentityResultType>;
};


export type MutationsCreatePasskeyArgs = {
  command: InputCreatePasskeyType;
};

export type Query = {
  passkeyCreationOptions?: Maybe<Scalars['String']['output']>;
  passkeyRequestOptions?: Maybe<Scalars['String']['output']>;
};

export type CreatePasskeyMutationVariables = Exact<{
  command: InputCreatePasskeyType;
}>;


export type CreatePasskeyMutation = { createPasskey?: { succeeded: boolean, errors?: Array<{ code?: string, description?: string }> } };

export type GetPasskeyCreationOptionsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetPasskeyCreationOptionsQuery = { passkeyCreationOptions?: string };

export type GetPasskeyRequestOptionsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetPasskeyRequestOptionsQuery = { passkeyRequestOptions?: string };


export const CreatePasskeyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreatePasskey"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"command"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"InputCreatePasskeyType"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createPasskey"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"command"},"value":{"kind":"Variable","name":{"kind":"Name","value":"command"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"succeeded"}},{"kind":"Field","name":{"kind":"Name","value":"errors"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"code"}},{"kind":"Field","name":{"kind":"Name","value":"description"}}]}}]}}]}}]} as unknown as DocumentNode<CreatePasskeyMutation, CreatePasskeyMutationVariables>;
export const GetPasskeyCreationOptionsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPasskeyCreationOptions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"passkeyCreationOptions"}}]}}]} as unknown as DocumentNode<GetPasskeyCreationOptionsQuery, GetPasskeyCreationOptionsQueryVariables>;
export const GetPasskeyRequestOptionsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetPasskeyRequestOptions"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"passkeyRequestOptions"}}]}}]} as unknown as DocumentNode<GetPasskeyRequestOptionsQuery, GetPasskeyRequestOptionsQueryVariables>;
export const OperationNames = {
  Query: {
    GetPasskeyCreationOptions: 'GetPasskeyCreationOptions',
    GetPasskeyRequestOptions: 'GetPasskeyRequestOptions'
  },
  Mutation: {
    CreatePasskey: 'CreatePasskey'
  }
}
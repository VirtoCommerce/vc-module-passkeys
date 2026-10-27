# Virto Commerce Passkeys Module

## Overview

The Passkeys module lets users sign in with passkeys (WebAuthn) instead of a password, in both the Commerce Manager and the storefront. A user adds a passkey from their profile; the browser or the device (Windows Hello, Touch ID, Android, a security key, a password manager) keeps the private key, and the platform keeps only the public key. Users can rename and delete their passkeys from the list of their sign-in keys.

The module has no database of its own: passkeys are stored in the platform's Identity store (`AspNetUserPasskeys`).

## Key Features

* **Passwordless sign-in** — sign in with a passkey in the Commerce Manager and in the storefront
* **Passkey management** — create passkeys from the user profile; rename and delete them through the platform's sign-in method API
* **`passkey` grant type** — exchanges a WebAuthn assertion for a token on the token endpoint and on the platform's cookie sign-in endpoint; lockout and the platform's sign-in rules apply
* **Readable key names** — a passkey without a name shows the authenticator's name, derived from its AAGUID
* **Backup state** — each passkey shows whether it is `DeviceOnly`, `BackedUp` or `NotBackedUp`
* **Storefront plugin** — adds the passkey sign-in button and passkey creation to the storefront through the XFrontend plugin system
* **Browser detection** — the passkey button and passkey creation appear only when the browser supports WebAuthn
* **Impersonation-safe** — passkeys cannot be created in an impersonated session

## Configuration

### Application Settings

The module is configured in `appsettings.json`:

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Passkey:Enabled` | Boolean | `true` | Turns passkey sign-in and passkey creation on or off |

```json
{
  "Passkey": {
    "Enabled": true
  }
}
```

The module adds no settings to the settings UI.

### Permissions

The module adds no permissions. Getting the sign-in options is anonymous; creating a passkey requires an authenticated user.

## Architecture

The module follows the Virto Commerce platform conventions:

```
┌─────────────────────────────────────────────────────────────┐
│  Web Layer (API Controller, Admin UI, Storefront plugin)    │
├─────────────────────────────────────────────────────────────┤
│  XAPI Layer (GraphQL queries and mutation)                  │
├─────────────────────────────────────────────────────────────┤
│  Data Layer (Passkey service, Grant type, Sign-in method)   │
├─────────────────────────────────────────────────────────────┤
│  Core Layer (Models, Service Interfaces, Constants)         │
├─────────────────────────────────────────────────────────────┤
│  Platform Identity store (AspNetUserPasskeys)               │
└─────────────────────────────────────────────────────────────┘
```

### Sign-In Flow

1. The client requests the WebAuthn **request options** (`POST api/passkeys/request-options` or the `passkeyRequestOptions` query)
2. The browser asks the user to pick a passkey via `navigator.credentials.get`
3. The client sends the result to the token endpoint with `grant_type=passkey` and `credential=<JSON>`
4. `PasskeyGrantTypeHandler` validates the assertion through `IPasskeyService.AuthenticateAsync` and updates the passkey's sign count
5. The platform issues the token; two-factor authentication is skipped, because a passkey is already a strong factor

### Passkey Creation Flow

1. A signed-in user requests the WebAuthn **creation options** (`POST api/passkeys/creation-options` or the `passkeyCreationOptions` query)
2. The browser creates a passkey via `navigator.credentials.create`
3. The client sends the result to `POST api/passkeys` or the `createPasskey` mutation
4. `PasskeyService` verifies the attestation, checks that it belongs to the current user, and stores the passkey in the Identity store

## Components

### Projects

| Project | Layer | Purpose |
|---------|-------|---------|
| `VirtoCommerce.Passkeys.Core` | Core | Models, service interfaces, module constants |
| `VirtoCommerce.Passkeys.Data` | Data | Passkey service, `passkey` grant type, sign-in method provider, known authenticator names |
| `VirtoCommerce.Passkeys.Xapi` | XAPI | GraphQL queries and mutation |
| `VirtoCommerce.Passkeys.Web` | Web | REST API controller, Admin UI, storefront plugin (`StorefrontApp`), module bootstrapping and DI registration |
| `VirtoCommerce.Passkeys.Tests` | Tests | Unit tests |

### Key Services

| Service | Interface | Responsibility |
|---------|-----------|----------------|
| `PasskeyService` | `IPasskeyService` | Builds WebAuthn options, verifies assertions and attestations, stores passkeys |
| `PasskeyGrantTypeHandler` | `GrantTypeHandlerBase` | Handles the `passkey` grant type on the token endpoint |
| `PasskeySignInMethodProvider` | `ISignInMethodProvider` | Lists a user's passkeys as sign-in keys; renames and deletes them |

### REST API

Base route: `api/passkeys`

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/request-options` | Get WebAuthn options for signing in with a passkey (anonymous) |
| `POST` | `/creation-options` | Get WebAuthn options for creating a passkey for the current user |
| `POST` | `/` | Create a passkey for the current user from `{ credential, name }` |

Renaming and deleting passkeys go through the platform's sign-in method API.

### GraphQL API

| Type | Name | Description |
|------|------|-------------|
| Query | `passkeyRequestOptions` | WebAuthn request options (JSON) for `navigator.credentials.get` (anonymous) |
| Query | `passkeyCreationOptions` | WebAuthn creation options (JSON) for `navigator.credentials.create` (authenticated user) |
| Mutation | `createPasskey` | Creates a passkey for the current user from `credential` and an optional `name` |

## Documentation

* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.Passkeys)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-passkeys/)

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-passkeys/releases/latest)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

This software is licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at http://virtocommerce.com/opensourcelicense.

Unless required by the applicable law or agreed to in written form, the software
distributed under the License is provided on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.

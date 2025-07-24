---
description: How to work with jwt tokens
icon: user
---

# Authentication

### <mark style="color:$info;">Components</mark>

* **AuthController**: Handles registration and login.
  * **Route**: `/api/auth`
* **RefreshController**: Manages token refresh.
  * **Route**: `/api/auth/token`

### <mark style="color:$info;">Authentication Process</mark>

* **Registration**:
  * **Endpoint**: `POST /api/auth/register`
  * **Input**: `RegistrationRequest` (name, password, inviteLink, deviceInfo)
  * **Output**: Access token
  * **Action**: Validates invite, registers user, opens session.
* **Login**:
  * **Endpoint**: `POST /api/auth/login`
  * **Input**: `LoginRequest` (name, password, deviceInfo)
  * **Output**: Access token
  * **Action**: Authenticates user, opens session.
* **Token Refresh**:
  * **Endpoint**: `POST /api/auth/token/refresh`
  * **Input**: `RefreshTokenRequest` (refreshToken)
  * **Output**: `RefreshTokenResponse` (accessToken, refreshToken)
  * **Action**: Refreshes expired access token.

### <mark style="color:$info;">Token Usage</mark>

* **Access Token**:
  * **Request**: Obtain during registration or login.
  * **Refresh**: Request via `/api/auth/token/refresh` when expired (e.g., 02:12 PM CEST, July 21, 2025).
* **Refresh Token**:
  * **Request**: Received during registration or login.
  * **Refresh**: Use to obtain new access token when current one expires.

### Security

* **Protocol**: Requires HTTPS.
* **Storage**: Store refresh token in HTTP-only cookie with `Secure` and `SameSite=Strict`.

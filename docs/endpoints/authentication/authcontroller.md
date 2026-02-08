---
description: >-
  Controller for handling user authentication operations, including registration
  and login, with session management and invite-based registration.
---

# AuthController

### Controller Description

* **Route**: `api/auth`
* **Authorize**: Allows anonymous access (`[AllowAnonymous]`).

### Endpoints

#### <mark style="color:$info;">Register</mark>

* **Description**: Registers a new user using a valid invite link and opens a user session, returning an authentication token.
* **Route**: `[POST] api/auth/register`
* **HTTP Method**: `POST`
* **Request**:
  * **Content-Type**: `application/json`
  *   **Request Body**: `RegistrationRequest` object with the following structure:

      ```json
      {
        "name": "string",
        "password": "string",
        "inviteLink": "string",
        "deviceInfo": "string"
      }
      ```
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns the authentication token upon successful registration and session creation.

      ```json
      {
        "refreshToken": "string",
        "accessToken": "string"
      }
      ```
  * <mark style="color:$danger;">**400 Bad Request**</mark>:
    * If model validation fails.
    * If the user already exists: `"Registration failed: user already exists."`
    * If the invite link is invalid: `"Invite link invalid."`
    * If the username is invalid: `"Invalid username: <error_message>"`
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "An unexpected error occurred. Please try again later."
      ```

#### <mark style="color:$info;">Login</mark>

* **Description**: Authenticates an existing user and opens a session, returning an authentication token.
* **Route**: `[POST] api/auth/login`
* **HTTP Method**: `POST`
* **Request**:
  * **Content-Type**: `application/json`
  *   **Request Body**: `LoginRequest` object with the following structure:

      ```json
      {
        "name": "string",
        "password": "string",
        "deviceInfo": "string"
      }
      ```
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns the authentication token upon successful login and session creation.

      ```json
      {
        "refreshToken": "string",
        "accessToken": "string"
      }
      ```
  * <mark style="color:$danger;">**400 Bad Request**</mark>:
    * If model validation fails.
    * If the user does not exist: `"Login failed: user does not exist."`
    * If the username or password is incorrect: `"Login failed: username or password is incorrect."`
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "An unexpected error occurred. Please try again later."
      ```

### Data Models

#### RegistrationRequest

* **Description**: Model for registration request data.
*   **Structure**:

    ```json
    {
      "name": "string",
      "password": "string",
      "inviteLink": "string",
      "deviceInfo": "string"
    }
    ```

#### LoginRequest

* **Description**: Model for login request data.
*   **Structure**:

    ```json
    {
      "name": "string",
      "password": "string",
      "deviceInfo": "string"
    }
    ```

#### RefreshResult

* **Description**: Model for refresh token response data.
*   **Structure**:

    ```json
    {
      "refreshToken": "string",
      "accessToken": "string"
    }
    ```

### Error Handling

* **Invalid User ID**: Not applicable (handled by session service).
* **Invalid Operations**: Registration fails if the user already exists, invite link is invalid, or username is invalid, returning `400 Bad Request`.
* **Unauthorized Access**: Not applicable (endpoint is `[AllowAnonymous]`).
* **Unexpected Errors**: General exceptions are caught, logged, and returned as `500 Internal Server Error`.

### Logging

* Logs successful registration and login events with username and user ID.
* Logs session opening events with username and user ID.
* Logs warnings for specific exceptions (e.g., user already exists, invalid invite link, login failures).
* Logs errors for unexpected exceptions during registration and login.

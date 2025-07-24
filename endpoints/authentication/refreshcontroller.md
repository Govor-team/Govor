---
description: >-
  Controller for managing token refresh operations, allowing users to refresh
  their access tokens using a valid refresh token.
---

# RefreshController

### Controller Description

* **Route**: `api/auth/token`
* **Authorize**: Allows anonymous access (`[AllowAnonymous]`).

### Endpoints

#### <mark style="color:$info;">Refresh</mark>

* **Description**: Refreshes an access token using a provided refresh token and returns a new access token and refresh token pair.
* **Route**: `[POST] api/auth/token/refresh`
* **HTTP Method**: `POST`
* **Request**:
  * **Content-Type**: `application/json`
  *   **Request Body**: `RefreshTokenRequest` object with the following structure:

      ```json
      {
        "refreshToken": "string"
      }
      ```
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a new access token and refresh token upon successful refresh.

      ```json
      {
        "accessToken": "string",
        "refreshToken": "string"
      }
      ```
  *   <mark style="color:$warning;">**400 Bad Request**</mark>: If model validation fails or the refresh token is empty.

      ```json
      "Refresh token cant be empty."
      ```
  *   <mark style="color:$warning;">**401 Unauthorized**</mark>: If the refresh token is invalid or authorization fails.

      ```json
      "Invalid refresh token"
      ```
  *   <mark style="color:$danger;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "An unexpected error occurred."
      ```

### Data Models

#### RefreshTokenRequest

* **Description**: Model for refresh token request data.
*   **Structure**:

    ```json
    {
      "refreshToken": "string"
    }
    ```

#### RefreshTokenResponse

* **Description**: Model for refresh token response data.
*   **Structure**:

    ```json
    {
      "accessToken": "string",
      "refreshToken": "string"
    }
    ```

### Error Handling

* **Invalid User ID**: Not applicable (handled by session service).
* **Invalid Operations**: Returns `400 Bad Request` if the refresh token is empty or invalid.
* **Unauthorized Access**: Returns `401 Unauthorized` if the refresh token fails authorization.
* **Unexpected Errors**: General exceptions are caught, logged, and returned as `500 Internal Server Error`.

### Logging

* Logs warnings for invalid or failed refresh token attempts.
* Logs errors for unexpected exceptions during token refresh.

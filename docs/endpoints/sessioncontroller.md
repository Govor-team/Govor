---
description: >-
  Controller for managing user sessions, including retrieving and closing
  sessions.
---

# SessionController

### Controller Description

* **Route**: `api/session`
* **Authorize**: Requires authenticated user with roles "Admin" or "User" (`[Authorize(Roles = "Admin,User")]`).

### Endpoints

#### <mark style="color:$info;">GetAllSessions</mark>

* **Description**: Retrieves all active sessions for the authenticated user.
* **Route**: `[GET] api/session/all`
* **HTTP Method**: `GET`
* **Request**: None
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a list of user sessions.

      ```json
      [
        {
          "id": "Guid",
          "deviceInfo": "string",
          "createdAt": "DateTime",
          "expiresAt": "DateTime",
          "isRevoked": "boolean"
        }
      ]
      ```
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "Unexpected Error! Please try again later."
      ```

#### <mark style="color:$info;">CloseSession</mark>

* **Description**: Closes a specific session for the authenticated user.
* **Route**: `[DELETE] api/session/close/{sessionId}`
* **HTTP Method**: `DELETE`
* **Request**:
  * **Path Parameter**: `sessionId` (Guid, required): ID of the session to close.
* **Responses**:
  * <mark style="color:$success;">**200 OK**</mark>: Indicates session was successfully closed.
    * No body.
  *   <mark style="color:$danger;">**400 Bad Request**</mark>: If `sessionId` is invalid or operation fails.

      ```json
      "string"
      ```
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$danger;">**404 Not Found**</mark>: If session is not found.

      ```json
      "string"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "Unexpected Error! Please try again later."
      ```

#### <mark style="color:$info;">CloseAllSessions</mark>

* **Description**: Closes all active sessions for the authenticated user.
* **Route**: `[DELETE] api/session/close/all`
* **HTTP Method**: `DELETE`
* **Request**: None
* **Responses**:
  * <mark style="color:$success;">**200 OK**</mark>: Indicates all sessions were successfully closed.
    * No body.
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$danger;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "Unexpected Error! Please try again later."
      ```

### Data Models

#### SessionDto

* **Description**: Data transfer object for session information.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "deviceInfo": "string",
      "createdAt": "DateTime",
      "expiresAt": "DateTime",
      "isRevoked": "boolean"
    }
    ```

### Error Handling

* **Invalid User ID**: Handled by `ICurrentUserService`, aborts if invalid.
* **Invalid Operations**: Returns `400 Bad Request` for `InvalidOperationException`.
* **Unauthorized Access**: Returns `403 Forbidden` for `UnauthorizedAccessException`.
* **Resource Not Found**: Returns `404 Not Found` for `NotFoundException`.
* **Unexpected Errors**: Caught and returned as `500 Internal Server Error`.

### Logging

* Logs warnings for `UnauthorizedAccessException` and `NotFoundException`.
* Logs errors for `InvalidOperationException` and unexpected exceptions.

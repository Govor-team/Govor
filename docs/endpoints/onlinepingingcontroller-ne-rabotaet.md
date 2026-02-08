---
description: >-
  Description: Controller for handling user online status updates by processing
  ping requests.
---

# OnlinePingingController(не работает)

**Route**: `api/online`\
**Authorize**: Requires "User" or "Admin" role

### <mark style="color:purple;">End Point {PATCH}</mark>

`[PATCH] api/online/ping`

#### Description

Updates the online status of the current user by sending a ping request.

**Request**

* **Content-Type**: `application/json`
* **Request Body**: None

**Responses**

*   **200 OK**: Ping processed successfully.

    ```json
    {}
    ```
*   **400 Bad Request**: If the user cannot be found in the database.

    ```json
    "User can't be found in our database."
    ```
*   **403 Forbidden**: If the user is not authorized to perform the action.

    ```json
    {
      "error": "Not authorized to perform this action."
    }
    ```
*   **500 Internal Server Error**: Indicates an unexpected error.

    ```json
    {
      "error": "Failed to send friend request."
    }
    ```

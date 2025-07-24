---
description: >-
  Description: Controller for managing friendship-related operations, including
  searching for users and retrieving the list of friends for the current user.
---

# FriendshipController

### Controller Description

* **Route**: `api/friends`
* **Authorize**: Requires authenticated user (`[Authorize]`).

### Endpoints

#### <mark style="color:$info;">Search</mark>

* **Description**: Searches for users based on a query string.
* **Route**: `[GET] api/friends/search?query=`
* **HTTP Method**: `GET`
* **Request**:
  * **Query Parameter**: `query` (string, required)
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a list of matching users.

      ```json
      [
        {
          "id": "Guid",
          "username": "string",
          "description": "string",
          "wasOnline": "DateTime",
          "iconId": "Guid"
        }
      ]
      ```
  *   <mark style="color:$danger;">**400 Bad Request**</mark>: If the query is empty.

      ```json
      "Query cannot be empty"
      ```
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      {
        "error": "Internal error during user search."
      }
      ```

#### <mark style="color:$info;">GetFriends</mark>

* **Description**: Retrieves the list of friends for the authenticated user.
* **Route**: `[GET] api/friends`
* **HTTP Method**: `GET`
* **Request**: None
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a list of the user's friends or an empty list if none.

      ```json
      [
        {
          "id": "Guid",
          "username": "string",
          "description": "string",
          "wasOnline": "DateTime",
          "iconId": "Guid"
        }
      ]
      ```
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      {
        "error": "Internal server error."
      }
      ```

### Data Models

#### UserDto

* **Description**: Data transfer object for user information.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "username": "string",
      "description": "string",
      "wasOnline": "DateTime",
      "iconId": "Guid"
    }
    ```

### Error Handling

* **Invalid User ID**: Handled by `ICurrentUserService`, aborts if invalid.
* **Invalid Operations**: Returns `400 Bad Request` for empty query.
* **Unexpected Errors**: Caught and returned as `500 Internal Server Error`.

### Logging

* Logs warnings for `SearchUsersException`.
* Logs errors for unexpected exceptions and `InvalidOperationException`.

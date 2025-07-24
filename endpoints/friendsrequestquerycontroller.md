---
description: >-
  Controller for querying friend request-related data, including incoming friend
  requests and responses to sent friend requests.
---

# FriendsRequestQueryController

### Controller Description

* **Route**: `api/friends`
* **Authorize**: Requires authenticated user (`[Authorize]`).

### Endpoints

#### <mark style="color:$info;">GetIncomingRequests</mark>

* **Description**: Retrieves a list of incoming friend requests for the authenticated user.
* **Route**: `[GET] api/friends/requests`
* **HTTP Method**: `GET`
* **Request**: None
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a list of incoming friend requests or an empty list if none.

      ```json
      [
        {
          "id": "Guid",
          "senderId": "Guid",
          "receiverId": "Guid",
          "status": "string",
          "createdAt": "DateTime"
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

#### <mark style="color:$info;">GetResponses</mark>

* **Description**: Retrieves a list of responses to the authenticated user's sent friend requests.
* **Route**: `[GET] api/friends/responses`
* **HTTP Method**: `GET`
* **Request**: None
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a list of friend request responses or an empty list if none.

      ```json
      [
        {
          "id": "Guid",
          "senderId": "Guid",
          "receiverId": "Guid",
          "status": "string",
          "createdAt": "DateTime"
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

#### FriendshipDto

* **Description**: Data transfer object for friend request information.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "senderId": "Guid",
      "receiverId": "Guid",
      "status": "string",
      "createdAt": "DateTime"
    }
    ```

### Error Handling

* **Invalid User ID**: Handled by `ICurrentUserService`, aborts if invalid.
* **Invalid Operations**: Returns `200 OK` with an empty list for `InvalidOperationException`.
* **Unexpected Errors**: Caught and returned as `500 Internal Server Error`.

### Logging

* Logs warnings for `InvalidOperationException`.
* Logs information for successful response retrieval.
* Logs errors for unexpected exceptions.

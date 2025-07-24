---
description: >-
  Description: SignalR hub for managing real-time friend request operations,
  including sending, accepting, and rejecting friend requests.
icon: wifi
---

# FriendsHub

**Route** `hubs/friends`

**Authorize**: Requires authentication (user must be logged in)

### <mark style="color:$info;">Hub Methods</mark>

#### <mark style="color:$primary;">OnConnectedAsync</mark>

**Description**: Automatically invoked when a client establishes a connection to the hub. Adds the user to their personal group for receiving friend request notifications.

**Behavior**:

* Retrieves the user's ID from the connection context.
* If the user ID is invalid (`Guid.Empty`), logs a warning and aborts the connection.
* Adds the user to a group named after their user ID (e.g., `userId.ToString()`).
* Logs the connection event with the user ID and connection ID.

***

#### <mark style="color:$primary;">OnDisconnectedAsync</mark>

**Description**: Automatically invoked when a client disconnects from the hub. Removes the user from their personal group.

**Behavior**:

* Retrieves the user's ID from the connection context, suppressing exceptions if the ID is unavailable (e.g., due to an early abort).
* If the user ID is valid, removes the user from their personal group.
* Logs the disconnection event with the user ID and connection ID.
* If an exception occurs, logs it as a warning along with the connection ID.
* If no exception occurs but the user ID is invalid, logs the disconnection with the connection ID.

***

#### <mark style="color:$info;">SendRequest</mark>

**Description**: Sends a friend request to a specified user and notifies the recipient in real-time.

**Request**:

* **Method Name**: `SendRequest`
* **Parameters**:
  * `targetUserId`: (`Guid`) The ID of the user to whom the friend request is sent.

**Responses**:

*   <mark style="color:$success;">**Success (201 Created)**</mark>: Friend request sent successfully.

    ```json
    {
      "status": 201,
      "result": null,
      "errorMessage": null
    }
    ```
*   <mark style="color:$danger;">**400 Bad Request**</mark>: If the operation is invalid (e.g., sending a request to oneself).

    ```json
    {
      "status": 400,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$danger;">**401 Unauthorized**</mark>: If the user is not authorized.

    ```json
    {
      "status": 401,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$danger;">**409 Conflict**</mark>: If a friend request was already sent.

    ```json
    {
      "status": 409,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$warning;">**500 Server Error**</mark>: Indicates an unexpected error.

    ```json
    {
      "status": 500,
      "result": null,
      "errorMessage": "Unexpected error! Please try later!"
    }
    ```

**Client-Side Events**:

* **FriendRequestReceived**: Triggered on the recipient's client to notify them of a new friend request.
  * Payload: `userId` (`Guid`) - The ID of the user who sent the request.

***

#### <mark style="color:purple;">AcceptRequest</mark>

**Description**: Accepts a friend request and notifies the user in real-time.

**Request**:

* **Method Name**: `AcceptRequest`
* **Parameters**:
  * `friendshipId`: (`Guid`) The ID of the friend request to accept.

**Responses**:

*   <mark style="color:$success;">**Success (200 OK)**</mark>: Friend request accepted successfully.

    ```json
    {
      "status": 200,
      "result": null,
      "errorMessage": null
    }
    ```
*   <mark style="color:$danger;">**400 Bad Request**</mark>: If the operation is invalid (e.g., invalid friendship ID).

    ```json
    {
      "status": 400,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$danger;">**401 Unauthorized**</mark>: If the user is not authorized to accept the request.

    ```json
    {
      "status": 401,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$warning;">**500 Server Error**</mark>: Indicates an unexpected error.

    ```json
    {
      "status": 500,
      "result": null,
      "errorMessage": "Unexpected error! Please try later!"
    }
    ```

**Client-Side Events**:

* **FriendRequestAccepted**: Triggered on the user's client to confirm the friend request was accepted.
  * Payload: `friendshipId` (`Guid`) - The ID of the accepted friend request.

***

#### <mark style="color:purple;">RejectRequest</mark>

**Description**: Rejects a friend request and notifies the user in real-time.

**Request**:

* **Method Name**: `RejectRequest`
* **Parameters**:
  * `friendshipId`: (`Guid`) The ID of the friend request to reject.

**Responses**:

*   <mark style="color:$success;">**Success (200 OK)**</mark>: Friend request rejected successfully.

    ```json
    {
      "status": 200,
      "result": null,
      "errorMessage": null
    }
    ```
*   <mark style="color:$danger;">**400 Bad Request**</mark>: If the operation is invalid (e.g., invalid friendship ID).

    ```json
    {
      "status": 400,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$danger;">**401 Unauthorized**</mark>: If the user is not authorized to reject the request.

    ```json
    {
      "status": 401,
      "result": null,
      "errorMessage": "<error_message>"
    }
    ```
*   <mark style="color:$warning;">**500 Server Error**</mark>: Indicates an unexpected error.

    ```json
    {
      "status": 500,
      "result": null,
      "errorMessage": "Unexpected error! Please try later!"
    }
    ```

**Client-Side Events**:

* **FriendRequestRejected**: Triggered on the user's client to confirm the friend request was rejected.
  * Payload: `friendshipId` (`Guid`) - The ID of the rejected friend request.

***

### <mark style="color:$info;">Error Handling</mark>

* **Invalid User ID**: If the user ID is `Guid.Empty` during connection, the connection is aborted with a logged warning.
* **Invalid Operations**: Operations like sending a request to oneself or accepting/rejecting an invalid request return a `BadRequest` result.
* **Unauthorized Access**: Unauthorized actions return an `Unauthorized` result with a logged warning.
* **Conflicts**: Attempting to send a duplicate friend request returns a `Conflict` result.
* **Unexpected Errors**: General exceptions are caught, logged, and returned as a `ServerError` result.

***

### Logging

* Logs connection and disconnection events with user ID and connection ID.
* Logs friend request operations (send, accept, reject) with success or failure details for monitoring and debugging.

***

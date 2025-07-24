---
description: SignalR hub for managing user online presence and notifications.
icon: wifi
---

# PresenceHub

### Controller Description

* **Route**: `hubs/presence`
* **Authorize**: Requires authenticated user with roles "Admin" or "User" (`[Authorize(Roles = "Admin, User")]`).

### Hub Methods

#### <mark style="color:$info;">OnConnectedAsync</mark>

* **Description**: Invoked when a client connects, updates online status and notifies friends.
* **Behavior**:
  * Retrieves user ID from context.
  * Aborts connection if user ID is `Guid.Empty` or user does not exist.
  * Adds user to online store and their personal group.
  * Notifies friends of user going online via `UserOnline` event.
* **Client-Side Events**:
  * **UserOnline**: Triggered for friends with payload `userId` (Guid).
* **Responses**: None (connection aborted on invalid user ID).

#### <mark style="color:$info;">OnDisconnectedAsync</mark>

* **Description**: Invoked when a client disconnects, updates offline status and notifies friends.
* **Behavior**:
  * Retrieves user ID from context, suppressing exceptions.
  * Removes user from personal group if ID is valid.
  * Updates user's `WasOnline` timestamp.
  * Sets user as offline in store.
  * Notifies friends of user going offline via `UserOffline` event.
* **Client-Side Events**:
  * **UserOffline**: Triggered for friends with payload `userId` (Guid).
* **Responses**: None.

### Data Models

* **N/A**: No specific data models defined for hub methods.

### Error Handling

* **Invalid User ID**: Aborts connection with logged warning if `Guid.Empty` or user not found.
* **Unexpected Errors**: Logged, but no specific response (handled by base method).

### Logging

* Logs warnings for invalid user ID on connection.
* Logs information for disconnections with user ID and connection ID.
* Logs errors for unexpected exceptions during disconnection.

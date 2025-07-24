---
icon: wifi
---

# ChatHub

**Route**: `hubs/chats`\
**Authorize**: Requires authentication (user must be logged in)

***

### Hub Methods

#### <mark style="color:$primary;">OnConnectedAsync</mark>

**Description**: Automatically invoked when a client establishes a connection to the hub. Adds the user to their personal group for private messages and notifications.

**Behavior**:

* Retrieves the user's ID from the connection context.
* If the user ID is invalid (`Guid.Empty`), logs a warning and aborts the connection.
* Adds the user to a group named after their user ID (e.g., `userId.ToString()`).
* Logs the connection event with the user ID and connection ID.

**Notes**:

* Future enhancements may include adding the user to their chat groups (currently a TODO in the code).

***

#### <mark style="color:$primary;">OnDisconnectedAsync</mark>

**Description**: Automatically invoked when a client disconnects from the hub. Removes the user from their personal group.

**Behavior**:

* Retrieves the user's ID from the connection context, suppressing exceptions if the ID is unavailable (e.g., due to an early abort).
* If the user ID is valid, removes the user from their personal group.
* Logs the disconnection event with the user ID and connection ID.
* If an exception occurs, logs it as a warning along with the connection ID.
* If no exception occurs but the user ID is invalid, logs the disconnection with the connection ID.

**Notes**:

* Future enhancements may include removing the user from their chat groups (currently a TODO in the code).

***

#### <mark style="color:$info;">Send</mark>

**Description**: Allows a client to send a message to a recipient, which can be either a user or a group.

**Request**:

* **Method Name**: `Send`
* **Parameters**:
  *   `request`: An object of type `MessageRequest` with the following structure:

      ```json
      {
        "encryptedContent": "string",
        "replyToMessageId": "Guid",
        "recipientId": "Guid",
        "recipientType": "string", // "User" or "Group"
        "mediaAttachments": [
          {
            "mediaId": "Guid",
            "encryptedKey": "string",
            "type": "string",
            "mimeType": "string"
          }
        ]
      }
      ```

**Responses**:

* <mark style="color:$success;">**Success**</mark>:
  * Sends the message to the recipient (user or group) via the `ReceiveMessage` event.
  * Notifies the sender with a confirmation via the `MessageSent` event.
  * Logs the successful message send with the message ID, sender ID, recipient ID, and recipient type.
* <mark style="color:$danger;">**Error**</mark>:
  * If the message is empty (no `encryptedContent` and no `mediaAttachments`), logs a warning and throws an `ArgumentException`.
  * If the message fails to send (e.g., due to an internal error), logs the error and throws a `HubException`.

**Client-Side Events**:

* <mark style="color:$primary;">**ReceiveMessage**</mark>: Triggered on the recipient's client(s) to deliver the message.
  * Payload: `UserMessageResponse` object.
* <mark style="color:$primary;">**MessageSent**</mark>: Triggered on the sender's client to confirm the message was sent.
  * Payload: `UserMessageResponse` object.

**Notes**:

* The message must contain either `encryptedContent` or `mediaAttachments`; otherwise, it is invalid.
* The recipient is determined by `recipientType`:
  * `"User"`: Sends to the recipient’s personal group (e.g., `recipientId.ToString()`).
  * `"Group"`: Sends to all members of the group (e.g., `group_{recipientId}`).

***

### Data Models

#### MessageRequest

```json
{
  "encryptedContent": "string",
  "replyToMessageId": "Guid",
  "recipientId": "Guid",
  "recipientType": "string", // "User" or "Group"
  "mediaAttachments": [
    {
      "mediaId": "Guid",
      "encryptedKey": "string",
      "type": "string",
      "mimeType": "string"
    }
  ]
}
```

#### UserMessageResponse

```json
{
  "messageId": "Guid",
  "senderId": "Guid",
  "recipientId": "Guid",
  "recipientType": "string",
  "encryptedContent": "string",
  "sentAt": "DateTime",
  "isEdited": "bool",
  "mediaAttachments": [
    {
      "mediaId": "Guid",
      "encryptedKey": "string",
      "type": "string",
      "mimeType": "string"
    }
  ],
  "replyToMessageId": "Guid"
}
```

***

### Error Handling

* **Invalid User ID**: If the user ID is `Guid.Empty` during connection, the connection is aborted with a logged warning.
* **Empty Message**: If a message lacks both content and attachments, an `ArgumentException` is thrown with a logged warning.
* **Message Send Failure**: If sending fails (e.g., due to a service error), a `HubException` is thrown with the error logged.

***

### Logging

* Logs connection events with user ID and connection ID.
* Logs disconnection events, including exceptions if present.
* Logs message send attempts, successes, and failures for monitoring and debugging.

***

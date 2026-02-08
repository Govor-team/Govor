---
description: Controller for loading chat messages for users and groups with pagination.
---

# ChatLoadController

### Controller Description

* **Route**: `api/chats`
* **Authorize**: Requires authenticated user with roles "Admin" or "User" (`[Authorize(Roles = "Admin, User")]`).

### Endpoints

#### <mark style="color:$info;">GetGroupMessages</mark>

* **Description**: Retrieves messages from a specified group chat with pagination.
* **Route**: `[GET] api/chats/group-messages`
* **HTTP Method**: `GET`
* **Request**:
  * **Path Parameter**: `chatId` (Guid, required): ID of the group chat.
  * **Query Parameter**: `query` (`MessageQuery`, required) with:
    * `startMessageId` (Guid?, optional): ID of the starting message.
    * `before` (int, default=20): Number of messages before the start.
    * `after` (int, default=2): Number of messages after the start.
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns a list of group chat messages.

      ```json
      [
        {
          "id": "Guid",
          "senderId": "Guid",
          "recipientId": "Guid",
          "recipientType": "string",
          "encryptedContent": "string",
          "sentAt": "DateTime",
          "isEdited": "boolean",
          "editedAt": "DateTime?",
          "replyToMessageId": "Guid?",
          "mediaAttachments": [
            {
              "id": "Guid",
              "messageId": "Guid",
              "mediaFileId": "Guid"
            }
          ],
          "reactions": [
            {
              "id": "Guid",
              "messageId": "Guid",
              "userId": "Guid",
              "reactionCode": "string",
              "reactedAt": "DateTime"
            }
          ],
          "messageViews": [
            {
              "id": "Guid",
              "messageId": "Guid",
              "userId": "Guid",
              "viewedAt": "DateTime"
            }
          ]
        }
      ]
      ```
  * <mark style="color:$danger;">**400 Bad Request**</mark>:
    * If `before` or `after` is negative or total exceeds 100: `"Values must be non-negative and total must not exceed 100."`
    * If an argument or resource is invalid: `"string"`
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "Unexpected Error! Please try again later."
      ```

#### <mark style="color:$info;">GetUserMessages</mark>

* **Description**: Retrieves messages from a specified user chat with pagination.
* **Route**: `[GET] api/chats/user-messages`
* **HTTP Method**: `GET`
* **Request**:
  * **Path Parameter**: `userId` (Guid, required): ID of the target user.
  * **Query Parameter**: `query` (`MessageQuery`, required) with:
    * `startMessageId` (Guid?, optional): ID of the starting message.
    * `before` (int, default=20): Number of messages before the start.
    * `after` (int, default=2): Number of messages after the start.
* **Responses**:
  *   **200 OK**: Returns a list of user chat messages.

      ```json
      [
        {
          "id": "Guid",
          "senderId": "Guid",
          "recipientId": "Guid",
          "recipientType": "string",
          "encryptedContent": "string",
          "sentAt": "DateTime",
          "isEdited": "boolean",
          "editedAt": "DateTime?",
          "replyToMessageId": "Guid?",
          "mediaAttachments": [
            {
              "id": "Guid",
              "messageId": "Guid",
              "mediaFileId": "Guid"
            }
          ],
          "reactions": [
            {
              "id": "Guid",
              "messageId": "Guid",
              "userId": "Guid",
              "reactionCode": "string",
              "reactedAt": "DateTime"
            }
          ],
          "messageViews": [
            {
              "id": "Guid",
              "messageId": "Guid",
              "userId": "Guid",
              "viewedAt": "DateTime"
            }
          ]
        }
      ]
      ```
  * **400 Bad Request**:
    * If `before` or `after` is negative or total exceeds 100: `"Values must be non-negative and total must not exceed 100."`
    * If an operation or resource is invalid: `"string"`
  *   **403 Forbidden**: If user lacks authorization.

      ```json
      "string"
      ```
  *   **500 Internal Server Error**: Indicates an unexpected error.

      ```json
      "Unexpected Error! Please try again later."
      ```

### Data Models

#### MessageQuery

* **Description**: Model for pagination query parameters.
*   **Structure**:

    ```json
    {
      "startMessageId": "Guid?",
      "before": "int",
      "after": "int"
    }
    ```

#### MessageResponse

* **Description**: Data transfer object for chat message details.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "senderId": "Guid",
      "recipientId": "Guid",
      "recipientType": "string",
      "encryptedContent": "string",
      "sentAt": "DateTime",
      "isEdited": "boolean",
      "editedAt": "DateTime?",
      "replyToMessageId": "Guid?",
      "mediaAttachments": [
        {
          "id": "Guid",
          "messageId": "Guid",
          "mediaFileId": "Guid"
        }
      ],
      "reactions": [
        {
          "id": "Guid",
          "messageId": "Guid",
          "userId": "Guid",
          "reactionCode": "string",
          "reactedAt": "DateTime"
        }
      ],
      "messageViews": [
        {
          "id": "Guid",
          "messageId": "Guid",
          "userId": "Guid",
          "viewedAt": "DateTime"
        }
      ]
    }
    ```

#### MediaAttachmentResponse

* **Description**: Data transfer object for media attachments in messages.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "messageId": "Guid",
      "mediaFileId": "Guid"
    }
    ```

#### MessageReactionResponse

* **Description**: Data transfer object for message reactions.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "messageId": "Guid",
      "userId": "Guid",
      "reactionCode": "string",
      "reactedAt": "DateTime"
    }
    ```

#### MessageViewResponse

* **Description**: Data transfer object for message view tracking.
*   **Structure**:

    ```json
    {
      "id": "Guid",
      "messageId": "Guid",
      "userId": "Guid",
      "viewedAt": "DateTime"
    }
    ```

### Error Handling

* **Invalid User ID**: Handled by `ICurrentUserService`, aborts if invalid.
* **Invalid Operations**: Returns `400 Bad Request` for negative values, total exceeding 100, or `InvalidOperationException`.
* **Unauthorized Access**: Returns `403 Forbidden` for `UnauthorizedAccessException`.
* **Resource Not Found**: Returns `400 Bad Request` for `NotFoundException`.
* **Unexpected Errors**: Caught and returned as `500 Internal Server Error`.

### Logging

* Logs warnings for `UnauthorizedAccessException`, `NotFoundException`, `ArgumentException`, and `InvalidOperationException`.
* Logs errors for unexpected exceptions.

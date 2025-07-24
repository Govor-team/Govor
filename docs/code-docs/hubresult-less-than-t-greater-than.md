---
description: >-
  Description: A generic class used to encapsulate the result of SignalR hub
  operations, providing a standardized way to return status, data, and error
  messages. It supports various HTTP-like status cod
---

# HubResult\<T>

**Namespace**: `Govor.Contracts.Responses.SignalR`

### <mark style="color:$info;">Properties</mark>

* **Status**: (`HubResultStatus`) Indicates the status of the hub operation, corresponding to HTTP-like status codes (e.g., Success, BadRequest).
* **Result**: (`T?`) The data returned by the hub operation, if applicable. Can be null for error cases or when no data is returned.
* **ErrorMessage**: (`string?`) A message describing the error, if the operation failed. Null for successful operations.

### <mark style="color:$info;">Static Methods</mark>

#### Ok

**Description**: Creates a successful result with an optional data payload.\
**Parameters**:

* `result`: (`T?`, optional) The data to include in the response (default: `null`).\
  **Returns**: A `HubResult<T>` with `Status` set to `Success` (200) and the provided `result`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.Ok("Operation completed");
```

#### Created

**Description**: Creates a result indicating a resource was created, with an optional data payload.\
**Parameters**:

* `result`: (`T?`, optional) The data to include in the response (default: `null`).\
  **Returns**: A `HubResult<T>` with `Status` set to `Created` (201) and the provided `result`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.Created("Resource created");
```

#### NoContent

**Description**: Creates a result indicating the operation was successful but no data is returned.\
**Parameters**: None\
**Returns**: A `HubResult<T>` with `Status` set to `NoContent` (204) and `Result` set to `null`.\
**Example**:

```csharp
HubResult<string> result = HubResult<string>.NoContent();
```

#### BadRequest

**Description**: Creates a result indicating a client error due to invalid input.\
**Parameters**:

* `message`: (`string`) The error message describing the issue.
* `details`: (`T?`, optional) Additional details about the error (default: `null`).\
  **Returns**: A `HubResult<T>` with `Status` set to `BadRequest` (400), the provided `message`, and optional `details`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.BadRequest("Invalid input provided");
```

#### NotFound

**Description**: Creates a result indicating a requested resource was not found.\
**Parameters**:

* `message`: (`string`) The error message describing the issue.
* `details`: (`T?`, optional) Additional details about the error (default: `null`).\
  **Returns**: A `HubResult<T>` with `Status` set to `NotFound` (404), the provided `message`, and optional `details`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.NotFound("User not found");
```

#### Unauthorized

**Description**: Creates a result indicating the client is not authorized to perform the operation.\
**Parameters**:

* `message`: (`string`) The error message describing the issue.\
  **Returns**: A `HubResult<T>` with `Status` set to `Unauthorized` (401) and the provided `message`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.Unauthorized("Authentication required");
```

#### Conflict

**Description**: Creates a result indicating a conflict, such as a duplicate resource.\
**Parameters**:

* `message`: (`string`) The error message describing the issue.\
  **Returns**: A `HubResult<T>` with `Status` set to `Conflict` (409) and the provided `message`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.Conflict("Resource already exists");
```

#### UnprocessableEntity

**Description**: Creates a result indicating the request could not be processed due to semantic errors.\
**Parameters**:

* `message`: (`string`) The error message describing the issue.\
  **Returns**: A `HubResult<T>` with `Status` set to `UnprocessableEntity` (422) and the provided `message`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.UnprocessableEntity("Invalid data format");
```

#### Error

**Description**: Creates a result indicating an internal server error.\
**Parameters**:

* `message`: (`string`) The error message describing the issue.\
  **Returns**: A `HubResult<T>` with `Status` set to `ServerError` (500) and the provided `message`.\
  **Example**:

```csharp
HubResult<string> result = HubResult<string>.Error("Unexpected server error");
```

### <mark style="color:$info;">Enum: HubResultStatus</mark>

**Description**: Enumerates the possible status codes for a `HubResult<T>` response, aligning with HTTP status codes for consistency in SignalR hub operations.

**Values**:

* <mark style="color:$success;">**Success**</mark> (200): The operation was successful.
* <mark style="color:$success;">**Created**</mark> (201): A resource was successfully created.
* <mark style="color:$success;">**NoContent**</mark> (204): The operation was successful, but no content is returned.
* <mark style="color:$danger;">**BadRequest**</mark> (400): The request was invalid or malformed.
* <mark style="color:$danger;">**Unauthorized**</mark> (401): The client is not authorized to perform the operation.
* <mark style="color:$danger;">**NotFound**</mark> (404): The requested resource was not found.
* <mark style="color:$danger;">**Conflict**</mark> (409): The request conflicts with an existing resource.
* <mark style="color:$danger;">**UnprocessableEntity**</mark> (422): The request could not be processed due to semantic errors.
* <mark style="color:$warning;">**ServerError**</mark> (500): An unexpected server error occurred.

### Example Usage

```csharp
// Successful operation with data
var successResult = HubResult<UserDto>.Ok(new UserDto { Id = Guid.NewGuid(), Username = "example" });

// Error due to invalid input
var errorResult = HubResult<UserDto>.BadRequest("Invalid username provided");

// Not found scenario
var notFoundResult = HubResult<UserDto>.NotFound("User not found in database");
```

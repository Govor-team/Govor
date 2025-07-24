---
description: Controller for uploading and downloading media files.
---

# MediaController

### Controller Description

* **Route**: `api/media`
* **Authorize**: Requires authenticated user with roles "User" or "Admin" (`[Authorize(Roles = "User,Admin")]`).

### Endpoints

#### <mark style="color:$info;">Upload</mark>

* **Description**: Uploads a media file with associated metadata.
* **Route**: `[POST] api/media/upload`
* **HTTP Method**: `POST`
* **Request**:
  * **Content-Type**: `multipart/form-data`
  *   **Request Body**: `MediaUploadRequest` object with the following structure:

      ```json
      {
        "fromFile": "IFormFile",
        "type": "MediaType",
        "mimeType": "string",
        "encryptedKey": "string"
      }
      ```
  * **Constraints**: File size limit of 20 MB.
* **Responses**:
  *   <mark style="color:$success;">**200 OK**</mark>: Returns the upload result.

      ```json
      "string" // Media ID or similar result
      ```
  * <mark style="color:$danger;">**400 Bad Request**</mark>:
    * If model validation fails.
    * If no file is uploaded: `"No file uploaded"`
    * If file exceeds 20 MB: `"File is too large"`
    * If MIME type is missing: `"Missing MIME type"`
    * If operation is invalid: `"string"`
  *   <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks authorization.

      ```json
      "string"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "Internal server error"
      ```

#### <mark style="color:$info;">Download</mark>

* **Description**: Downloads a media file by its ID.
* **Route**: `[GET] api/media/download/{id}`
* **HTTP Method**: `GET`
* **Request**:
  * **Path Parameter**: `id` (Guid, required): ID of the media file.
* **Responses**:
  * <mark style="color:$success;">**200 OK**</mark>: Returns the media file as a stream.
    * **Content-Type**: Matches `MimeType` of the media.
    * **Content-Disposition**: Includes `filename` from `FileName`.
  * <mark style="color:$danger;">**403 Forbidden**</mark>: If user lacks access.
    * No body.
  *   <mark style="color:$danger;">**404 Not Found**</mark>: If media is not found.

      ```json
      "Media not found"
      ```
  *   <mark style="color:$warning;">**500 Internal Server Error**</mark>: Indicates an unexpected error.

      ```json
      "Internal server error"
      ```

### Data Models

#### MediaUploadRequest

* **Description**: Model for media upload request data.
*   **Structure**:

    ```json
    {
      "fromFile": "IFormFile",
      "type": "MediaType",
      "mimeType": "string",
      "encryptedKey": "string"
    }
    ```

### Error Handling

* **Invalid User ID**: Handled by `ICurrentUserService`, aborts if invalid.
* **Invalid Operations**: Returns `400 Bad Request` for `InvalidOperationException`.
* **Unauthorized Access**: Returns `403 Forbidden` for `UnauthorizedAccessException`.
* **Resource Not Found**: Returns `404 Not Found` for `KeyNotFoundException`.
* **Unexpected Errors**: Caught and returned as `500 Internal Server Error`.

### Logging

* Logs warnings for `UnauthorizedAccessException`, `InvalidOperationException`, and `KeyNotFoundException`.
* Logs information for successful file uploads.
* Logs errors for unexpected exceptions during upload and download.

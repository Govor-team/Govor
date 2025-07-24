---
description: >-
  Description: This sub-documentation provides example Java client code for
  interacting with the FriendsHub SignalR hub using the microsoft/signalr
  library. It covers connecting to the hub, handling con
icon: java
---

# FriendsHub Client (Java)

**Prerequisites**:

*   Include the SignalR Java client library in your project. For Gradle, add the following dependency:

    ```gradle
    implementation("com.microsoft.signalr:signalr:9.0.7")
    ```
* Ensure the client has a valid authentication token (JWT) for connecting to the hub, as it requires authentication.

**Hub Route**: `hubs/friends`\
**Authorize**: Requires a valid JWT token with a `userId` claim.

### <mark style="color:purple;">Setup and Connection</mark>

#### Connecting to the Hub

This example demonstrates how to establish a connection to the `FriendsHub` with authentication and handle connection events.

```java
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import io.reactivex.rxjava3.core.Completable;
import io.reactivex.rxjava3.core.Single;

public class FriendsHubClient {
    private final HubConnection hubConnection;
    private final String accessToken; // JWT token with userId claim

    public FriendsHubClient(String hubUrl, String accessToken) {
        this.accessToken = accessToken;
        this.hubConnection = HubConnectionBuilder.create(hubUrl)
                .withAccessTokenProvider(Single.just(accessToken))
                .build();

        // Handle connection established
        hubConnection.onConnected(() -> {
            System.out.println("Connected to FriendsHub with connection ID: " + hubConnection.getConnectionId());
        });

        // Handle connection closed
        hubConnection.onClosed((exception) -> {
            if (exception != null) {
                System.err.println("Connection closed with error: " + exception.getMessage());
            } else {
                System.out.println("Connection closed normally");
            }
        });
    }

    public Completable startConnection() {
        return hubConnection.start();
    }

    public Completable stopConnection() {
        return hubConnection.stop();
    }
}
```

**Usage**:

```java
String hubUrl = "https://your-api-url/hubs/friends";
String jwtToken = "your-jwt-token-here";
FriendsHubClient client = new FriendsHubClient(hubUrl, jwtToken);
client.startConnection()
    .blockingAwait(); // Wait for connection to establish
```

**Notes**:

* Replace `your-api-url` with the actual server URL.
* The `accessToken` must include a valid JWT with the `userId` claim to avoid connection abortion.
* Use `stopConnection()` to gracefully disconnect when done.

### <mark style="color:purple;">Subscribing to Hub Events</mark>

The `FriendsHub` sends three client-side events: `FriendRequestReceived`, `FriendRequestAccepted`, and `FriendRequestRejected`. Below is an example of subscribing to these events.

```java
public class FriendsHubClient {
    // ... (constructor and connection methods as above)

    public void subscribeToEvents() {
        // Handle FriendRequestReceived event
        hubConnection.on("FriendRequestReceived", (userId) -> {
            System.out.println("Received friend request from user ID: " + userId);
            // Handle the friend request (e.g., update UI or notify user)
        }, String.class);

        // Handle FriendRequestAccepted event
        hubConnection.on("FriendRequestAccepted", (friendshipId) -> {
            System.out.println("Friend request accepted, friendship ID: " + friendshipId);
            // Update UI or local state to reflect new friendship
        }, String.class);

        // Handle FriendRequestRejected event
        hubConnection.on("FriendRequestRejected", (friendshipId) -> {
            System.out.println("Friend request rejected, friendship ID: " + friendshipId);
            // Update UI or notify user of rejection
        }, String.class);
    }
}
```

**Usage**:

```java
FriendsHubClient client = new FriendsHubClient(hubUrl, jwtToken);
client.subscribeToEvents();
client.startConnection().blockingAwait();
```

**Notes**:

*   The `userId` and `friendshipId` parameters are received as strings (representing `Guid` values) due to JSON serialization. Parse them to `UUID` if needed:

    ```java
    UUID uuid = UUID.fromString(userId);
    ```
* Call `subscribeToEvents()` before starting the connection to ensure event handlers are registered.

### <mark style="color:purple;">Invoking Hub Methods</mark>

#### SendRequest

Sends a friend request to another user.

```java
public Completable sendFriendRequest(String targetUserId) {
    return hubConnection.invoke(HubResult.class, "SendRequest", targetUserId)
        .doOnSuccess(result -> {
            switch (result.getStatus()) {
                case 201: // Created
                    System.out.println("Friend request sent successfully");
                    break;
                case 400: // BadRequest
                    System.err.println("Bad request: " + result.getErrorMessage());
                    break;
                case 401: // Unauthorized
                    System.err.println("Unauthorized: " + result.getErrorMessage());
                    break;
                case 409: // Conflict
                    System.err.println("Conflict: " + result.getErrorMessage());
                    break;
                case 500: // ServerError
                    System.err.println("Server error: " + result.getErrorMessage());
                    break;
                default:
                    System.err.println("Unexpected status: " + result.getStatus());
            }
        })
        .ignoreElement();
}
```

**Usage**:

```java
client.sendFriendRequest("target-user-guid-here")
    .blockingAwait();
```

**Notes**:

* The `targetUserId` should be a valid `Guid` string (e.g., "550e8400-e29b-41d4-a716-446655440000").
* The response is a `HubResult<object>` with a status code and optional error message.

#### AcceptRequest

Accepts a friend request by its friendship ID.

```java
public Completable acceptFriendRequest(String friendshipId) {
    return hubConnection.invoke(HubResult.class, "AcceptRequest", friendshipId)
        .doOnSuccess(result -> {
            switch (result.getStatus()) {
                case 200: // OK
                    System.out.println("Friend request accepted successfully");
                    break;
                case 400: // BadRequest
                    System.err.println("Bad request: " + result.getErrorMessage());
                    break;
                case 401: // Unauthorized
                    System.err.println("Unauthorized: " + result.getErrorMessage());
                    break;
                case 500: // ServerError
                    System.err.println("Server error: " + result.getErrorMessage());
                    break;
                default:
                    System.err.println("Unexpected status: " + result.getStatus());
            }
        })
        .ignoreElement();
}
```

**Usage**:

```java
client.acceptFriendRequest("friendship-guid-here")
    .blockingAwait();
```

#### RejectRequest

Rejects a friend request by its friendship ID.

```java
public Completable rejectFriendRequest(String friendshipId) {
    return hubConnection.invoke(HubResult.class, "RejectRequest", friendshipId)
        .doOnSuccess(result -> {
            switch (result.getStatus()) {
                case 200: // OK
                    System.out.println("Friend request rejected successfully");
                    break;
                case 400: // BadRequest
                    System.err.println("Bad request: " + result.getErrorMessage());
                    break;
                case 401: // Unauthorized
                    System.err.println("Unauthorized: " + result.getErrorMessage());
                    break;
                case 500: // ServerError
                    System.err.println("Server error: " + result.getErrorMessage());
                    break;
                default:
                    System.err.println("Unexpected status: " + result.getStatus());
            }
        })
        .ignoreElement();
}
```

**Usage**:

```java
client.rejectFriendRequest("friendship-guid-here")
    .blockingAwait();
```

### <mark style="color:purple;">Data Model</mark>

The client expects a `HubResult` response from hub methods, which is serialized as JSON:

```json
{
  "status": "number", // Corresponds to HubResultStatus (e.g., 200, 400, 401, etc.)
  "result": null, // Always null for FriendsHub methods
  "errorMessage": "string" // Null for success, error message for failures
}
```

### <mark style="color:purple;">Error Handling</mark>

* **Connection Errors**: Handle connection failures in `onClosed` or by catching exceptions from `startConnection()`.
* **Invalid User ID**: If the JWT lacks a valid `userId` claim, the hub aborts the connection, and `onClosed` is triggered with an error.
* **Hub Method Errors**: Check the `status` and `errorMessage` fields in the `HubResult` response to handle specific errors (e.g., `BadRequest`, `Unauthorized`, `Conflict`).

### <mark style="color:purple;">Example Full Client</mark>

Below is a complete example combining connection, event subscription, and method invocation.

```java
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import io.reactivex.rxjava3.core.Completable;
import io.reactivex.rxjava3.core.Single;

public class FriendsHubClient {
    private final HubConnection hubConnection;
    private final String accessToken;

    public FriendsHubClient(String hubUrl, String accessToken) {
        this.accessToken = accessToken;
        this.hubConnection = HubConnectionBuilder.create(hubUrl)
                .withAccessTokenProvider(Single.just(accessToken))
                .build();

        // Connection events
        hubConnection.onConnected(() -> {
            System.out.println("Connected to FriendsHub with connection ID: " + hubConnection.getConnectionId());
        });
        hubConnection.onClosed((exception) -> {
            System.err.println(exception != null ? "Connection closed with error: " + exception.getMessage() : "Connection closed normally");
        });

        // Subscribe to hub events
        hubConnection.on("FriendRequestReceived", (userId) -> {
            System.out.println("Received friend request from user ID: " + userId);
        }, String.class);
        hubConnection.on("FriendRequestAccepted", (friendshipId) -> {
            System.out.println("Friend request accepted, friendship ID: " + friendshipId);
        }, String.class);
        hubConnection.on("FriendRequestRejected", (friendshipId) -> {
            System.out.println("Friend request rejected, friendship ID: " + friendshipId);
        }, String.class);
    }

    public Completable startConnection() {
        return hubConnection.start();
    }

    public Completable stopConnection() {
        return hubConnection.stop();
    }

    public Completable sendFriendRequest(String targetUserId) {
        return hubConnection.invoke(HubResult.class, "SendRequest", targetUserId)
            .doOnSuccess(this::handleHubResult)
            .ignoreElement();
    }

    public Completable acceptFriendRequest(String friendshipId) {
        return hubConnection.invoke(HubResult.class, "AcceptRequest", friendshipId)
            .doOnSuccess(this::handleHubResult)
            .ignoreElement();
    }

    public Completable rejectFriendRequest(String friendshipId) {
        return hubConnection.invoke(HubResult.class, "RejectRequest", friendshipId)
            .doOnSuccess(this::handleHubResult)
            .ignoreElement();
    }

    private void handleHubResult(HubResult result) {
        switch (result.getStatus()) {
            case 200:
                System.out.println("Operation successful");
                break;
            case 201:
                System.out.println("Resource created successfully");
                break;
            case 400:
                System.err.println("Bad request: " + result.getErrorMessage());
                break;
            case 401:
                System.err.println("Unauthorized: " + result.getErrorMessage());
                break;
            case 409:
                System.err.println("Conflict: " + result.getErrorMessage());
                break;
            case 500:
                System.err.println("Server error: " + result.getErrorMessage());
                break;
            default:
                System.err.println("Unexpected status: " + result.getStatus());
        }
    }

    public static void main(String[] args) {
        String hubUrl = "https://your-api-url/hubs/friends";
        String jwtToken = "your-jwt-token-here";
        FriendsHubClient client = new FriendsHubClient(hubUrl, jwtToken);

        // Start connection and send a friend request
        client.startConnection()
            .andThen(client.sendFriendRequest("550e8400-e29b-41d4-a716-446655440000"))
            .blockingAwait();

        // Simulate accepting a friend request
        client.acceptFriendRequest("friendship-guid-here")
            .blockingAwait();

        // Stop connection
        client.stopConnection().blockingAwait();
    }
}
```

**Notes**:

* Replace `your-api-url` and `your-jwt-token-here` with actual values.
* The `main` method demonstrates a complete workflow, but in a real application, you would integrate this with your UI or event loop.
* Use `UUID.fromString()` to convert string GUIDs to `UUID` objects if needed for internal logic.

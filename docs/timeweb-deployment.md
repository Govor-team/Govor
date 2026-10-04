# Deployment to Timeweb App Platform

Use the repository root as the build context and select the **Dockerfile** framework.
The Dockerfile publishes the API and starts `dotnet Govor.API.dll`, listening on port 8080.
Set the health check path to `/server/ping` and use the technical HTTPS domain from the dashboard.

## Runtime variables

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_HTTP_PORTS=8080`
- `ConnectionStrings__GovorDbContext=Host=<database-host>;Port=5432;Database=<database-name>;Username=<database-user>;Password=<database-password>`
- `JwtAccessOption__SecretKey=<random-private-signing-key>`
- `EncryptionOption__Secret=<private-encryption-secret>`
- `Firebase__Enabled=true`
- `Firebase__CredentialsJson=<complete-service-account-JSON>` (a secret runtime variable)

Alternatively, mount the service-account file and set `Firebase__CredentialsPath` or
`GOOGLE_APPLICATION_CREDENTIALS` to its container path. Relative paths are resolved
against the API content root. Never put credentials into the image or Git repository.
Replace any Firebase private key that was previously committed to a public repository.

Use the database private address only if the application is connected to the same
Timeweb private network. Use the database TLS settings supplied by Timeweb; do not
disable certificate validation. Keep existing encryption secrets when migrating
an existing database, because changing them can make existing encrypted values unreadable.

## Apply migrations

First check the target database and back up existing data. The latest group migration
removes a legacy column, consolidates duplicate memberships and removes orphan rows.

Run this command **inside the deployed container/network**, with the same database
runtime variable as the API:

```sh
dotnet Govor.API.dll --migrate-only
```

This applies pending EF Core migrations and exits without starting the HTTP server
or push worker. Firebase credentials are not required for this command. A failure
returns a nonzero exit status; do not start the deployment until it succeeds.
The normal API startup does not automatically modify the database schema.

If using EF tooling from the source checkout instead, use version 10.x:

```sh
dotnet ef database update --project Govor.Domain --startup-project Govor.API
```

For EF tooling, set `Firebase__Enabled=false` in the tooling process only, or supply
valid Firebase credentials. The database must be reachable from that process.

## Verify

Confirm that the app log reports `Now listening on: http://[::]:8080` (or an equivalent
8080 listener) and has no database errors. Request `https://<domain>/server/ping` and
expect HTTP 200. Then check an authenticated API request and SignalR connections.
An HTTP ping alone does not verify database schema or Firebase delivery.

The API stores media under `uploads` on disk. Configure persistent storage before
relying on uploads surviving container replacement.

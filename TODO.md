# Project TODO

This checklist captures the remaining work needed to align the solution with the assessment brief.

## API
- [ ] Allow patients to call the booking endpoints instead of requiring the `RequireAdminRole` policy for every action. Update the authorization attributes so authenticated users can list availability and create bookings as intended in the scenario.
- [ ] Harden the login flow to handle unknown email addresses gracefully before calling `CheckPasswordAsync`, and return consistent error responses when credentials are invalid.
- [ ] Add validation and clearer problem details responses around booking creation (e.g., distinguish between a missing slot vs. a slot already booked) so the UI can show accurate error messages.
- [ ] Implement logging inside the placeholder `NoteException` helpers (or replace them with proper logging) to aid troubleshooting when API calls fail.
- [ ] Review seeding and configuration defaults (connection strings, token key, CORS origins) to ensure the API can run locally with the provided UI without extra manual tweaks.

## UI
- [ ] Persist the JWT token (e.g., local storage) and restore it on startup so the HttpClient keeps the Authorization header after the page reloads.
- [ ] Surface authentication errors in the login bar using user-friendly copy and accessibility-friendly markup, and reset the busy state when API calls fail unexpectedly.
- [ ] Handle API problem responses in the booking form by mapping server-side validation errors to the appropriate fields instead of showing a single generic message.
- [ ] Disable or hide booking actions when the user is not authenticated to avoid confusing unauthenticated visitors who cannot submit a booking.
- [ ] Add navigation routes (or components) for viewing existing bookings and user profile data once the corresponding API endpoints are available.

## Testing & Tooling
- [ ] Expand API unit test coverage to include `GetBookingByIdAsync`, the login flow, and negative paths around authorization.
- [ ] Add UI integration or component tests (e.g., using bUnit) to validate the booking flow and login bar interactions.
- [ ] Set up a CI build that runs `dotnet build` and the test suite so build errors and warnings are caught automatically.


# ADR 002: Auth Backend Structure

## Status
Accepted - 2025-10-16

## Context
Implementation of authentication backend with Supabase Auth integration and custom email system.

## Decision

### Validation Architecture
- Use IUnifiedValidatorService (based on WatchdogAPI pattern)
- Automatic validator registration through reflection
- Validators in `Validation/` folders next to Handlers
- FluentValidation with async validation

### Email Service
- Location: Infrastructure/Email/
- Library: MailKit (most mature .NET email library)
- HTML templates as embedded resources
- 3 templates: Verification, PasswordReset, PasswordChanged

### Supabase Integration
- Supabase Auth for JWT tokens
- Custom SMTP for email sending (full control)
- HTTP-only cookies for token storage

### Test Project Structure
- Separate PortalForge.Tests project
- Split into Unit and Integration
- xUnit + FluentAssertions + Moq
- TestDataBuilder pattern for complex objects

## Consequences

### Positive
- Clean separation of concerns
- Automatic validation without boilerplate
- Easy testing (DI-friendly)
- Full control over emails
- Secure token storage
- Isolated unit tests

### Negative
- Additional EmailService complexity
- Need to manage SMTP credentials
- More projects in solution

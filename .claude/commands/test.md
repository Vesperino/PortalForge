---
description: Generate comprehensive tests for existing or new code
---

Please help me write tests for the code. Follow these steps:

1. **Identify Test Scope**:
   - Ask me what needs testing (component, API, feature)
   - Determine test type (unit, integration, E2E)

2. **Backend Tests (xUnit + FluentAssertions)**:
   - Test domain entity logic
   - Test command/query handlers
   - Test validators
   - Test API endpoints (integration tests)
   - Mock external dependencies
   - Test error scenarios
   - Test edge cases

3. **Frontend Tests**:
   - **Unit Tests (Vitest)**:
     - Test composables
     - Test utility functions
     - Test component logic with vue-test-utils
     - Mock API calls
   - **E2E Tests (Playwright)**:
     - Test user flows
     - Test critical paths
     - Test form submissions

4. **Test Structure**:
   - Use AAA pattern (Arrange, Act, Assert)
   - Clear test names describing behavior
   - One assertion per test (when possible)
   - Test both happy path and error cases

5. **Coverage**:
   - Aim for 70%+ coverage on business logic
   - Focus on critical paths
   - Don't test framework code

Generate test code with clear comments explaining what's being tested.

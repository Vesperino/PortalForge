---
description: Review code for quality, best practices, and potential issues
---

Please review the recent code changes and check for:

## Backend (.NET)
- [ ] Clean Architecture principles followed
- [ ] CQRS pattern properly implemented
- [ ] Proper use of async/await
- [ ] FluentValidation for inputs
- [ ] Proper error handling (Result pattern or exceptions)
- [ ] Security considerations (auth, validation, sanitization)
- [ ] Performance considerations (pagination, caching)
- [ ] Unit tests included
- [ ] XML documentation for public APIs
- [ ] Logging with structured data

## Frontend (Vue/Nuxt)
- [ ] Composition API with `<script setup>`
- [ ] TypeScript types properly defined
- [ ] No use of `any` type
- [ ] Proper component structure
- [ ] Tailwind CSS for styling
- [ ] Accessibility considerations (WCAG 2.1 AA)
- [ ] Error handling and user feedback
- [ ] Loading states implemented
- [ ] Responsive design
- [ ] Tests included

## General
- [ ] Follows conventions in CLAUDE.md
- [ ] No secrets or sensitive data committed
- [ ] Proper commit messages
- [ ] Documentation updated
- [ ] MVP scope respected

Provide a summary with:
1. ✅ What looks good
2. ⚠️ Suggestions for improvement
3. 🐛 Potential bugs or issues
4. 🔐 Security concerns

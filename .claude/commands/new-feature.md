---
description: Start implementing a new feature with proper planning and structure
---

I want to implement a new feature. Please help me by:

1. **Understand Requirements**: Ask me about the feature details if needed
2. **Check PRD**: Verify the feature is in scope according to `.ai/prd.md`
3. **Plan Implementation**:
   - Break down into backend and frontend tasks
   - Identify affected files and components
   - List required dependencies
4. **Create Task List**: Use TodoWrite to track all tasks
5. **Backend First**:
   - Create domain entity (if needed)
   - Create command/query with MediatR
   - Add validator with FluentValidation
   - Implement handler
   - Create API endpoint
   - Write tests
6. **Frontend Second**:
   - Create composable for API integration
   - Create Pinia store (if needed)
   - Build UI components
   - Add pages/routes
   - Write tests
7. **Document**: Update relevant `.ai/` documentation
8. **Commit**: Create proper git commits following conventional commits

Before starting, provide me with a summary of the plan for approval.

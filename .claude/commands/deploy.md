---
description: Prepare and execute deployment to staging or production
---

I want to deploy the application. Please help me:

1. **Pre-Deployment Checks**:
   - [ ] All tests passing
   - [ ] Code reviewed and approved
   - [ ] Documentation updated
   - [ ] No console.logs or debug code
   - [ ] Environment variables configured
   - [ ] Database migrations ready
   - [ ] Secrets properly secured

2. **Backend Deployment**:
   - Build Docker image
   - Tag with version number
   - Push to container registry
   - Update deployment configuration
   - Run database migrations
   - Deploy to VPS/cloud
   - Verify health check endpoint

3. **Frontend Deployment**:
   - Run production build
   - Verify no build errors
   - Check bundle size
   - Deploy to hosting (Vercel/Netlify/VPS)
   - Verify environment variables
   - Test critical paths

4. **Post-Deployment**:
   - Smoke test critical functionality
   - Monitor logs for errors
   - Check performance metrics
   - Verify database connections
   - Test authentication flow
   - Notify team of deployment

5. **Rollback Plan**:
   - Document how to rollback
   - Keep previous version available
   - Have database backup ready

Ask me which environment (staging/production) and proceed with the checklist.

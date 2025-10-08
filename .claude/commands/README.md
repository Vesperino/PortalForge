# Claude Custom Commands

Custom slash commands for PortalForge project to streamline AI-assisted development.

## Available Commands

### `/update-docs`
Update project documentation after completing a feature or making significant changes.
- Updates `.ai/` folder documentation
- Creates progress log entries
- Documents architectural decisions

### `/new-feature`
Start implementing a new feature with proper planning and structure.
- Verifies feature is in scope (PRD)
- Creates implementation plan
- Breaks down into backend/frontend tasks
- Uses TodoWrite for tracking

### `/review-code`
Review code for quality, best practices, and potential issues.
- Checks Clean Architecture adherence
- Validates TypeScript usage
- Verifies security considerations
- Ensures accessibility standards

### `/debug`
Help debug an issue by analyzing logs, code, and potential causes.
- Gathers diagnostic information
- Analyzes potential root causes
- Proposes solutions with examples
- Suggests prevention strategies

### `/test`
Generate comprehensive tests for existing or new code.
- Backend: xUnit + FluentAssertions
- Frontend: Vitest + Playwright
- Follows AAA pattern
- Tests happy path and edge cases

### `/deploy`
Prepare and execute deployment to staging or production.
- Pre-deployment checklist
- Deployment steps for backend/frontend
- Post-deployment verification
- Rollback plan

## Usage

Type `/` in the Claude chat to see available commands, then select the command you want to use.

Example:
```
/new-feature
```

Claude will then follow the workflow defined in the command file.

## Creating New Commands

1. Create a new `.md` file in `.claude/commands/`
2. Add frontmatter with description:
   ```yaml
   ---
   description: Brief description of what this command does
   ---
   ```
3. Write the command prompt
4. Commit to Git to share with team

## Best Practices

- Use commands to maintain consistency across sessions
- Commands help new team members understand workflows
- Update commands as project evolves
- Keep commands focused on specific tasks
- Use TodoWrite in commands for complex multi-step tasks

---

**Note**: These commands are checked into Git and available to all team members.

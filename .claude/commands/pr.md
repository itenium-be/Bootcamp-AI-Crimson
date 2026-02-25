# Create PR with AI Code Review

Create a pull request and post an AI code review to GitHub.

## Steps

1. **Check branch state**
   - Run `git status` and `git log origin/master..HEAD --oneline` to see what will be in the PR
   - If on master, stop and ask user to create a feature branch first
   - Push branch if not yet pushed: `git push -u origin HEAD`

2. **Analyze changes**
   - Run `git diff origin/master...HEAD` to see all changes
   - Understand what the PR accomplishes

3. **Create the PR**
   - Use `gh pr create` with a clear title and summary
   - Format body as:
     ```
     ## Summary
     <bullet points of what changed>

     ## Test plan
     <how to verify the changes>
     ```

4. **Perform code review**
   Review the diff for:
   - Bugs or logic errors
   - Security issues (injection, secrets, auth)
   - Missing error handling
   - Missing tests
   - Performance concerns

   Be pragmatic - this is a hackathon. Flag real issues, not nitpicks.

5. **Post review to GitHub**
   Use `gh pr review --comment --body "<review>"` to post findings.

   If no issues found:
   ```
   gh pr review --approve --body "AI Review: Code looks good. No blocking issues found."
   ```

   If issues found:
   ```
   gh pr review --request-changes --body "<detailed findings>"
   ```

6. **Return the PR URL** so the user can view it.

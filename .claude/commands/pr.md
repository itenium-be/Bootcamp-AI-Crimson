# Create PR with AI Code Review

Create a pull request and post an AI code review to GitHub with line-specific comments.

## Steps

1. **Check for uncommitted changes**
   - Run `git status` to check for staged/unstaged changes
   - If there are uncommitted changes:
     - Run `git diff` to understand what changed
     - Ask the user if they want to commit first, suggesting a commit message based on the changes
     - Use AskUserQuestion with options like:
       - "Yes, commit with suggested message" (show the suggestion)
       - "Yes, but let me provide the message"
       - "No, I'll handle it myself"
     - If yes, stage and commit the changes

2. **Check branch state**
   - Run `git branch --show-current` to get current branch
   - If on master/main:
     - Analyze the changes to suggest a branch name (e.g., `feature/add-user-auth`, `fix/login-validation`)
     - Ask the user for the branch name using AskUserQuestion with options:
       - The suggested branch name (recommended)
       - "Let me type a different name"
     - Create the branch: `git checkout -b <branch-name>`
   - Run `git log origin/master..HEAD --oneline` to see commits for PR
   - Push branch if not yet pushed: `git push -u origin HEAD`

3. **Analyze changes**
   - Run `git diff origin/master...HEAD` to see all changes
   - Understand what the PR accomplishes

4. **Create the PR**
   - Use `gh pr create` with a clear title and summary
   - Format body as:
     ```
     ## Summary
     <bullet points of what changed>

     ## Test plan
     <how to verify the changes>
     ```

5. **Perform code review**
   Review the diff for:
   - Bugs or logic errors
   - Security issues (injection, secrets, auth)
   - Missing error handling
   - Performance concerns
   - Anti-patterns (e.g., `catch (Exception ex) { throw ex; }` instead of `throw;`)

   Be pragmatic - this is a hackathon. Flag real issues, not nitpicks.

6. **Post line-specific comments**
   For each issue found, post a comment on the specific line:
   ```bash
   # Get PR number and commit SHA
   PR_NUMBER=$(gh pr view --json number --jq '.number')
   COMMIT_SHA=$(gh pr view --json headRefOid --jq '.headRefOid')
   REPO=$(gh repo view --json nameWithOwner --jq '.nameWithOwner')

   # Post comment on specific line
   gh api repos/$REPO/pulls/$PR_NUMBER/comments \
     --method POST \
     --field body="Your comment here" \
     --field commit_id="$COMMIT_SHA" \
     --field path="path/to/file.cs" \
     --field line=42 \
     --field side="RIGHT"
   ```

   Common issues to flag:
   - `throw ex;` → "Use `throw;` to preserve stack trace"
   - `catch (Exception) { }` → "Empty catch block swallows errors"
   - Hardcoded secrets → "Move to configuration/environment variables"
   - Missing null checks → "Potential NullReferenceException"
   - SQL concatenation → "Use parameterized queries to prevent SQL injection"

7. **Post summary review**
   After line comments, post overall review:
   ```bash
   gh pr review --comment --body "AI Review: Found N issues - see inline comments."
   ```

   Or if no issues:
   ```bash
   gh pr review --approve --body "AI Review: Code looks good."
   ```

8. **Return the PR URL** so the user can view it.

# Create PR with AI Code Review

Create a pull request and post an AI code review to GitHub with line-specific comments.

## Steps

1. **Check branch state**
   - Run `git status` and `git log origin/master..HEAD --oneline` to see commits
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
   - Performance concerns
   - Anti-patterns (e.g., `catch (Exception ex) { throw ex; }` instead of `throw;`)

   Be pragmatic - this is a hackathon. Flag real issues, not nitpicks.

5. **Post line-specific comments**
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

6. **Post summary review**
   After line comments, post overall review:
   ```bash
   gh pr review --comment --body "AI Review: Found N issues - see inline comments."
   ```

   Or if no issues:
   ```bash
   gh pr review --approve --body "AI Review: Code looks good."
   ```

7. **Return the PR URL** so the user can view it.

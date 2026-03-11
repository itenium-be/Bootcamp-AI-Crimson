# Git Worktree Helper

Manage git worktrees to run multiple Claude instances in parallel on different features.

## Usage

- `/worktree add <branch-name>` - Create a new worktree
- `/worktree list` - List existing worktrees
- `/worktree remove <branch-name>` - Remove a worktree

## Arguments

$ARGUMENTS - The action and optional branch name (e.g., "add feature-login", "list", "remove feature-login")

## Steps

1. **Parse arguments**
   - Extract the action (add/list/remove) and branch name from $ARGUMENTS
   - If no arguments or invalid action, show usage help

2. **For `list`**
   - Run `git worktree list`
   - Display results

3. **For `add <branch-name>`**
   - Validate branch name is provided
   - Get repository name: `basename $(git rev-parse --show-toplevel)`
   - Create worktree in sibling directory: `git worktree add ../<repo>-<branch-name> -b <branch-name>`
   - If branch already exists, use: `git worktree add ../<repo>-<branch-name> <branch-name>`
   - Print instructions:
     ```
     Worktree created at: ../<repo>-<branch-name>

     To start a parallel Claude session:
       cd ../<repo>-<branch-name>
       claude

     When done, merge your branch and clean up:
       git checkout master && git merge <branch-name>
       /worktree remove <branch-name>
     ```

4. **For `remove <branch-name>`**
   - Validate branch name is provided
   - Get repository name: `basename $(git rev-parse --show-toplevel)`
   - Remove worktree: `git worktree remove ../<repo>-<branch-name>`
   - Optionally delete the branch if merged: `git branch -d <branch-name>`
   - Print confirmation

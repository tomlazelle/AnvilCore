# GitHub Branch Protection Setup

Use this once in GitHub to enforce merge policy for protected branches (for example: `main`).

## 1. Open Branch Protection

1. Go to repository settings.
2. Open **Rules** -> **Rulesets** (recommended) or **Branches** -> **Branch protection rules**.
3. Create a rule targeting your protected branch (for example, `main`).

## 2. Require PRs and your review

Enable:

- Require a pull request before merging
- Required approvals: 1
- Require review from Code Owners
- Dismiss stale pull request approvals when new commits are pushed
- Do not allow bypassing the above settings

This repo has [../.github/CODEOWNERS](../.github/CODEOWNERS), which assigns all files to `@tomlazelle`.
With "Require review from Code Owners" enabled, your review is required.

## 3. Require CI checks before merge

Enable:

- Require status checks to pass before merging

Select this status check after the first workflow run:

- Build and Test / build-test

## 4. Keep protected branch safe

Enable:

- Require branches to be up to date before merging
- Restrict who can push to matching branches (optional but recommended)

If you restrict push access, include only your account or trusted automation (for example, release bot) as needed.

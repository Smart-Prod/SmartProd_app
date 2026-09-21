# Branch Protection Rules Configuration

## Main Branch (`main`)

```yaml
# Configure via GitHub UI: Settings > Branches > Branch protection rules
# Or use GitHub CLI:
# gh api --method PUT /repos/Smart-Prod/SmartProd_app/branches/main/protection \
#   -f required_status_checks='{"strict":true,"contexts":["CI"]}' \
#   -f enforce_admins=true \
#   -f required_pull_request_reviews='{"required_approving_review_count":2,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}' \
#   -f restrictions='{"users":[],"teams":["backend-team","frontend-team"]}'
```

### Required Settings:
- [x] **Require a pull request before merging**
  - [x] Require approvals: **2**
  - [x] Dismiss stale PR approvals when new commits are pushed
  - [x] Require review from CODEOWNERS
  - [x] Require conversation resolution before merging

- [x] **Require status checks to pass before merging**
  - [x] Require branches to be up to date before merging
  - Required checks: `CI` (backend-test, frontend-test, security-scan)

- [x] **Require signed commits** (optional but recommended)

- [x] **Require linear history** (no merge commits)

- [x] **Include administrators** (enforce for admins too)

- [ ] **Restrict who can push to matching branches** (optional - use teams instead)

- [ ] **Allow force pushes** - **NEVER ENABLE**

- [ ] **Allow deletions** - **NEVER ENABLE**

---

## Develop Branch (`develop`)

Same as main but:
- Required approvals: **1**
- Allow auto-merge after checks pass

---

## Release Branches (`release/*`)

```yaml
# Pattern: release/*
# Same as main but with:
# - Required approvals: 2
# - Only release managers can push
```

---

## Hotfix Branches (`hotfix/*`)

```yaml
# Pattern: hotfix/*
# Same as main but:
# - Required approvals: 1 (expedited)
# - Can bypass some checks with admin approval
```

---

## GitHub CLI Commands to Apply

```bash
# Main branch
gh api --method PUT /repos/Smart-Prod/SmartProd_app/branches/main/protection \
  -f required_status_checks='{"strict":true,"contexts":["CI"]}' \
  -f enforce_admins=true \
  -f required_pull_request_reviews='{"required_approving_review_count":2,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}'

# Develop branch
gh api --method PUT /repos/Smart-Prod/SmartProd_app/branches/develop/protection \
  -f required_status_checks='{"strict":true,"contexts":["CI"]}' \
  -f enforce_admins=true \
  -f required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}'

# Release branches pattern
gh api --method POST /repos/Smart-Prod/SmartProd_app/rules/branches \
  -f name='release/*' \
  -f required_status_checks='{"strict":true,"contexts":["CI"]}' \
  -f required_pull_request_reviews='{"required_approving_review_count":2,"dismiss_stale_reviews":true}'

# Hotfix branches pattern
gh api --method POST /repos/Smart-Prod/SmartProd_app/rules/branches \
  -f name='hotfix/*' \
  -f required_status_checks='{"strict":true,"contexts":["CI"]}' \
  -f required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true}'
```

---

## Required Repository Secrets

Configure in: Settings > Secrets and variables > Actions

| Secret Name | Description | Required For |
|-------------|-------------|--------------|
| `SMARTPROD_DB_CONNECTION` | PostgreSQL connection string | Backend tests, staging/prod deploy |
| `SMARTPROD_JWT_SECRET` | JWT signing key (64+ chars base64) | Backend tests, all environments |
| `GHCR_TOKEN` | GitHub Container Registry token | Docker builds |
| `KUBECONFIG_STAGING` | Kubernetes config for staging | Staging deploy |
| `KUBECONFIG_PRODUCTION` | Kubernetes config for production | Production deploy |
| `SLACK_WEBHOOK_URL` | Slack notification webhook | Deployment notifications |

---

## Required Environments

Configure in: Settings > Environments

### `staging`
- Protection rules: None (auto-deploy from develop)
- Secrets: `KUBECONFIG_STAGING`, `SMARTPROD_DB_CONNECTION`, `SMARTPROD_JWT_SECRET`

### `production`
- Protection rules: **Required reviewers** (release managers team)
- Wait timer: 5 minutes
- Secrets: `KUBECONFIG_PRODUCTION`, `SMARTPROD_DB_CONNECTION`, `SMARTPROD_JWT_SECRET`

---

## Verification Checklist

After applying:
- [ ] Try pushing directly to main - should be blocked
- [ ] Open PR without reviews - should not allow merge
- [ ] Open PR with 1 approval - should not allow merge (main needs 2)
- [ ] Open PR with 2 approvals + CI passing - should allow merge
- [ ] Push to develop - should trigger CI
- [ ] Tag `v1.0.0` - should trigger production deploy workflow
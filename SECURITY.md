# Security Policy

## Supported Versions

We release patches for security vulnerabilities. Here are the versions currently supported:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | ✅ Yes             |
| < 1.0   | ❌ No              |

## Reporting a Vulnerability

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via:

1. **GitHub Security Advisories** (preferred): Go to the Security tab > "Report a vulnerability"
2. **Email**: security@smartprod.example.com (replace with actual email)

### What to Include

- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Any known workarounds
- Your contact information

### Response Timeline

- **Acknowledgment**: Within 48 hours
- **Initial Assessment**: Within 5 business days
- **Fix Timeline**: 
  - Critical: Within 7 days
  - High: Within 14 days
  - Medium: Within 30 days
  - Low: Next scheduled release

## Security Best Practices

### For Contributors

1. **Never commit secrets** - Use environment variables or GitHub Secrets
2. **Validate all inputs** - Both client and server side
3. **Use parameterized queries** - Never concatenate SQL
4. **Keep dependencies updated** - Dependabot will create PRs
5. **Follow OWASP guidelines** - Especially for authentication/authorization

### For Deployment

1. **Rotate secrets regularly** - JWT secrets, DB passwords
2. **Use HTTPS everywhere** - Enforce TLS 1.2+
3. **Implement rate limiting** - Prevent brute force attacks
4. **Monitor audit logs** - Track authentication events
5. **Run security scans** - Trivy, CodeQL in CI/CD

## Vulnerability Disclosure

We follow responsible disclosure. We will:
- Acknowledge your report
- Investigate and validate
- Develop and test a fix
- Release a patch
- Credit you (if desired) in the release notes

## Security Contacts

- Security Team: @Smart-Prod/security-team
- Email: security@smartprod.example.com
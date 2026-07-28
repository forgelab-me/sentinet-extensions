# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability in this project:

1. **DO NOT** open a public issue
2. Email security concerns to: [contact@forgelab.me]
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

## Security Best Practices for Deployment

When deploying this extension to your Sentinet environment:

1. **Verify checksums** of downloaded DLLs from GitHub Releases
2. **Scan DLLs** with your organization's security tools before deployment
3. **Test in non-production** environments first
4. **Review configuration** of obfuscation patterns for your specific needs
5. **Monitor logs** after deployment for unexpected behavior

## Dependencies

This project depends on:
- Nevatech.Sentinet (proprietary)
- Newtonsoft.Json (open source, regularly updated)

We monitor security advisories for all dependencies and update promptly.

## Responsible Disclosure

We appreciate responsible disclosure of security vulnerabilities and will acknowledge contributors (with permission) in release notes.

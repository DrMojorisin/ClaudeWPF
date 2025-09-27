# Security Policy

## Supported Versions

We actively support the following versions of WPFBase with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability in WPFBase, please report it responsibly.

### How to Report

1. **Do NOT create a public GitHub issue** for security vulnerabilities
2. **Email us directly** at: [security@example.com] (replace with your email)
3. **Use our private vulnerability disclosure process**

### What to Include

Please include the following information:
- Description of the vulnerability
- Steps to reproduce the issue
- Potential impact assessment
- Suggested fix (if you have one)

### Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Timeline**: Within 30 days for critical issues

### Security Best Practices

When using WPFBase:

1. **Keep Dependencies Updated**
   - Regularly update NuGet packages
   - Monitor for security advisories

2. **Input Validation**
   - Always validate user input
   - Use the built-in validation features

3. **Configuration Security**
   - Don't commit secrets to version control
   - Use secure configuration management
   - Review appsettings.json for sensitive data

4. **Logging Security**
   - Don't log sensitive information
   - Review log outputs for data leaks
   - Use appropriate log levels

### Known Security Considerations

- **Serialization**: Be cautious when deserializing untrusted data
- **File Operations**: Validate file paths to prevent directory traversal
- **Network Operations**: Use HTTPS for all external communications
- **User Permissions**: Follow principle of least privilege

### Security Updates

Security updates will be:
- Released as patch versions (e.g., 1.0.1)
- Announced in release notes
- Tagged with security advisory labels
- Communicated through GitHub Security Advisories

## Acknowledgments

We appreciate security researchers and users who help keep WPFBase secure by responsibly disclosing vulnerabilities.

## Contact

For security-related questions or concerns:
- Security Email: [security@example.com] (replace with your email)
- General Contact: GitHub Issues (for non-security questions)

---

Thank you for helping keep WPFBase and the community safe!
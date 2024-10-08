A small program that automatically renews certificates for nginx.

- Connects via SSH to a Linux machine running CertBot
- Performs a certificate request with a CSR (runs a renew command)
- Downloads the certificates
- Reloads nginx to apply the certificates

---

SSH server cipher must be compatible with [SSH.NET](https://github.com/sshnet/SSH.NET)
Does not need administrative privileges - must run as the same user as the nginx process in order to be able to signal a reload.

Written in C# (12.0) .Net Framework v4.8.1
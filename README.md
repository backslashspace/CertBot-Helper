A small program that automatically renews certificates for nginx.

- Connects via SSH to a Linux machine running CertBot
- Performs a certificate request with a CSR (runs a renew command)
- Downloads the certificates
- Reloads nginx to apply the certificates

---

Written in C# - .Net Framework 4.8.1 (C# 12.0)
Does not need administrative privileges - must run as the same user as the nginx process in order to be able to signal a reload.
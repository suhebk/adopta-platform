# Migration Execution Approval Checklist

## Purpose

This checklist defines the minimum approval requirements before any EF migration source may be executed.

Migration execution is not approved by Sprint 5 Slice 5.

## Explicit Non-Approval

The following remain not approved:

- running migrations;
- automatic startup migration;
- automatic database creation;
- startup database mutation;
- production Azure SQL deployment;
- deployment automation;
- live SQL Server connectivity checks.

## Required Approval Gates

- Migration source reviewed and approved.
- Target environment approved.
- Data owner approval recorded.
- Engineering owner approval recorded.
- Security review completed.
- Tenant isolation review completed.
- Rollback plan reviewed and rehearsed.
- Incident-response plan reviewed.
- Secrets confirmed to come from secure configuration only.
- Release window approved.

## Pre-Execution Validation

- Confirm no repository file contains real connection strings, hostnames, passwords, tokens, tenant secrets, or credentials.
- Confirm application startup contains no automatic migration or database creation logic.
- Confirm appsettings remain safe by default.
- Confirm migration source is traceable to reviewed schema changes.
- Confirm backup and restore process is tested in an approved environment.

## Rollback Checklist

- Identify rollback owner.
- Confirm backup timestamp.
- Confirm reverse-script or restore approach.
- Confirm application version compatibility.
- Confirm tenant-isolation impact.
- Confirm communications path for incident response.

## Post-Execution Checklist

This section is for a future approved execution slice only.

- Confirm migration completed.
- Confirm no tenant isolation regression.
- Confirm audit/history writes behave as expected.
- Confirm rollback remains available.
- Confirm operational logs contain no sensitive values.

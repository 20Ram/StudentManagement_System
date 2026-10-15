# Validation Review and Implementation Report

**Review date:** 2026-10-15  
**Scope:** `CrudOperation` ASP.NET Core API and the `SmsFrontend` Razor/JavaScript client.  
**Status:** The API and client-side validation changes described here are implemented. The database migration has been generated but not applied.

## Summary

The API controllers use `[ApiController]`, so MVC automatically validates request DTOs and returns HTTP 400 for invalid model state. The implementation adds missing field bounds and role-specific rules, aligns update actions to their existing `PUT` semantics, adds conflict checks for duplicate email, constrains pagination and IDs, strengthens document upload checks, and mirrors common input checks in the browser.

Client validation is for usability only. Direct HTTP callers can bypass it, so all authoritative rules are enforced by the API/service. Persistence-level uniqueness is provided by the new EF migration and becomes effective after that migration is applied.

## Validation applied by request

| Request / route | Implemented validation |
|---|---|
| `CreateStudentDto` / `POST Admin/create-student` | Name required, 2–100 characters, non-whitespace; email required, valid, maximum 254 characters; age 7–99; course required and up to 100 characters; marks required and 0–100; role fixed to Student. User and student profile creation run in a transaction. |
| `CreateTeacherDto` / `POST Admin/create-teacher` | Name required, 2–100 characters, non-whitespace; valid email up to 254 characters; age 18–99; department required and up to 100 characters; role fixed to Teacher. User and teacher profile creation run in a transaction. |
| `RegisterRequestDto` / `POST Auth/register` | Required name and email with bounds; student role requires age 7–99, course, and marks 0–100; teacher role requires age 18–99 and department. Unverified registration retries update the same role’s profile; changing role during an unverified registration is rejected. |
| `UpdateStudentDto` / student and teacher student-update `PUT`s | Full-update contract: name, email, age, course, and marks are required; strings are bounded and non-whitespace; marks is 0–100. The actions now apply all required values. Duplicate email is rejected with 409. |
| `UpdateTeacherDto` / `PUT Teacher/Update-teacher` | Full-update contract: name, email, age, and department are required and bounded. Unused `Roll` was removed. Duplicate email is rejected with 409. |
| `LoginRequestDto` / `POST Auth/Login` | Required, valid email up to 254 characters; email is normalized before lookup. |
| `VerifyOtpDto` / OTP verification routes | Required, valid email up to 254 characters; OTP must be exactly six ASCII digits. Service code repeats the format guard and limits verification to six attempts. Generating a new OTP invalidates earlier unused OTPs. |
| `UpdateRoledto` / `PUT Admin/change-role/{id}` | Role restricted to exact values `Admin`, `Teacher`, or `Student`; user ID must be positive. Student/teacher roles require a matching profile; attempts to change to a role without one return 409. |
| `DocumentUploadDto` / `POST Document/upload` | Student ID must be positive; file is required, nonempty, and no larger than 5 MB. Extension allowlist is case-insensitive (`pdf`, `jpg`, `jpeg`, `png`, `doc`, `docx`); content signatures are checked; stored filenames are generated, not client supplied; MIME type is derived from the validated extension. Request/form limits include multipart overhead. |
| `ChatRequestDto` / `POST Chat/ask` | Required message, 1–2000 characters; the service additionally rejects whitespace-only messages and enforces the same maximum. |
| `PaginationQueryDto` / student list | Page number 1–2,000,000; page size 1–100; search up to 100 characters; sort field and direction allowlisted. Invalid values yield model-validation 400 instead of silent clamping. `IsActive` remains accepted but unapplied to preserve existing endpoint behavior. |
| Document list filters and ID routes | Email/name filters have maximum lengths; document IDs must be positive. The service repeats essential ID, size, and caller checks. |

`StudentResponseDto` and `DocumentResponseDto` are output types; their annotations do not validate incoming requests.

## Browser-side validation

The registration form now checks name/email, dynamically adjusts student/teacher age bounds, and makes course/marks or department required according to role. Login and OTP screens check email validity and six numeric OTP digits. The reusable modal forms apply field-level required, format, length, and numeric limits. Document upload checks the positive integer student ID, extension, filename, and 5 MB limit before sending.

These checks improve feedback only; API DTO and service validation remain mandatory.

## File storage and error handling

New documents are stored under `App_Data/Uploads/Documents`, outside the static web root. Requests for the legacy `/uploads/documents` static path are blocked; the service can still retrieve files in the old `wwwroot/Uploads/Documents` location until they are migrated. Expected upload errors map to 400, 403, or 404; unexpected IO/database failures are not converted into success-shaped responses or exposed as arbitrary 400 messages.

Content checks verify common file signatures and DOCX archive entries. This is not antivirus scanning, and it does not guarantee a file is safe to open.

## Persistence migration

`Migrations/AddUniqueUserEmail` changes `Users.Email` to `varchar(254)` and adds a unique index. Application-side duplicate checks provide prompt 409 responses; the unique index is necessary to prevent concurrent duplicate registrations.

**Before applying the migration**, audit existing rows for:

- duplicate email values (including case variations, depending on database collation);
- email values longer than 254 characters.

The migration has not been applied to the configured database. Do not bypass a migration failure by deleting or truncating user data without reviewing it.

Age and marks are validated at the API boundary but are not yet protected by database check constraints. Add check constraints only after confirming the business rules and auditing existing rows.

## Remaining deployment and test work

1. Audit email data, then apply the generated email-index migration through the normal EF workflow.
2. Move legacy files from `wwwroot/Uploads/Documents` into `App_Data/Uploads/Documents`. Retrieval fallback exists during migration, but direct static access is already blocked.
3. Confirm product choices for name/course/department length limits and the age ranges (student 7–99, teacher 18–99).
4. Add automated tests for required/null/blank values, each numeric boundary, role-conditional registration, email conflicts (including concurrent requests), OTP attempt/expiry behavior, invalid pagination, and missing/oversized/mismatched uploads.
5. Consider rate limiting OTP-send/verification endpoints and scanning uploads if required by the deployment threat model.

## How to add or maintain a validation

Put ordinary input constraints on the **request DTO** (for example `[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]`, and `[RegularExpression]`). Use a custom DTO validator such as `IValidatableObject` when rules depend on another submitted value, as with registration role. Keep database-dependent rules (uniqueness, record existence, and permission checks) in controller/service logic, and protect invariants that must hold across concurrent or non-API writes with database constraints. Keep browser checks synchronized, but never rely on them as the security or correctness boundary.

# Implementation Plan: Per-Student Rate & Per-Session Rate Override

## Overview

This plan implements the rate override feature on top of the existing tutoring booking system. It adds an optional `HourlyRate` to the Student entity, an optional `RateOverride` to the Session entity, a new `UpdateSessionRateCommand`, modifies `BookSessionCommand` to accept an optional rate override, updates invoice generation to use the Effective_Rate priority chain (Session RateOverride > Student HourlyRate > Tutor HourlyRate), updates UI pages, creates an EF Core migration, and updates FsCheck generators.

## Tasks

- [ ] 1. Add HourlyRate to Student entity and update commands
  - [ ] 1.1 Add optional HourlyRate property to Student entity
    - Add `decimal? HourlyRate` property to `src/BatbyEducation.Domain/Entities/Student.cs`
    - Implement `SetHourlyRate(decimal? rate)` method returning `Result` — rejects if rate is non-null and outside £0.01–£999.99
    - Update `Student.Create()` factory to accept optional `decimal? hourlyRate` parameter and validate range
    - Update `Student.Update()` to accept optional `decimal? hourlyRate` parameter, validate range, and record audit entry on change
    - _Requirements: 1.1, 1.7, 1.8, 1.9_

  - [ ] 1.2 Update RegisterStudentCommand to accept optional hourly rate
    - Add `decimal? HourlyRate` parameter to `RegisterStudentCommand` record in `src/BatbyEducation.Application/Commands/Students/RegisterStudentCommand.cs`
    - Update `RegisterStudentCommandHandler` to pass hourly rate to `Student.Create()`
    - Update `RegisterStudentCommandValidator` to validate hourly rate range (£0.01–£999.99) when provided
    - _Requirements: 1.1, 1.7, 1.9_

  - [ ] 1.3 Update UpdateStudentCommand to accept optional hourly rate
    - Add `decimal? HourlyRate` parameter to `UpdateStudentCommand` record in `src/BatbyEducation.Application/Commands/Students/UpdateStudentCommand.cs`
    - Update `UpdateStudentCommandHandler` to pass hourly rate to `Student.Update()`
    - Update `UpdateStudentCommandValidator` to validate hourly rate range when provided
    - _Requirements: 1.8, 1.9_

- [ ] 2. Add RateOverride to Session entity and update booking
  - [ ] 2.1 Add optional RateOverride property to Session entity
    - Add `decimal? RateOverride` property to `src/BatbyEducation.Domain/Entities/Session.cs`
    - Update `Session.Create()` factory to accept optional `decimal? rateOverride` parameter and validate range (£0.01–£999.99) when provided
    - Implement `SetRateOverride(decimal? rate)` method returning `Result` — succeeds only when Status is Scheduled; rejects if rate is non-null and outside £0.01–£999.99
    - _Requirements: 3.1, 3.8, 3.9, 3.10_

  - [ ] 2.2 Update BookSessionCommand to accept optional rate override
    - Add `decimal? RateOverride = null` parameter to `BookSessionCommand` record in `src/BatbyEducation.Application/Commands/Sessions/BookSessionCommand.cs`
    - Update `BookSessionCommandHandler` to pass rate override to `Session.Create()`
    - Update `BookSessionCommandValidator` to validate rate override range (£0.01–£999.99) when provided
    - _Requirements: 3.1, 3.10_

  - [ ] 2.3 Create UpdateSessionRateCommand
    - Create `UpdateSessionRateCommand` record with `Guid SessionId` and `decimal? RateOverride` in `src/BatbyEducation.Application/Commands/Sessions/`
    - Create `UpdateSessionRateCommandHandler` that loads the session, calls `SetRateOverride()`, and saves
    - Create `UpdateSessionRateCommandValidator` that validates rate range (£0.01–£999.99) when non-null
    - _Requirements: 3.8, 3.9, 3.10_

- [ ] 3. Update invoice generation to use Effective_Rate priority chain
  - [ ] 3.1 Modify GenerateInvoiceCommandHandler to resolve Effective_Rate
    - Update `src/BatbyEducation.Application/Commands/Invoices/GenerateInvoiceCommandHandler.cs`
    - For each completed session line item, determine rate using priority: `session.RateOverride ?? student.HourlyRate ?? tutor.HourlyRate`
    - Ensure student is loaded once and its `HourlyRate` is available for all line items
    - Update line item creation to pass the resolved Effective_Rate
    - _Requirements: 6.2, 6.3_

- [ ] 4. Checkpoint - Ensure domain and application changes compile and existing tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 5. EF Core migration for new columns
  - [ ] 5.1 Update entity configuration and create migration
    - Add `HourlyRate` column configuration to Student entity type configuration (nullable decimal)
    - Add `RateOverride` column configuration to Session entity type configuration (nullable decimal)
    - Generate EF Core migration: `dotnet ef migrations add AddRateOverrideColumns --project src/BatbyEducation.Infrastructure --startup-project src/BatbyEducation.Web`
    - Verify migration creates correct nullable decimal columns
    - _Requirements: 1.1, 3.1_

- [ ] 6. Update Blazor UI pages
  - [ ] 6.1 Add hourly rate field to Student registration page
    - Update `src/BatbyEducation.Web/Components/Pages/Students/StudentRegister.razor` to include optional "Hourly Rate (£)" input field
    - Add validation message for out-of-range values
    - Pass the value through to `RegisterStudentCommand`
    - _Requirements: 1.7, 1.9_

  - [ ] 6.2 Add hourly rate field to Student edit page
    - Update `src/BatbyEducation.Web/Components/Pages/Students/StudentEdit.razor` to include optional "Hourly Rate (£)" input field
    - Display current rate (or "Not set") and allow clearing
    - Pass the value through to `UpdateStudentCommand`
    - _Requirements: 1.8, 1.9_

  - [ ] 6.3 Add rate override field to Session booking page
    - Update `src/BatbyEducation.Web/Components/Pages/Sessions/SessionBook.razor` to include optional "Rate Override (£)" input field
    - Add validation message for out-of-range values
    - Pass the value through to `BookSessionCommand`
    - _Requirements: 3.1, 3.10_

  - [ ] 6.4 Add rate override editing to Session list page
    - Update `src/BatbyEducation.Web/Components/Pages/Sessions/SessionList.razor` to show current rate override per session
    - Add an inline edit button/modal for Scheduled sessions to set or clear the rate override via `UpdateSessionRateCommand`
    - Disable editing for non-Scheduled sessions
    - _Requirements: 3.8, 3.9_

- [ ] 7. Checkpoint - Ensure UI compiles and application runs
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Update FsCheck generators and write property tests
  - [ ] 8.1 Update DomainGenerators with rate-related generators
    - Add `OptionalHourlyRate()` generator producing `decimal?` (null or £0.01–£999.99) in `tests/BatbyEducation.Domain.Tests/Generators/DomainGenerators.cs`
    - Add `InvalidHourlyRate()` generator producing decimals outside [0.01, 999.99] (e.g., 0, -1, 1000+)
    - Update existing Student/Session generators to include the new rate fields
    - _Requirements: All (testing infrastructure)_

  - [ ]* 8.2 Write property test for session rate override state machine
    - **Property 28: Session rate override state machine**
    - For any session in Scheduled status, SetRateOverride with valid rate or null succeeds; for any non-Scheduled session, SetRateOverride is rejected
    - **Validates: Requirements 3.8, 3.9**

  - [ ]* 8.3 Write property test for Effective_Rate priority chain
    - **Property 29: Effective_Rate priority chain**
    - For any combination of Session.RateOverride, Student.HourlyRate, and Tutor.HourlyRate values, the invoice line item rate equals Session.RateOverride if set, else Student.HourlyRate if set, else Tutor.HourlyRate
    - **Validates: Requirements 6.2, 6.3**

  - [ ]* 8.4 Write property test for hourly rate range validation
    - **Property 30: Hourly rate range validation**
    - For any rate value outside [0.01, 999.99], Student.SetHourlyRate and Session.SetRateOverride reject it; for any value within range, they accept it
    - **Validates: Requirements 1.9, 3.10**

- [ ] 9. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- This is a delta implementation on top of the existing completed base system
- The Effective_Rate priority chain is: Session RateOverride > Student HourlyRate > Tutor HourlyRate
- Rate validation range: £0.01–£999.99 (nullable — null means "not set")
- `SetRateOverride` on Session only works when session Status is Scheduled
- The EF Core migration adds two nullable decimal columns — no data migration needed
- Property tests 28, 29, 30 correspond to the design document's correctness properties

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "2.1"] },
    { "id": 1, "tasks": ["1.2", "1.3", "2.2", "2.3"] },
    { "id": 2, "tasks": ["3.1"] },
    { "id": 3, "tasks": ["5.1"] },
    { "id": 4, "tasks": ["6.1", "6.2", "6.3", "6.4"] },
    { "id": 5, "tasks": ["8.1"] },
    { "id": 6, "tasks": ["8.2", "8.3", "8.4"] }
  ]
}
```

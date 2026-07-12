# Requirements Document

## Introduction

This document defines the requirements for a booking and accounting system for a small tutoring business. The system enables tutors and administrators to schedule tutoring sessions, manage student information, track payments, generate invoices, and maintain financial records. The goal is to streamline daily operations by combining session scheduling with financial tracking in a single integrated system.

## Glossary

- **Booking_System**: The subsystem responsible for creating, modifying, and cancelling tutoring session bookings
- **Accounting_System**: The subsystem responsible for tracking payments, generating invoices, and maintaining financial records
- **Session**: A scheduled tutoring appointment with a defined date, time, duration, tutor, and student
- **Student**: A person who receives tutoring services and is registered in the system
- **Tutor**: A person who provides tutoring services and has availability configured in the system
- **Invoice**: A financial document issued to a student or their guardian detailing charges for tutoring sessions
- **Payment**: A monetary transaction recorded against an invoice or student account, classified as either Cash or Bank Transfer
- **Cash_Payment**: A payment made in person during a tutoring session
- **Bank_Transfer_Payment**: A payment made via bank transfer, required at least 24 hours before the session start time
- **Ledger**: A categorised financial record that tracks transactions; the system maintains separate ledgers for cash and bank transfer payments
- **Tax_Year**: The UK tax year running from 6 April to 5 April of the following year
- **Rate**: The hourly or per-session charge for tutoring services, which may vary by tutor or subject
- **Student_Hourly_Rate**: An optional hourly rate (nullable decimal, £0.01–£999.99) stored on the student record. When set, this rate takes precedence over the tutor's default hourly rate for all sessions involving that student
- **Session_Rate_Override**: An optional hourly rate (nullable decimal, £0.01–£999.99) stored on an individual session record. When set, this rate takes precedence over both the Student_Hourly_Rate and the tutor's hourly rate for that session
- **Effective_Rate**: The rate used for invoice line item calculation, determined by the priority chain: Session_Rate_Override > Student_Hourly_Rate > Tutor HourlyRate
- **Availability**: The time slots during which a tutor is available to conduct sessions
- **Cancellation**: The act of removing a previously scheduled session, subject to cancellation policies
- **Guardian**: A parent or responsible adult associated with a student account for billing purposes

## Requirements

### Requirement 1: Student Registration

**User Story:** As an administrator, I want to register students in the system, so that I can manage their bookings and billing information.

#### Acceptance Criteria

1. THE Booking_System SHALL store student records containing name (maximum 100 characters), contact email, phone number, associated guardian information comprising guardian name (maximum 100 characters) and guardian contact email, and an optional hourly rate (nullable decimal, £0.01–£999.99)
2. WHEN a new student is registered, THE Booking_System SHALL assign a unique identifier to the student record
3. WHEN a student record is updated, THE Booking_System SHALL retain the previous values in an audit history including the timestamp of the change and the field that was modified
4. IF a student registration is submitted with a duplicate email address, THEN THE Booking_System SHALL reject the registration and display an error indicating the email is already in use
5. IF a student registration is submitted with any required field missing or empty, THEN THE Booking_System SHALL reject the registration and indicate which fields are required
6. IF a student registration is submitted with an email address that does not conform to a valid email format, THEN THE Booking_System SHALL reject the registration and indicate the email format is invalid
7. WHEN a student is registered with an hourly rate provided, THE Booking_System SHALL store the Student_Hourly_Rate on the student record
8. WHEN a student record is updated to set or change the hourly rate, THE Booking_System SHALL record the rate change in the audit history and update the Student_Hourly_Rate
9. IF a student registration or update is submitted with an hourly rate outside the range £0.01 to £999.99, THEN THE Booking_System SHALL reject the request and indicate the allowed rate range

### Requirement 2: Tutor Management

**User Story:** As an administrator, I want to manage tutor profiles and their availability, so that sessions can be scheduled during appropriate times.

#### Acceptance Criteria

1. THE Booking_System SHALL store tutor records containing name (maximum 100 characters), contact email, subjects taught (1 to 20 subjects), and hourly rate (£0.01 to £999.99)
2. WHEN a new tutor is registered, THE Booking_System SHALL assign a unique identifier to the tutor record
3. WHEN a tutor sets their availability, THE Booking_System SHALL store recurring weekly time slots (each defined by day of week, start time, and end time with a minimum duration of 30 minutes) and one-off availability exceptions
4. WHEN a tutor marks a time slot as unavailable, THE Booking_System SHALL prevent new sessions from being booked during that slot
5. IF a tutor's availability is removed for a slot that has an existing booking, THEN THE Booking_System SHALL update the affected session status to "Requires Rescheduling" and include it in the administrator's notification list
6. IF a tutor registration is submitted with a duplicate email address, THEN THE Booking_System SHALL reject the registration and display an error indicating the email is already in use

### Requirement 3: Session Booking

**User Story:** As an administrator, I want to book tutoring sessions for students, so that lessons are scheduled and both parties are informed.

#### Acceptance Criteria

1. WHEN a session is booked, THE Booking_System SHALL record the tutor, student, subject, date, start time, duration (minimum 15 minutes, maximum 4 hours), and an optional Session_Rate_Override (nullable decimal, £0.01–£999.99)
2. WHEN a session is booked, THE Booking_System SHALL verify the tutor is available during the requested time slot (start time through start time plus duration)
3. IF a booking is requested for a time slot where the tutor already has a session, THEN THE Booking_System SHALL reject the booking and indicate the conflicting session date and time
4. WHEN a session is successfully booked, THE Booking_System SHALL assign the session a unique identifier and set its status to "Scheduled"
5. IF a booking is requested outside the tutor's configured availability, THEN THE Booking_System SHALL reject the booking and display the tutor's next available slots for the requested date
6. IF a booking is requested with a subject not in the tutor's configured subjects list, THEN THE Booking_System SHALL reject the booking and display the subjects the tutor teaches
7. IF a booking is requested for a time slot where the student already has a session, THEN THE Booking_System SHALL reject the booking and indicate the student's conflicting session
8. WHILE a session is in "Scheduled" status, THE Booking_System SHALL allow the Session_Rate_Override to be set or changed
9. IF a Session_Rate_Override is set or changed for a session that is not in "Scheduled" status, THEN THE Booking_System SHALL reject the update and indicate that rate overrides can only be modified for scheduled sessions
10. IF a Session_Rate_Override is submitted with a value outside the range £0.01 to £999.99, THEN THE Booking_System SHALL reject the request and indicate the allowed rate range

### Requirement 4: Session Cancellation and Rescheduling

**User Story:** As an administrator, I want to cancel or reschedule sessions, so that changes can be accommodated while maintaining records.

#### Acceptance Criteria

1. WHEN a session with status "Scheduled" is cancelled, THE Booking_System SHALL update the session status to "Cancelled" and retain the original session record including the cancellation timestamp
2. WHEN a session is cancelled fewer than 24 hours before the scheduled start time, THE Booking_System SHALL mark the session as a "Late Cancellation" and retain the session for billing purposes
3. WHEN a session is rescheduled, THE Booking_System SHALL create a new session record linked to the original via a reference ID and update the original session status to "Rescheduled"
4. IF a reschedule is requested for a time slot that is not available (tutor has a conflict or is outside availability), THEN THE Booking_System SHALL reject the reschedule and indicate the conflict
5. IF a cancellation is requested for a session that is not in "Scheduled" status, THEN THE Booking_System SHALL reject the cancellation and display an error indicating only scheduled sessions can be cancelled

### Requirement 5: Session Completion Tracking

**User Story:** As a tutor, I want to mark sessions as completed, so that they can be accurately invoiced.

#### Acceptance Criteria

1. WHEN a tutor marks a session as completed, THE Booking_System SHALL verify the session status is "Scheduled" or "Pending Confirmation", update the session status to "Completed", and record the completion timestamp
2. WHEN a session's scheduled end time (start time plus scheduled duration) has passed without the session being marked as completed or cancelled, THE Booking_System SHALL flag the session as "Pending Confirmation"
3. IF a session is marked as completed with an actual duration different from the originally scheduled duration, THEN THE Booking_System SHALL record the actual duration for billing purposes, provided the actual duration is between 15 minutes and 4 hours inclusive
4. IF a tutor attempts to mark a session as completed that is in "Cancelled" or "Rescheduled" status, THEN THE Booking_System SHALL reject the request and display an error indicating the session is not eligible for completion
5. IF a session is marked as completed with an actual duration outside the range of 15 minutes to 4 hours, THEN THE Booking_System SHALL reject the completion and display an error indicating the allowed duration range

### Requirement 6: Invoice Generation

**User Story:** As an administrator, I want to generate invoices for completed sessions, so that students or guardians can be billed accurately.

#### Acceptance Criteria

1. WHEN an invoice is generated for a specified student and a specified billing period defined by a start date and end date, THE Accounting_System SHALL include all sessions with status "Completed" or "Late Cancellation" for that student where the session date falls within the billing period (inclusive)
2. THE Accounting_System SHALL determine the Effective_Rate for each line item using the following priority chain: if the session has a Session_Rate_Override set, use that rate; otherwise if the student has a Student_Hourly_Rate set, use that rate; otherwise use the tutor's hourly rate as recorded at the time of the session
3. THE Accounting_System SHALL calculate each line item amount by multiplying the session's actual duration in hours by the Effective_Rate for that session
4. WHEN an invoice is generated, THE Accounting_System SHALL assign a unique invoice number and set the status to "Issued"
5. THE Accounting_System SHALL include the student name, guardian name, billing period start and end dates, and total amount on each invoice, where each line item contains the session date, tutor name, subject, duration, Effective_Rate used, and calculated amount
6. WHEN a late-cancelled session is included in an invoice, THE Accounting_System SHALL apply the configured late cancellation fee as the line item amount in place of the duration-based calculation
7. IF an invoice generation is requested and no completed or late-cancelled sessions exist for the specified student within the billing period, THEN THE Accounting_System SHALL not create an invoice and SHALL indicate that no billable sessions were found for the specified period

### Requirement 7: Payment Recording

**User Story:** As an administrator, I want to record payments received and classify them by method, so that I can track settlements and maintain separate ledgers for cash and bank transfer income.

#### Acceptance Criteria

1. WHEN a payment is recorded, THE Accounting_System SHALL associate the payment with a specific invoice and record the amount (minimum £0.01), date, and payment method (Cash or Bank Transfer)
2. WHEN a Cash_Payment is recorded, THE Accounting_System SHALL record the payment against the session date on which the cash was collected
3. WHEN a Bank_Transfer_Payment is recorded and the payment was received at least 24 hours before the associated session start time, THE Accounting_System SHALL accept the payment and associate it with the invoice
4. IF a Bank_Transfer_Payment has not been received 24 hours before the session start time, THEN THE Accounting_System SHALL flag the session as "Payment Not Received" for follow-up
5. WHEN the total payments for an invoice equal or exceed the invoice amount, THE Accounting_System SHALL update the invoice status to "Paid"
6. WHEN a payment is recorded and the payment amount is less than the outstanding balance on the invoice, THE Accounting_System SHALL update the invoice status to "Partially Paid" and display the remaining balance
7. IF a payment amount exceeds the outstanding balance on an invoice, THEN THE Accounting_System SHALL record the overpayment as a credit on the student account
8. IF a payment is submitted with an amount of zero or less, THEN THE Accounting_System SHALL reject the payment and display an error indicating the amount must be greater than zero
9. IF a payment is submitted against an invoice with a status of "Paid" or "Cancelled", THEN THE Accounting_System SHALL reject the payment and display an error indicating the invoice is not eligible for payment

### Requirement 8: Separate Ledgers

**User Story:** As an administrator, I want cash and bank transfer payments tracked in separate ledgers, so that I can reconcile each income stream independently.

#### Acceptance Criteria

1. THE Accounting_System SHALL maintain a Cash Ledger that records all Cash_Payment transactions including payment date, amount, invoice reference, and student name
2. THE Accounting_System SHALL maintain a Bank Transfer Ledger that records all Bank_Transfer_Payment transactions including payment date, amount, invoice reference, and student name
3. WHEN a payment is recorded, THE Accounting_System SHALL automatically post the transaction to the appropriate ledger based on the payment method
4. WHEN a ledger summary is requested for a specified date range, THE Accounting_System SHALL display total receipts, number of transactions, and a chronological list of entries for that date range
5. IF a ledger summary is requested and no transactions exist within the specified date range, THEN THE Accounting_System SHALL display the summary with totals as zero and an empty transaction list

### Requirement 9: Outstanding Balance Tracking

**User Story:** As an administrator, I want to view outstanding balances per student, so that I can follow up on overdue payments.

#### Acceptance Criteria

1. THE Accounting_System SHALL calculate each student's outstanding balance as the sum of all unpaid and partially paid invoice amounts minus any account credits
2. WHEN an invoice with status "Issued" or "Partially Paid" remains without full payment 30 days past the issue date, THE Accounting_System SHALL flag the invoice as "Overdue"
3. WHEN an overdue invoice list is requested, THE Accounting_System SHALL display all overdue invoices sorted by the number of days past due in descending order, including student name, guardian name, invoice number, amount outstanding, and days overdue
4. WHEN a student balance summary is requested, THE Accounting_System SHALL display each student's name, total outstanding balance, number of unpaid invoices, and any account credit balance

### Requirement 10: Payment Not Received Report

**User Story:** As an administrator, I want a report showing sessions where bank transfer payment has not been received on time, so that I can follow up with students or guardians before the session.

#### Acceptance Criteria

1. WHEN a payment-not-received report is requested, THE Accounting_System SHALL list all sessions flagged as "Payment Not Received" where the session date has not yet passed, sorted by session date ascending
2. THE Accounting_System SHALL include the student name, guardian name, session date, expected amount (the outstanding unpaid amount for the session), and number of days since the payment deadline (24 hours before session) on each report entry
3. WHEN a flagged session subsequently receives full payment equal to or exceeding the expected amount, THE Accounting_System SHALL remove the session from the payment-not-received report
4. WHEN a flagged session's date has passed without payment being received, THE Accounting_System SHALL move the session to a "Past Due Sessions" section of the report

### Requirement 11: Financial Reporting

**User Story:** As an administrator, I want to view financial summaries, so that I can understand the business's income and outstanding receivables.

#### Acceptance Criteria

1. WHEN a revenue report is requested for a date range, THE Accounting_System SHALL display total invoiced amount (summing invoices whose issue date falls within the range), total payments received (summing payments whose payment date falls within the range), and total outstanding balance (sum of all unpaid and partially paid invoice amounts minus account credits for invoices issued within the range)
2. WHEN a tutor earnings report is requested for a specified date range, THE Accounting_System SHALL display, for each tutor, the total number of completed session hours and total invoiced amount calculated from completed sessions whose session date falls within the range
3. WHEN a monthly summary view is requested for a specified tax year, THE Accounting_System SHALL display invoiced revenue, collected revenue, and outstanding amounts for each of the 12 months within that tax year (6 April to 5 April)
4. WHEN a revenue report is requested, THE Accounting_System SHALL break down total payments received into cash receipts and bank transfer receipts, consistent with the Cash Ledger and Bank Transfer Ledger totals for the same date range

### Requirement 12: Tax Year Summary

**User Story:** As an administrator, I want to generate a financial summary for the tax year (6 April to 5 April), so that I can prepare accurate tax returns.

#### Acceptance Criteria

1. WHEN a tax year summary is requested for a specified tax year (identified by start year), THE Accounting_System SHALL calculate total income received as the sum of all payments recorded with a payment date between 6 April of the start year and 5 April of the following year (inclusive)
2. WHEN a tax year summary is generated, THE Accounting_System SHALL break down the total income received into Cash Ledger total and Bank Transfer Ledger total
3. WHEN a tax year summary is generated, THE Accounting_System SHALL display total sessions with status "Completed" within the tax year, total hours tutored (sum of actual durations of those sessions), and total invoiced amount for invoices issued within the tax year
4. WHEN a tax year summary is generated, THE Accounting_System SHALL display outstanding receivables as of the tax year end date (5 April), calculated as the sum of all unpaid and partially paid invoice amounts minus any account credits at that date
5. WHEN a tax year summary is generated, THE Accounting_System SHALL provide a month-by-month breakdown of income received across the 12 tax year months (each month running from the 6th of one calendar month to the 5th of the next)
6. IF a tax year summary is requested and no payment or invoice records exist for the specified tax year, THEN THE Accounting_System SHALL display the summary with all totals as zero

### Requirement 13: Schedule Calendar View

**User Story:** As an administrator or tutor, I want to view a calendar of upcoming sessions, so that I can plan my schedule effectively.

#### Acceptance Criteria

1. WHEN a calendar view is requested without a date range, THE Booking_System SHALL default to showing the current week (Monday to Sunday) and display all sessions for that range grouped by day
2. WHEN a calendar view is requested with a specified date range, THE Booking_System SHALL display all sessions within that range grouped by day, showing for each session: tutor name, student name, subject, start time, duration, and status
3. WHEN a calendar is filtered by tutor, THE Booking_System SHALL display only sessions assigned to the selected tutor
4. WHEN a calendar is filtered by student, THE Booking_System SHALL display only sessions for the selected student
5. THE Booking_System SHALL assign a distinct colour code to each session status: Scheduled (blue), Completed (green), Cancelled (grey), Pending Confirmation (amber), and Requires Rescheduling (red)
6. IF no sessions exist for the requested date range and filters, THEN THE Booking_System SHALL display an empty calendar with a message indicating no sessions are scheduled

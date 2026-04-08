# FutureMe Backend

The backend for **FutureMe**, a small full-stack Azure project that lets users schedule emails to their future selves.

This backend is built with **Azure Functions** and handles:
- receiving scheduled email requests through an HTTP API
- storing scheduled emails in **Azure Table Storage**
- processing pending emails on a timer
- sending emails through **Azure Communication Services Email**

## Overview

FutureMe is split into two parts:

- **Frontend**: React app for entering and scheduling emails
- **Backend**: Azure Functions app for storing, processing, and sending those emails

This repository contains the **backend only**.

## Features

- HTTP-triggered Azure Function for scheduling new emails
- Timer-triggered Azure Function for processing pending emails
- Azure Table Storage persistence
- Azure Communication Services Email integration
- Status tracking for scheduled emails
- Error handling for failed sends

## Tech stack

- **C#**
- **Azure Functions (.NET isolated)**
- **Azure Table Storage**
- **Azure Communication Services Email**

## Architecture

```text
Frontend (React)
    ↓
HTTP-triggered Azure Function (ScheduleEmail)
    ↓
Azure Table Storage (ScheduledEmails)
    ↓
Timer-triggered Azure Function (SendScheduledEmails)
    ↓
Azure Communication Services Email
```

## How it works
### 1. Schedule email

The frontend sends a POST request to the ScheduleEmail endpoint with:

- recipient email
- subject
- body
- scheduled send date/time in UTC

### 2. Store in Table Storage

The backend validates the request and stores it as a new ScheduledEmailEntity in Azure Table Storage with status Pending.

### 3. Process pending emails

A timer-triggered function checks for emails that:

- are still marked Pending
- are due to be sent

### 4. Send email

When an email is due, the backend sends it through Azure Communication Services Email.

### 5. Update status

After processing:

- Sent if successful
- Failed if an error occurs

## Partitioning strategy

The PartitionKey is based on the scheduled send date in UTC:

yyyyMMdd

Example:

20260408

This makes the storage layout better aligned with the timer-based processing workflow, since emails are grouped by the day they should be sent.

## API Management

This backend can optionally sit behind Azure API Management.

In that setup, API Management can provide:

- rate limiting
- CORS handling
- public API abstraction
- backend protection by forwarding the Function key internally

##  CORS

If the frontend runs locally or on a different domain, the public API layer must allow the frontend origin.

Depending on the setup, that may be:

- Function App CORS when the frontend calls the Function directly
- API Management CORS when the frontend calls APIM instead

## Deployment

This backend can be deployed to an Azure Function App.

Runtime configuration should be provided through Azure app settings, for example:

- AzureWebJobsStorage
- AcsEmailConnectionString
- AcsEmailSender

## Related repository

This repository contains the backend only.
The frontend is implemented separately in React.

## Purpose

This project was built as a portfolio piece to demonstrate:

- Azure cloud development
- serverless backend design
- asynchronous processing with timer functions
- Azure service integration
- full-stack coordination with a separate frontend

## Author

Built by Nick.

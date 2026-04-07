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
# Timeline Validation Guide

## Overview

The TimelineValidationService provides centralized timeline validation logic for checking if actions are allowed based on the current date and configured timelines.

## How to Use

### 1. Service Injection

Make sure to inject `ITimelineValidationService` into your service constructor:

```csharp
private readonly ITimelineValidationService _timelineValidationService;

public YourService(/* other dependencies */, ITimelineValidationService timelineValidationService)
{
    // ... other assignments
    _timelineValidationService = timelineValidationService;
}
```

### 2. Validating Timeline for an Action

To check if an action is allowed during the current time:

```csharp
// Validate timeline for a specific action type
var isValidTime = await _timelineValidationService.IsValidTimeForAction(TimelineTypeEnum.ProjectRegistration);
if (!isValidTime)
{
    throw new ServiceException("Project registration is not currently open. Please check the registration schedule.");
}
```

### 3. Getting Active Timeline

To get the active timeline for a specific action type:

```csharp
// Get active timeline to use its sequence ID or other properties
var activeTimeline = await _timelineValidationService.GetActiveTimeline(TimelineTypeEnum.ProjectRegistration);
if (activeTimeline == null)
{
    throw new ServiceException("No active timeline found for project registration.");
}
int sequenceId = activeTimeline.SequenceId.Value;
```

### 4. Validating with Specific Sequence

To validate with a specific sequence:

```csharp
// Validate timeline for a specific action type and sequence
var isValidTime = await _timelineValidationService.IsValidTimeForAction(
    TimelineTypeEnum.ProjectRegistration,
    sequenceId: 1);
```

## Available Timeline Types

The available timeline types are defined in `TimelineTypeEnum`:

- `ProjectRegistration = 1`: For registering new projects
- `ReviewPeriod = 2`: For reviewing project proposals
- `FundRequest = 3`: For requesting funds
- `FundApproval = 4`: For approving fund requests
- `ConferenceSubmission = 5`: For submitting to conferences
- `PaperReview = 6`: For paper review periods

## Creating New Timeline Types

1. Add a new enum value to `TimelineTypeEnum`
2. Office users can create timelines with the new type using the Timeline Management interface

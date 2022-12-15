# Octopus.TaskTree (A dotnet component)

[![Release to Nuget](https://github.com/gokulegit/Octopus/actions/workflows/publish.yml/badge.svg?branch=1.1.1)](https://github.com/gokulegit/Octopus/actions/workflows/publish.yml) [![NuGet Downloads](https://img.shields.io/nuget/dt/Octopus.TaskTree.svg)](https://www.nuget.org/packages/Octopus.TaskTree) ![Nuget Version](https://img.shields.io/nuget/v/Octopus.TaskTree?label=nuget)

Dotnet component that helps you to manage `async` tasks in a hierarchical fashion.

There are situations that we might have to manage tasks in a tree like structure. Means, the parent task depends on child tasks to be completed in a serial or concurrent fashion. The parent task needs to show the average progress value of overall operation.

This component `ITaskNode` lets you to Create a task, Set a custom asynchronous function `Func<IProgressReporter, CancellationToken, Task>` for it, Create one or more child tasks and get overall progress.

You have several options that you can execute them concurrently or in series. The overall progress will be updated to parent task.

Please see below code snippet for the better understanding.

**Create structured Tasks**
``` C#
ITaskNode rootTask = new TaskNode("root");

ITaskNode childTask_1 = new TaskNode("Task-1");
ITaskNode childTask_2 = new TaskNode("Task-2");

rootTask.AddChild(childTask_1);
rootTask.AddChild(childTask_2);
```

**Set actions**

```C#
childTask_1.SetAction(async (reporter, cancellationToken) => {
    // Simple delay function.
    reporter.ReportProgress(TaskStatus.InProgress, 10, "Started...");
    await Task.Delay(1000);
    reporter.ReportProgress(TaskStatus.InProgress, 100, "Finished...");
});

childTask_2.SetAction(async (reporter, cancellationToken) => {
    // Simple delay function.
    reporter.ReportProgress(TaskStatus.InProgress, 5, "Started...");
    await Task.Delay(2500);
    reporter.ReportProgress(TaskStatus.InProgress, 100, "Finished...");
});

```

**Subscribe to reporting event to get overall progress**

```C#
// Before starting the execution, you need to subscribe for progress report.
rootTask.Reporting += (object sender, ProgressReportingEventArgs eventArgs) => {
    eventArgs.ProgressValue; // -> this will represent the overall progress
};

```

**Start execution**
```C#
// Create and pass the cancellation token
var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;

// Start the execution concurrently
rootTask.ExecuteConcurrently(cancellationToken: token, throwOnError: true);


// OR

// Start the execution in series
rootTask.ExecuteInSeries(cancellationToken: token, throwOnError: true);

```

> _Please note:_ If `rootTask` has action set to it then the action will run after executing all child tasks in case of series execution. However in concurrent execution, `rootTask`'s action will also be executed along with child tasks and gets the overall progress.

## Road map

This is the initial version of the component. Please raise issues if you find any. Comments, suggestions and contributions are always welcome. 

Here's the list of items in backlog for the upcoming releases

- [x] Core Component
- [x] Wpf Sample
- [x] Blazor Server Sample
- [ ] Syntax Enhancement on subtasks creation and settings
- [ ] Json/Xml config loader
- [ ] Extend to Java, Python

Sample tree below:

``` C#
var rootTask =
    new TaskNode("Root", null,
        new TaskNode("Task1", Task1_Action,
            new TaskNode("Task1.1", Task1_1_Action),
            new TaskNode("Task1.2", Task1_2_Action)),
        new TaskNode("Task2", null,
            new TaskNode("Task2.1", null,
                new TaskNode("Task2.1.1", Task2_1_1_Action),
                new TaskNode("Task2.1.2", Task_2_1_2_Action)),
            new TaskNode("Task2.2", Task2_2_Action)),
        new TaskNode("Task3", Task3_Action),
        new TaskNode("Task4", Task4_Action),
        new TaskNode("Task5", Task5_Action),
        new TaskNode("Task6", Task6_Action));


private async Task Task1_Action(IProgressReporter reporter, CancellationToken token)
{
    reporter.ReportProgress(TaskStatus.InProgress, 10, "Started...");
    await Task.Delay(1000);
    reporter.ReportProgress(TaskStatus.InProgress, 40, "InProgress...");
    await Task.Delay(1000);
    reporter.ReportProgress(TaskStatus.Completed, 100, "Done...");
}
```

- [ ] Find a Child and Set Preferences to it.

``` C#
var task2_1_2 = rootTask.Find("Task2/Task2.1/Task2.1.2");

// Preferences
task2_1_2.ExecutionMode = ExecutionMode.Series;
task2_1_2.ThrowOnError = false;

rootTask.ExecuteConcurrently(cancellationToken: token, throwOnError: true);
```

using Octopus.TaskTree;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TaskStatus = Octopus.TaskTree.TaskStatus;

namespace Octopus.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ITaskNode rootTask;

        public MainWindow()
        {
            InitializeComponent();

            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            CreateTasks(ExecutionMode.Series);

        }

        CodeBehavior defaultCodeBehavior = new();

        private void CreateTasks(ExecutionMode PreferredExecMode)
        {

            //rootTask = new TaskNode("TaskNode-1");
            //rootTask.AddChild(new TaskNode("TaskNode-1.1", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 20 }, "1.1")));
            //var subTask = new TaskNode("TaskNode-1.2");
            //subTask.AddChild(new TaskNode("TaskNode-1.2.1", async (reporter, token) => await SimpleTimer(reporter, token,
            //    defaultCodeBehavior with { ShouldPerformAnInDeterminateAction = true, InDeterminateActionDelay = 2000 }, "1.2.1")));

            //var subTask2 = new TaskNode("TaskNode-1.2.2");
            //subTask2.AddChild(new TaskNode("TaskNode-1.2.2.1", async (reporter, token) => await SimpleTimer(reporter, token,
            //    defaultCodeBehavior with { ShouldThrowExceptionDuringProgress = false, IntervalDelay = 65 }, "1.2.2.1")));

            //subTask.AddChild(subTask2);
            //subTask.AddChild(new TaskNode("TaskNode-1.2.3", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 60 }, "1.2.3")));
            //subTask.AddChild(new TaskNode("TaskNode-1.2.4", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 30 }, "1.2.4")));

            //rootTask.AddChild(subTask);
            //rootTask.AddChild(new TaskNode("TaskNode-1.3", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 160 }, "1.3")));
            //rootTask.AddChild(new TaskNode("TaskNode-1.4", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 50 }, "1.4")));
            //rootTask.AddChild(new TaskNode("TaskNode-1.5", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 20 }, "1.5")));
            //rootTask.AddChild(new TaskNode("TaskNode-1.6", async (reporter, token) => await SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 250 }, "1.6")));

            var task1_2_2 = new TaskNode("TaskNode-1.2.2", null, PreferredExecMode,
                           new TaskNode("TaskNode-1.2.2.1", Task1_2_2_1Action, PreferredExecMode));

            var task1_2 = new TaskNode("TaskNode-1.2", null, PreferredExecMode,
                              new TaskNode("TaskNode-1.2.1", Task1_2_1Action, PreferredExecMode),
                              task1_2_2,
                              new TaskNode("TaskNode-1.2.3", Task1_2_3Action, PreferredExecMode),
                              new TaskNode("TaskNode-1.2.4", Task1_2_4Action, PreferredExecMode));

            rootTask = new TaskNode("TaskNode-1", null, PreferredExecMode,
                new TaskNode("TaskNode-1.1", Task1_1Action, PreferredExecMode),
                task1_2,
                new TaskNode("TaskNode-1.3", Task1_3Action, PreferredExecMode),
                new TaskNode("TaskNode-1.4", Task1_4Action, PreferredExecMode),
                new TaskNode("TaskNode-1.5", Task1_5Action, PreferredExecMode),
                new TaskNode("TaskNode-1.6", Task1_6Action, PreferredExecMode)
                );

            rootTask.Reporting += (sender, eArgs) =>
            {
                if (eArgs.TaskStatus == TaskStatus.InDeterminate)
                {
                    pb.IsIndeterminate = true;
                }
                else
                {
                    pb.IsIndeterminate = false;
                    pb.Value = eArgs.ProgressValue;
                }
            };

            tv.ItemsSource = new ObservableCollection<TaskNodeViewModel> { new TaskNodeViewModel(rootTask) };
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            txtError.Text = e.Exception.Message;
            errorBox.Visibility = Visibility.Visible;
            CreateTasks(ExecutionMode.Series);
            e.Handled = true;
        }

        private async Task SimpleTimer(IProgressReporter progressReporter, CancellationToken token, CodeBehavior behaviors = null, string progressMessage = null)
        {
            behaviors ??= new CodeBehavior();
            progressMessage ??= "In progress ";
            progressReporter.Report(TaskStatus.InProgress, 0, $"{progressMessage}: 0%");
            bool error = false;

            if (behaviors.ShouldThrowException)
            {
                throw new Exception();
            }

            try
            {
                if (behaviors.ShouldPerformAnInDeterminateAction)
                {
                    progressReporter.Report(TaskStatus.InDeterminate, 0, $"{progressMessage}: 0%");
                    if (behaviors.ShouldHangInAnInDeterminateState)
                    {
                        await Task.Delay(-1);
                    }
                    await Task.Delay(behaviors.InDeterminateActionDelay);
                }
                else
                {
                    foreach (int i in Enumerable.Range(1, 100))
                    {
                        if (behaviors.ShouldThrowExceptionDuringProgress)
                        {
                            throw new Exception();
                        }
                        if (token.IsCancellationRequested)
                        {
                            progressReporter.Report(TaskStatus.Cancelled, i);
                            break;
                        }
                        await Task.Delay(behaviors.IntervalDelay);
                        progressReporter.Report(TaskStatus.InProgress, i, $"{progressMessage}: {i}%");

                        if (i > 20 && behaviors.ShouldHangDuringProgress)
                        {
                            await Task.Delay(-1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = true;
                progressReporter.Report(TaskStatus.Failed, 0, ex);
                throw;
            }
            if (!error && !token.IsCancellationRequested)
            {
                progressReporter.Report(TaskStatus.Completed, 100, $"{progressMessage}: 100%");
            }
        }

        CancellationTokenSource tokenSource;
        private async void StartClick(object sender, RoutedEventArgs e)
        {
            grpExecutionMethod.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnCancel.IsEnabled = true;
            try
            {
                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                CreateTasks(rdConcurrent.IsChecked.Value ? ExecutionMode.Concurrent : ExecutionMode.Series);
                await rootTask.Execute(cancellationToken: token, throwOnError: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            btnStart.IsEnabled = true;
            btnCancel.IsEnabled = false;
            grpExecutionMethod.IsEnabled = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            tokenSource?.Cancel();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                errorBox.Visibility = Visibility.Collapsed;
                // RESET UI Controls
                btnCancel.IsEnabled = false;
                btnStart.IsEnabled = true;
                pb.Value = 0;
                rdConcurrent.IsChecked = false;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue is TaskNodeViewModel oldTask)
            {
                oldTask.BaseTaskNode.Reporting -= ChildReport;
            }
            if (e.NewValue is TaskNodeViewModel taskVM)
            {
                var task = taskVM.BaseTaskNode;
                txtId.Text = task.Id;
                txtStatus.Text = task.TaskStatus.ToString("G");
                pbChild.Value = task.ProgressValue;
                txtChildState.Text = task.ProgressState + "";
                task.Reporting += ChildReport;
            }
        }

        private void ChildReport(object sender, ProgressReportingEventArgs eventArgs)
        {
            if (sender is ITaskNode task)
            {
                txtId.Text = task.Id;
                txtStatus.Text = task.TaskStatus.ToString("G");
                pbChild.Value = task.ProgressValue;
                txtChildState.Text = task.ProgressState + "";
            }
        }

        private void btnResetClick(object sender, RoutedEventArgs e)
        {
            CreateTasks(ExecutionMode.Series);
            btnCancel.IsEnabled = false;
            btnStart.IsEnabled = true;
            pb.Value = 0;
            rdConcurrent.IsChecked = false;
            rdSeries.IsChecked = true;
        }


        #region Task Actions

        private Task Task1_1Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 20 }, "1.1");

        private Task Task1_2_1Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { ShouldPerformAnInDeterminateAction = true, InDeterminateActionDelay = 2000 }, "1.2.1");

        private Task Task1_2_2_1Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { ShouldThrowExceptionDuringProgress = false, IntervalDelay = 65 }, "1.2.2.1");

        private Task Task1_2_3Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 60 }, "1.2.3");

        private Task Task1_2_4Action(IProgressReporter reporter, CancellationToken token)
            => SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 30 }, "1.2.4");

        private Task Task1_3Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 160 }, "1.3");

        private Task Task1_4Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 50 }, "1.4");

        private Task Task1_5Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 20 }, "1.5");

        private Task Task1_6Action(IProgressReporter reporter, CancellationToken token) =>
            SimpleTimer(reporter, token, defaultCodeBehavior with { IntervalDelay = 250 }, "1.6");

        #endregion
    }
}


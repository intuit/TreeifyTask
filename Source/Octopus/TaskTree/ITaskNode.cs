using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Octopus.TaskTree
{
    public interface ITaskNode : IProgressReporter
    {
        string Id { get; }
        double ProgressValue { get; }
        object ProgressState { get; }
        ITaskNode Parent { get; set; }
        IEnumerable<ITaskNode> ChildTasks { get; }
        TaskStatus TaskStatus { get; }
        ExecutionMode PreferredExecutionMode { get; set; }
        /// <summary>
        /// Starts the execution in series or concurrent fashion. 
        /// If in case of concurrent execution, All tasks including self action will be started asynchronously. In case of exception and throwOnError is set to false, the error task will have the exception details in ProgressState property and other tasks will be continued. In case of exception and throwOnError is set to true, the error task will have the exception details in ProgressState property, other tasks will continue and upon completion of all tasks, an AggregatedException will be thrown to the caller.
        /// 
        /// If in case of series execution, All children tasks will be executed one by one in the order it is constructed. If throwOnError is set to true, then it throws and stops executing further children. If throwOnError is set to false, then it set the exception details to ProgressState and continues to next child task
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        Task Execute(CancellationToken cancellationToken, bool throwOnError);
        /// <summary>
        /// Reset the task status to NotStarted
        /// </summary>
        void ResetStatus();
        /// <summary>
        /// Returns the children task nodes along with self as a FlatList.
        /// </summary>
        /// <returns>List of children nodes containing self</returns>
        IEnumerable<ITaskNode> ToFlatList();
    }
}

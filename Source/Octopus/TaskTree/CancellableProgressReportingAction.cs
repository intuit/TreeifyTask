using System.Threading;
using System.Threading.Tasks;

namespace Octopus.TaskTree
{
    public delegate Task CancellableProgressReportingAction(
        IProgressReporter progressReporter, CancellationToken cancellationToken);
}

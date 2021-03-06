#if NET20 || NET30 || NET35

namespace System.Threading.Tasks
{
    public partial class Task
    {
        public static Task Run(Action action)
        {
            var result = new Task(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach);
            result.Start();
            return result;
        }

        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            var result = new Task(action, cancellationToken, TaskCreationOptions.DenyChildAttach);
            if (!cancellationToken.IsCancellationRequested)
            {
                result.Start(result.Scheduler, false);
            }
            return result;
        }

        public static Task Run(Func<Task> action)
        {
            // TODO: change to continue with and promise task?
            if (action == null)
            {
                throw new ArgumentNullException();
            }
            var result = new Task
                (
                    () =>
                    {
                        var task = action();
                        task.Start(task.Scheduler, false);
                        task.Wait();
                        if (task.IsFaulted)
                        {
                            throw task.Exception;
                        }
                        if (task.IsCanceled)
                        {
                            throw new TaskCanceledException(task);
                        }
                    },
                    CancellationToken.None,
                    TaskCreationOptions.DenyChildAttach
                );
            result.Start();
            return result;
        }

        public static Task Run(Func<Task> action, CancellationToken cancellationToken)
        {
            // TODO: change to continue with and promise task?
            if (action == null)
            {
                throw new ArgumentNullException();
            }
            var result = new Task
                (
                    () =>
                    {
                        var task = action();
                        task.Start(task.Scheduler, false);
                        task.Wait(cancellationToken);
                        if (task.IsFaulted)
                        {
                            throw task.Exception;
                        }
                        if (task.IsCanceled)
                        {
                            throw new TaskCanceledException(task);
                        }
                    },
                    cancellationToken,
                    TaskCreationOptions.DenyChildAttach
                );
            if (!cancellationToken.IsCancellationRequested)
            {
                result.Start(result.Scheduler, false);
            }
            return result;
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            var result = new Task<TResult>(function, CancellationToken.None, TaskCreationOptions.DenyChildAttach);
            result.Start();
            return result;
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            var result = new Task<TResult>(function, cancellationToken, TaskCreationOptions.DenyChildAttach);
            if (!cancellationToken.IsCancellationRequested)
            {
                result.Start(result.Scheduler, false);
            }
            return result;
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            // TODO: change to continue with and promise task?
            if (function == null)
            {
                throw new ArgumentNullException();
            }
            var result = new Task<TResult>(() => function().Result, CancellationToken.None, TaskCreationOptions.DenyChildAttach);
            result.Start();
            return result;
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            // TODO: change to continue with and promise task?
            if (function == null)
            {
                throw new ArgumentNullException();
            }
            var result = new Task<TResult>(() => function().Result, cancellationToken, TaskCreationOptions.DenyChildAttach);
            if (!cancellationToken.IsCancellationRequested)
            {
                result.Start(result.Scheduler, false);
            }
            return result;
        }
    }
}

#endif
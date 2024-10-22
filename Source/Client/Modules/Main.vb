Namespace Modules
    Module Program
        Sub Main()
            ' Start the asynchronous tasks
            Dim startupTask = Task.Run(Sub() Startup())
            Dim clientTask = Task.Run(Sub() Client.Run())

            ' Wait for both tasks to complete
            Task.WaitAll(startupTask, clientTask)
        End Sub
    End Module
End Namespace

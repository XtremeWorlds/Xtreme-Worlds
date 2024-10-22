Namespace Modules
    Module Program
        Sub Main()
            ' Run asynchronous tasks
            Task.Run(Sub()
                Startup()
            End Sub)

            Task.Run(Sub()
                Client.Run()
            End Sub)
        End Sub
    End Module
End NameSpace
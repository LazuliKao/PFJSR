Imports System
Imports System.Runtime.InteropServices

Namespace PFJSRBDSAPI
    Public Module Ex
        Public ReadOnly Property HasConsole As Boolean
            Get

                Try
                    __ = Console.WindowHeight
                    Return True
                Catch
                    Return False
                End Try
            End Get
        End Property

        Private Const STD_OUTPUT_HANDLE As Integer = -11
        Private Const ENABLE_VIRTUAL_TERMINAL_PROCESSING As UInteger = &H0004
        'private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        'private const uint ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;
        <DllImport("kernel32.dll")>
        Private Function GetConsoleMode(ByVal hConsoleHandle As IntPtr, <Out> ByRef lpMode As UInteger) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Private Function SetConsoleMode(ByVal hConsoleHandle As IntPtr, ByVal dwMode As UInteger) As Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Function GetStdHandle(ByVal nStdHandle As Integer) As IntPtr
        End Function

        <DllImport("kernel32.dll")>
        Public Function GetLastError() As UInteger
        End Function

        Public Sub FixConsole()
            Dim iStdOut = GetStdHandle(STD_OUTPUT_HANDLE)

            If iStdOut = IntPtr.Zero Then
                Return
            End If

            Dim outConsoleMode As UInteger = Nothing

            If GetConsoleMode(iStdOut, outConsoleMode) Then
                If ENABLE_VIRTUAL_TERMINAL_PROCESSING <> (outConsoleMode And ENABLE_VIRTUAL_TERMINAL_PROCESSING) Then
                    outConsoleMode = outConsoleMode Or ENABLE_VIRTUAL_TERMINAL_PROCESSING

                    If SetConsoleMode(iStdOut, outConsoleMode) Then
                        Console.WriteLine("[PF+] Console ENABLE_VIRTUAL_TERMINAL_PROCESSING enabled.")
                    End If
                End If
            End If
        End Sub
    End Module
End Namespace

using System;
using System.IO;
using System.Runtime.InteropServices;

public static class ConsoleHelper
{
    public static void CreateConsole()
    {
        AllocConsole();

        // stdout's handle seems to always be equal to 7
        IntPtr defaultStdout = new IntPtr(7);
        IntPtr currentStdout = GetStdHandle(StdOutputHandle);

        if (currentStdout != defaultStdout)
            // reset stdout
            SetStdHandle(StdOutputHandle, defaultStdout);

        // reopen stdout
        TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = false };
        Console.SetOut(writer);
    }
    public static void CloseConsole()
    {
        FreeConsole();


    }
    // P/Invoke required:
    private const UInt32 StdOutputHandle = 0xFFFFFFF5;
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
    [DllImport("kernel32.dll")]
    private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
    [DllImport("kernel32")]
    static extern bool AllocConsole();
    [DllImport("kernel32")]
    static extern bool FreeConsole();
}
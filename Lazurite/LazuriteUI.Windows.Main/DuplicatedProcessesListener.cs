﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LazuriteUI.Windows.Main
{
    public static class DuplicatedProcessesListener
    {
        public static event Action<Process[]> Found;

        public static void Start()
        {
            var launcherAssembly = typeof(LazuriteUI.Windows.Launcher.App).Assembly;
            var launcherExePath = Lazurite.Windows.Utils.Utils.GetAssemblyPath(launcherAssembly);
            var launcherProcessName = Path.GetFileNameWithoutExtension(launcherExePath);

            var currentProcess = Process.GetCurrentProcess();
            var currentProcessName = currentProcess.ProcessName;
            var currentProcessLocation = currentProcess.StartInfo.FileName;
            var currentProcessId = currentProcess.Id;

            var listenerThread = new Thread(() =>
            {
                while (true)
                {
                    var processes = Process.GetProcesses();
                    var targetProcesses = processes.Where(x =>
                        x.ProcessName == launcherProcessName ||
                        (x.ProcessName == currentProcessName &&
                        x.StartInfo.FileName == currentProcessLocation &&
                        x.Id != currentProcessId))
                        .ToArray();
                    if (targetProcesses.Any())
                    {
                        foreach (var process in targetProcesses)
                            process.Kill();
                        Found?.Invoke(targetProcesses);
                        Thread.Sleep(2000);
                    }
                    else
                        Thread.Sleep(700);
                }
            })
            {
                IsBackground = true
            };
            listenerThread.Start();
        }
    }
}
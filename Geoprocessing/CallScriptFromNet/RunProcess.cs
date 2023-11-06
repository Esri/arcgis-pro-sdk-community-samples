/*

   Copyright 2023 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallScriptFromNet
{
  internal class RunProcess
  {
    private Process _process;
    private StringBuilder _sbOut = new ();
    private StringBuilder _sbError = new ();

    public (string Output, string Error, int ErrCode) RunProcessGrabOutput(string Executable, string Arguments, string WorkingDirectory)
    {
      int exitCode = -1;
      try
      {
        _sbOut.Clear();
        _sbError.Clear();
        _process = new Process();
        _process.StartInfo.FileName = Executable;
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.WorkingDirectory = WorkingDirectory;
        _process.StartInfo.RedirectStandardInput = true;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.StartInfo.RedirectStandardError = true;
        _process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        _process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        _process.StartInfo.CreateNoWindow = true;
        _process.StartInfo.EnvironmentVariables.Add("PYTHONUNBUFFERED", "TRUE");

        if (!string.IsNullOrEmpty(Arguments))
          _process.StartInfo.Arguments = Arguments;

        _process.EnableRaisingEvents = true;
        _process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
        _process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorHandler);
        _process.Start();

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // You can set the priority only AFTER the you started the process.
        _process.PriorityClass = ProcessPriorityClass.BelowNormal;
        _process.WaitForExit();
        exitCode = _process.ExitCode;
      }
      catch
      {
        // This is how we indicate that something went wrong.
        throw;
      }

      return (_sbOut.ToString(), _sbError.ToString(), exitCode);
    }

    private void ProcessOutputHandler(object SendingProcess, DataReceivedEventArgs OutLine)
    {
      _sbOut.AppendLine(OutLine.Data);
    }

    private void ProcessErrorHandler(object SendingProcess, DataReceivedEventArgs OutLine)
    {
      _sbError.AppendLine(OutLine.Data);
    }
  }
}

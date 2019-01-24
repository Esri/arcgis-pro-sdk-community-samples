//Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHostGDB.Common {
    class TaskUtils {
        //http://stackoverflow.com/questions/16720496/set-apartmentstate-on-a-task
        /// <summary>
        /// The problem is that Core.Data must be STA. An STA thread can't be a threadpool thread 
        /// and must pump a message loop
        /// </summary>
        /// <remarks>Note: there is no non-generic equivalent for TaskCompletionSource</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns>Task{T}</returns>
        public static Task<T> StartSTATask<T>(Func<T> func) {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new Thread(() => {
                try {
                    tcs.SetResult(func());
                }
                catch (Exception e) {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}

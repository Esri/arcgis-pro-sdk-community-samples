/*

   Copyright 2025 Esri

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
namespace AddInShared
{
	// Define the event arguments
	public class AddInToAddInEventArgs(string fromAddIn, string message) : System.EventArgs
	{
		/// <summary>
		/// who sent the message
		/// </summary>
		public string FromAddIn { get; set; } = fromAddIn;
		/// <summary>
		/// the message
		/// </summary>
		public string Message { get; set; } = message;
	}

	// Define the delegate for the event handler
	public delegate void AddInToAddInEventHandler(object sender, AddInToAddInEventArgs e);

	public interface IAddInToAddIn
	{
		// Implement the event
		event AddInToAddInEventHandler AddInToAddInEvent;

		// Implement the method to trigger the event
		void OnAddInToAddInEvent(AddInToAddInEventArgs e);
	}
}
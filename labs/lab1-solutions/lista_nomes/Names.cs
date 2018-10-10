using System;
using System.Collections.Generic;

namespace Names
{
	/// <summary>
	/// Name list interface.
	/// </summary>
	public interface INameList {
		/// <summary>
		/// Add a new name into the list.
		/// </summary>
		/// <param name="novoNome">
		/// New name to be added.
		/// </param>
		void Add(string NewName);
		
		/// <summary>
		/// Empties the list.
		/// </summary>
		void Empty();

		/// <summary>
		/// Returns the whole list as a string.
		/// </summary>
		/// <returns>string containing all the list's elements.</returns>
		string toString();
	}

	/// <summary>
	/// Summary description for nomes.
	/// </summary>
	public class NameList : INameList
	{
		/// <summary>
		/// List containing the names.
		/// </summary>
		private List<string> list;

		/// <summary>
		/// NameList default constructor
		/// </summary>
		public NameList() {
			list = new List<string>();
		}

		/// <summary>
		/// Adds a new name to the list.
		/// </summary>
		/// <param name="newName">
		/// New name to be added to the list.
		/// </param>
		public void Add(string newName) {
			list.Add(newName);
		}
		
		/// <summary>
		/// Empties the list
		/// </summary>
		public void Empty() {
			list.Clear();
		}

        /// <summary>
        /// Returns the whole list as a string.
        /// </summary>
        /// <returns>string containing all the list's elements.</returns>
        public string toString() {
			string s = "";
			foreach (string n in list) {
				s += n + "\r\n";
			}
			return s;
		}
	}
}

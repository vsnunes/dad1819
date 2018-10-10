// The "using" keyword allows you to use classes from the used namespace without a full name.
// In this case it allows you to write "Console." instead of "System.Console." .
using System;

// It's important that namespaces and the classes in them have different names.
namespace HelloConsola
{
	/// <summary>
	/// Simple class containing the application's Main entry method.
	/// </summary>
	class CMain
	{
		/// <summary>
		/// Main method, application's entry point
		/// </summary>
		static void Main()
		{
			Console.WriteLine("Hello World! Olá Mundo!");
            Console.ReadLine();
		}
	}
}

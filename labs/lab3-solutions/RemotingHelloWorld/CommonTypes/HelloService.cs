using System;

namespace RemotingHelloWorld
{
	public class HelloService : MarshalByRefObject  {

    public HelloService() : base() { }

    public string Hello() {
      return "Hello World! Olá Mundo!";
		}
  }
}
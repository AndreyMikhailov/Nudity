using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Nudity;
using TestLib;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //var sbExposed1 = new Class1().AsExposed();
            var sbExposed3 = new JsonSerializer().AsExposed();
            Console.WriteLine(sbExposed3._culture);
            sbExposed3._culture = new CultureInfo("en-US");
            new StringBuilder().AsExposed();
        }
    }
}
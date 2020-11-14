using System;
using Crofana.IoC;

namespace Crofana.IoC.Test
{
    class NotCrofanaObject { }

    [CrofanaObject]
    class CrofanaObject1
    {
        [Autowired]
        private CrofanaObject2 co2;
        [Autowired]
        public CrofanaObject3 co3 { get; set; }
        public CrofanaObject2 CO2 => co2;
    }

    [CrofanaObject]
    class CrofanaObject2
    {
        public int x = 10;
    }

    [CrofanaObject]
    class CrofanaObject3
    {
        public int x = 50;
    }

    class Program
    {
        static void Main(string[] args)
        {
            SimpleCrofanaObjectFactory cof = new SimpleCrofanaObjectFactory();

            var co = cof.GetObject<CrofanaObject1>();

            Console.WriteLine(co.CO2.x);
            Console.WriteLine(co.co3.x);
        }
    }
}

using System;

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
        private CrofanaObject3() { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            StandardCrofanaObjectFactory cof = new StandardCrofanaObjectFactory();

            var co = cof.GetObject<CrofanaObject1>();

            Console.WriteLine(co.CO2.x);
            Console.WriteLine(co.co3.x);
        }
    }
}

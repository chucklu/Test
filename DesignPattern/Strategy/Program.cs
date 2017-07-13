﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    class Program
    {
        static void Main(string[] args)
        {
            Context context;

            context = new Context(new ConcreteStrategyA());
            context.Sort();

            context = new Context(new ConcreteStrategyB());
            context.Sort();

            Console.ReadKey();
        }
    }
}
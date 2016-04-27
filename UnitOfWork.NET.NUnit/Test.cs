using NUnit.Framework;
using System;
using System.Diagnostics;
using UnitOfWork.NET.NUnit.Classes;
using UnitOfWork.NET.NUnit.Repositories;

namespace UnitOfWork.NET.NUnit
{
    [TestFixture]
    public class UnitOfWorkTest
    {
        [Test]
        public void TestSingleRepository()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var uow = new TestUnitOfWork())
            {
                var numbers = uow.Repository<IntValue>().All();

                stopwatch.Stop();

                Console.WriteLine(stopwatch.ElapsedMilliseconds);

                foreach (var number in numbers)
                    Console.WriteLine(number.Value);
            }
        }

        [Test]
        public void TestDoubleRepository()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var uow = new TestUnitOfWork())
            {
                var numbers = uow.Repository<DoubleValue, FloatValue>().AllBuilt();
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                foreach (var number in numbers)
                    Console.WriteLine(number.Value);
            }
        }

        [Test]
        public void TestCustomRepository()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var uow = new TestUnitOfWork())
            {
                var numbers = uow.CustomRepository<DoubleRepository>().NewList();
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                foreach (var number in numbers)
                    Console.WriteLine(number.Value);
            }
        }

        [Test]
        public void TestCustomUnitOfWork()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var uow = new TestUnitOfWork())
            {
                var numbers = uow.Doubles.NewList();
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                foreach (var number in numbers)
                    Console.WriteLine(number.Value);
            }
        }
    }
}


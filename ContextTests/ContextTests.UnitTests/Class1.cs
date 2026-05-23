using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextTests.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContextTests.UnitTests
{
    [TestClass]
    public class Class1
    {
        private static readonly DomainEvent<FeePaidOff> EventRegistar = new DomainEvent<FeePaidOff>();
        [TestMethod]
        public void TestOne()
        {
            var agr = new SomeAggregate();

            using (EventRegistar.Register(new FeePaidOffHandler(null)))
            {
                agr.DoSomething();
            }
            agr.DoSomething();
        }
        
    }

    public class SomeAggregate
    {
        DomainEvent<FeePaidOff> de = new DomainEvent<FeePaidOff>();
            
        public bool DoSomething()
        {
            de.Raise(new FeePaidOff()
                {
                    Fee = 67
                });
            return true;
        }
    }
    public class FeePaidOffHandler : IHandler<FeePaidOff>
    {
        private readonly IStudentRepository _customerRepository;

        public FeePaidOffHandler(IStudentRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public void Handle(FeePaidOff args)
        {
            var fee = args.Fee;
            Console.WriteLine("I was raised");
            //var customer = _customerRepository.GetAllStudents();

        }
    }

    public class FeePaidOff:IDomainEvent
    {
        public decimal Fee { get; set; }
    }

    public interface IDomainEvent
    {
    }

    public interface IHandler<E> where E:IDomainEvent 
    {
        void Handle(E args);
    }

    public class DomainEventRegistrationRemover : IDisposable
    {
        private readonly Action CallOnDispose;
        
        public DomainEventRegistrationRemover(Action ToCall)
        {
            this.CallOnDispose = ToCall;
        }
        
        public void Dispose()
        {
            this.CallOnDispose.DynamicInvoke();
        }
    }

    public class DomainEvent<E> where E:IDomainEvent
    {
        [ThreadStatic]
        private static List<IHandler<E>> _actions;

        protected List<IHandler<E>> actions
        {
            get
            {
                if (_actions == null)
                    _actions = new List<IHandler<E>>();

                return _actions;
            }
        }

        public IDisposable Register(IHandler<E> callback)
        {
            actions.Add(callback);
            return new DomainEventRegistrationRemover(() => actions.Remove(callback));
        }
        
        public void Raise(E args)
        {
            var handled = false;
            foreach (var action in actions)
            {
                action.Handle(args);
                handled = true;

            }
            if (!handled) Console.WriteLine("Unhandled event raised");

        }
    }
}

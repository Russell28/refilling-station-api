using RefillingStation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Tests.Domain.Builders
{
    public class CustomerBuilder
    {
        private string _name = "Customer";

        public CustomerBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public Customer Build()
        {
            var customer = new Customer
            {
                Name = _name
            };

            return customer;
        }
    }
}

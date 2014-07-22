using System;
using System.Diagnostics.Contracts;

namespace BlackFox.Roslyn.Diagnostics
{
    public static class Parameter
    {
        [ContractArgumentValidator]
        public static void MustNotBeNull<TParameter>(TParameter value, string name)
            where TParameter : class
        {
            if (value == null)
            {
                throw new ArgumentNullException("name");
            }

            Contract.EndContractBlock();
        }

        [ContractArgumentValidator]
        public static TCast MustBeOfType<TCast>(object value, string name)
            where TCast : class
        {
            var result = value as TCast;

            if (result == null)
            {
                var valueType = value?.GetType()?.FullName ?? "<null>";
                var message = string.Format("Parameter {0} is expected to be of type {1} but was of type {2}",
                    name, typeof(TCast).FullName, valueType);
                throw new ArgumentException(message, name);
            }

            Contract.EndContractBlock();

            return result;
        }
    }
}

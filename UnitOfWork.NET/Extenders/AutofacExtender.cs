using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Builder;
using UnitOfWork.NET.Interfaces;

namespace UnitOfWork.NET.Extenders
{
    internal static class AutofacExtender
    {
        public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> AsRepository<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration, Type sourceType = null, Type destinationType = null)
        {
            var res = registration.As<IRepository>();

            if (sourceType != null)
            {
                res = res.As(typeof(IRepository<>).MakeGenericType(sourceType));

                if (destinationType != null)
                    res = res.As(typeof(IRepository<,>).MakeGenericType(sourceType, destinationType));
            }

            return res;
        }


        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> AsRepository<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration, Type sourceType = null, Type destinationType = null) where TConcreteActivatorData : IConcreteActivatorData
        {
            var res = registration.As<IRepository>();

            if (sourceType != null)
            {
                res = res.As(typeof(IRepository<>).MakeGenericType(sourceType));

                if (destinationType != null)
                    res = res.As(typeof(IRepository<,>).MakeGenericType(sourceType, destinationType));
            }

            return res;
        }
    }
}

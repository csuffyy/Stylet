﻿using System;
using System.Linq.Expressions;

namespace StyletIoC.Creation
{
    /// <summary>
    /// Delegate used to create an IRegistration
    /// </summary>
    /// <param name="parentContext">Context on which this registration will be created</param>
    /// <param name="serviceType">Service type for this registration</param>
    /// <param name="creator">ICreator used by the IRegistration to create new instances</param>
    /// <param name="key">Key associated with the registration</param>
    /// <returns>A new IRegistration</returns>
    public delegate IRegistration RegistrationFactory(IRegistrationContext parentContext, Type serviceType, ICreator creator, string key);

    /// <summary>
    /// An IRegistration is responsible to returning an appropriate (new or cached) instanced of a type, or an expression doing the same.
    /// It owns an ICreator, and will use it to create a new instance when needed.
    /// </summary>
    public interface IRegistration
    {
        /// <summary>
        /// Gets the type of the object returned by the registration
        /// </summary>
        RuntimeTypeHandle TypeHandle { get; }

        /// <summary>
        /// Fetches an instance of the relevaent type
        /// </summary>
        /// <returns>An object of type Type, which is supplied by the ICreator</returns>
        Func<IRegistrationContext, object> GetGenerator();

        /// <summary>
        /// Fetches an expression which evaluates to an instance of the relevant type
        /// </summary>
        /// <param name="registrationContext">Context which calls this method</param>
        /// <returns>An expression evaluating to an instance of type Type, which is supplied by the ICreator></returns>
        Expression GetInstanceExpression(ParameterExpression registrationContext);
    }
}

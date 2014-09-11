﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

using System;
using System.Linq;

namespace Remote.Linq.TypeSystem
{
    public partial class TypeResolver : ITypeResolver
    {
        private static readonly ITypeResolver _defaultTypeResolver = new TypeResolver();

        protected TypeResolver()
        {
        }

        public static ITypeResolver Instance
        {
            get { return _instance ?? _defaultTypeResolver; }
            set { _instance = value; }
        }
        private static ITypeResolver _instance;

        protected virtual Type ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) throw new ArgumentNullException("typeName", "Expected a valid type name");

            var type = Type.GetType(typeName);
            if (ReferenceEquals(null, type))
            {
                var assemblies = GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(typeName);
                    if (!ReferenceEquals(null, type)) break;
                }
            }
            return type;
        }

        public virtual Type ResolveType(TypeInfo typeInfo)
        {
#if NET
            Type type;
            if (typeInfo.IsAnonymousType)
            {
                type = new Remote.Linq.TypeSystem.Emit.TypeEmitter().EmitType(typeInfo);
            }
            else
            {
                type = ResolveType(typeInfo.FullName);
            }
#else
            Type type = ResolveType(typeInfo.FullName);

            if (!ReferenceEquals(null, type) && (type.IsAnonymousType() || typeInfo.IsAnonymousType))
            {
                var properties = type.GetProperties().Select(x => x.Name).ToList();
                var propertyNames = typeInfo.Properties;

                var match =
                    type.IsAnonymousType() &&
                    typeInfo.IsAnonymousType &&
                    properties.Count == propertyNames.Count &&
                    propertyNames.All(x => properties.Contains(x));

                if (!match)
                {
                    throw new Exception(string.Format("Anonymous type '{0}' could not be resolved, expecting properties: {1}", typeInfo.FullName, string.Join(", ", propertyNames.ToArray())));
                }
            }
#endif

            if (ReferenceEquals(null, type))
            {
                throw new Exception(string.Format("Type '{0}' could not be resolved", typeInfo.FullName));
            }

            if (typeInfo.IsGenericType)
            {
                var generigArguments =
                    from x in typeInfo.GenericArguments
                    select ResolveType(x);
                type = type.MakeGenericType(generigArguments.ToArray());
            }
            return type;
        }
    }
}
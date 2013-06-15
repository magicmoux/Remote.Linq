﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

using System;

namespace Remote.Linq.Expressions
{
    [Serializable]
    public enum ExpressionType
    {
        PropertyAccess,
        ConstantValue,
        Conversion,
        Binary,
        Unary,
        Collection,
        Parameter,
        MethodCall,
        Sort,
        Lambda,
    }
}
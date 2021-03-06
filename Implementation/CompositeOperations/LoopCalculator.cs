﻿using System;
using System.Collections.Generic;
using System.Linq;
using MilpManager.Abstraction;

namespace MilpManager.Implementation.CompositeOperations
{
    public class LoopCalculator : ICompositeOperationCalculator
    {
        public bool SupportsOperation(CompositeOperationType type, ICompositeOperationParameters parameters,
            params IVariable[] arguments)
        {
            return type == CompositeOperationType.Loop && (parameters as LoopParameters)?.Body?.Length == arguments.Length;
        }

        public IEnumerable<IVariable> Calculate(IMilpManager milpManager, CompositeOperationType type, ICompositeOperationParameters parameters,
            params IVariable[] arguments)
        {
            if (!SupportsOperation(type, parameters, arguments)) throw new NotSupportedException(SolverUtilities.FormatUnsupportedMessage(type, parameters, arguments));
            var options = parameters as LoopParameters;

            var totalBound = milpManager.CreateAnonymous(Domain.PositiveOrZeroInteger);
            totalBound.Set(ConstraintType.LessOrEqual, milpManager.FromConstant(options.MaxIterations));

            options.BeforeLoopAction(totalBound, arguments);

            for (int i = 1; i <= options.MaxIterations; ++i)
            {
                var counter = milpManager.FromConstant(i);
                var isLooping = counter.Operation(OperationType.IsLessOrEqual, totalBound);

                options.BeforeIterationAction(counter, isLooping, totalBound, arguments);

                for (int v = 0; v < arguments.Length; ++v)
                {
                    if (options.BeforeBody.Length > v)
                    {
                        options.BeforeBody[v](arguments[v], counter, isLooping, totalBound, arguments);
                    }

                    arguments[v] = milpManager.Operation(OperationType.Condition, isLooping, options.Body[v](arguments[v], counter, isLooping, totalBound, arguments), arguments[v]);

                    if (options.AfterBody.Length > v)
                    {
                        options.AfterBody[v](arguments[v], counter, isLooping, totalBound, arguments);
                    }
                }

                options.AfterIterationAction(counter, isLooping, totalBound, arguments);
            }

            options.AfterLoopAction(totalBound, arguments);

            return arguments.Concat(new[] {totalBound}).ToArray();
        }
    }
}
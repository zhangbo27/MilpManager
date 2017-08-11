﻿using System;
using System.Linq;
using MilpManager.Abstraction;

namespace MilpManager.Implementation.Operations
{
	public abstract class BaseOperationCalculator : IOperationCalculator
	{

		public bool SupportsOperation<TOperationType>(params IVariable[] arguments) where TOperationType : OperationType
		{
			return SupportedTypes.Contains(typeof (TOperationType)) && SupportsOperationInternal<TOperationType>(arguments);
		}

		public IVariable Calculate<TOperationType>(IMilpManager milpManager, params IVariable[] arguments) where TOperationType : OperationType
		{
			if (!SupportsOperation<TOperationType>(arguments)) throw new NotSupportedException(SolverUtilities.FormatUnsupportedMessage(typeof(TOperationType), arguments));

			return arguments.All(x => x.IsConstant())
				? CalculateConstantInternal<TOperationType>(milpManager, arguments)
				: CalculateInternal<TOperationType>(milpManager, arguments);
		}

		protected abstract bool SupportsOperationInternal<TOperationType>(params IVariable[] arguments)
			where TOperationType : OperationType;

		protected abstract IVariable CalculateInternal<TOperationType>(IMilpManager milpManager, params IVariable[] arguments)
			where TOperationType : OperationType;

		protected abstract IVariable CalculateConstantInternal<TOperationType>(IMilpManager milpManager, params IVariable[] arguments)
			where TOperationType : OperationType;

		protected abstract Type[] SupportedTypes { get; }
	}
}
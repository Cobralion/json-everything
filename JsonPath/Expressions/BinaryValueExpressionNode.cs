﻿using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryValueExpressionNode : ValueExpressionNode
{
	public IBinaryValueOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; set; }
	public int NestLevel { get; }

	public int Precedence => NestLevel * 10 + Operator.Precedence;

	public BinaryValueExpressionNode(IBinaryValueOperator op, ValueExpressionNode left, ValueExpressionNode right, int nestLevel)
	{
		Operator = op;
		Left = left;
		Right = right;
		NestLevel = nestLevel;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Left is BinaryValueExpressionNode lBin &&
		               (lBin.Precedence - Precedence) % 10 > 1;

		if (useGroup)
			builder.Append('(');
		Left.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	
		builder.Append(Operator);

		useGroup = Right is BinaryValueExpressionNode rBin &&
		           (rBin.Precedence - Precedence) % 10 > 1;
		if (useGroup)
			builder.Append('(');
		Right.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	}

	public override string ToString()
	{
		return $"{Left}{Operator}{Right}";
	}
}

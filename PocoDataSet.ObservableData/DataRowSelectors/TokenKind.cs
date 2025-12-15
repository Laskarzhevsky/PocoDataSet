namespace PocoDataSet.ObservableData
{
    internal enum TokenKind
    {
        Identifier, String, Number,
        OpEq, OpNe, OpLike, OpIs, OpNull,
        LParen, RParen, And, Or, Not, Comma, End
    }
}

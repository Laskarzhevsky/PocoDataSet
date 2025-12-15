namespace PocoDataSet.ObservableData
{
    internal class Token
    {
        public TokenKind Kind;
        public string Text;
        public Token(TokenKind k, string t)
        {
            Kind = k;
            Text = t;
        }
    }
}

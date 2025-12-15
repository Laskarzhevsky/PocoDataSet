using System;

namespace PocoDataSet.ObservableData
{
    internal class Parser
    {
        readonly Lexer _lex;
        Token _t;
        bool _caseSensitive;

        public Parser(string s, bool caseSensitive)
        {
            _lex = new Lexer(s);
            _t = _lex.Next();
            _caseSensitive = caseSensitive;
        }

        public IRowFilter ParseExpression()
        {
            // OR has lowest precedence
            IRowFilter left = ParseAnd();
            while (_t.Kind == TokenKind.Or)
            {
                Consume(TokenKind.Or);
                IRowFilter right = ParseAnd();
                left = new OrFilter(left, right);
            }
            return left;
        }

        IRowFilter ParseAnd()
        {
            IRowFilter left = ParseNot();
            while (_t.Kind == TokenKind.And)
            {
                Consume(TokenKind.And);
                IRowFilter right = ParseNot();
                left = new AndFilter(left, right);
            }
            return left;
        }

        IRowFilter ParseNot()
        {
            if (_t.Kind == TokenKind.Not)
            {
                Consume(TokenKind.Not);
                IRowFilter inner = ParsePrimary();
                return new NotFilter(inner);
            }
            return ParsePrimary();
        }

        IRowFilter ParsePrimary()
        {
            if (_t.Kind == TokenKind.LParen)
            {
                Consume(TokenKind.LParen);
                IRowFilter e = ParseExpression();
                Consume(TokenKind.RParen);
                return e;
            }

            // Comparison
            string column = ExpectIdentifier();
            if (_t.Kind == TokenKind.OpIs)
            {
                Consume(TokenKind.OpIs);
                bool neg = false;
                if (_t.Kind == TokenKind.Not)
                {
                    Consume(TokenKind.Not);
                    neg = true;
                }
                Consume(TokenKind.OpNull);
                if (neg)
                    return new IsNullFilter(column, false);
                return new IsNullFilter(column, true);
            }

            if (_t.Kind == TokenKind.OpLike)
            {
                Consume(TokenKind.OpLike);
                string pat = ExpectString();
                return new LikeFilter(column, pat, _caseSensitive);
            }

            if (_t.Kind == TokenKind.OpEq || _t.Kind == TokenKind.OpNe)
            {
                bool isEq = (_t.Kind == TokenKind.OpEq);
                Consume(_t.Kind);
                object value = ReadValue();
                return new CompareFilter(column, isEq, value, _caseSensitive);
            }

            throw new ArgumentException("Expected operator after identifier.");
        }

        public void ExpectEnd()
        {
            if (_t.Kind != TokenKind.End)
            {
                throw new ArgumentException("Unexpected trailing input near: " + _t.Text);
            }
        }

        string ExpectIdentifier()
        {
            if (_t.Kind != TokenKind.Identifier)
            {
                throw new ArgumentException("Identifier expected.");
            }
            string s = _t.Text;
            _t = _lex.Next();
            return s;
        }

        string ExpectString()
        {
            if (_t.Kind != TokenKind.String)
            {
                throw new ArgumentException("String literal expected.");
            }
            string s = _t.Text;
            _t = _lex.Next();
            return s;
        }

        object ReadValue()
        {
            if (_t.Kind == TokenKind.String)
            {
                string s = _t.Text;
                _t = _lex.Next();
                return s;
            }
            if (_t.Kind == TokenKind.Number)
            {
                string n = _t.Text;
                _t = _lex.Next();
                // keep as string; compare numerically later when possible
                return n;
            }
            if (_t.Kind == TokenKind.Identifier)
            {
                // allow bare identifiers as values (e.g., TRUE/FALSE or unquoted words)
                string s = _t.Text;
                _t = _lex.Next();
                return s;
            }
            throw new ArgumentException("Value expected.");
        }

        void Consume(TokenKind k)
        {
            if (_t.Kind != k)
            {
                throw new ArgumentException("Unexpected token: " + _t.Text);
            }
            _t = _lex.Next();
        }
    }
}

using System;

namespace PocoDataSet.ObservableData
{
    // ------------- Lexer/Parser (supports AND/OR/NOT, =, !=, <>, LIKE, IS NULL) -------------
    internal class Lexer
    {
        readonly string _s;
        int _i, _n;
        public Lexer(string s)
        {
            _s = s ?? string.Empty;
            _n = _s.Length;
            _i = 0;
        }

        public Token Next()
        {
            SkipWs();
            if (_i >= _n)
                return new Token(TokenKind.End, string.Empty);

            char c = _s[_i];

            if (c == '(')
            {
                _i++;
                return new Token(TokenKind.LParen, "(");
            }
            if (c == ')')
            {
                _i++;
                return new Token(TokenKind.RParen, ")");
            }
            if (c == ',')
            {
                _i++;
                return new Token(TokenKind.Comma, ",");
            }

            if (c == '\'')
            {
                // string literal with '' escape
                _i++;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                while (_i < _n)
                {
                    char ch = _s[_i++];
                    if (ch == '\'')
                    {
                        if (_i < _n && _s[_i] == '\'')
                        {
                            sb.Append('\'');
                            _i++;
                            continue;
                        }
                        return new Token(TokenKind.String, sb.ToString());
                    }
                    sb.Append(ch);
                }
                throw new ArgumentException("Unterminated string literal.");
            }

            if (char.IsDigit(c))
            {
                int start = _i;
                while (_i < _n && (char.IsDigit(_s[_i]) || _s[_i] == '.' || _s[_i] == '+' || _s[_i] == '-'))
                    _i++;
                return new Token(TokenKind.Number, _s.Substring(start, _i - start));
            }

            if (IsIdentStart(c))
            {
                int start = _i;
                _i++;
                while (_i < _n && IsIdentPart(_s[_i]))
                    _i++;
                string ident = _s.Substring(start, _i - start);
                string up = ident.ToUpperInvariant();

                if (up == "AND")
                    return new Token(TokenKind.And, ident);
                if (up == "OR")
                    return new Token(TokenKind.Or, ident);
                if (up == "NOT")
                    return new Token(TokenKind.Not, ident);
                if (up == "LIKE")
                    return new Token(TokenKind.OpLike, ident);
                if (up == "IS")
                    return new Token(TokenKind.OpIs, ident);
                if (up == "NULL")
                    return new Token(TokenKind.OpNull, ident);

                return new Token(TokenKind.Identifier, ident);
            }

            // operators: =, !=, <>
            if (c == '=')
            {
                _i++;
                return new Token(TokenKind.OpEq, "=");
            }
            if ((c == '!' || c == '<') && _i + 1 < _n && _s[_i + 1] == '>')
            {
                _i += 2;
                return new Token(TokenKind.OpNe, "<>");
            }
            if (c == '!' && _i + 1 < _n && _s[_i + 1] == '=')
            {
                _i += 2;
                return new Token(TokenKind.OpNe, "!=");
            }

            throw new ArgumentException("Unexpected character: " + c);
        }

        void SkipWs()
        {
            while (_i < _n && char.IsWhiteSpace(_s[_i]))
                _i++;
        }

        static bool IsIdentStart(char c)
        {
            return char.IsLetter(c) || c == '_' || c == '$';
        }

        static bool IsIdentPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '$';
        }
    }
}
